using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BairroEmDia.Api.DTOs;
using BairroEmDia.Api.Services;
using BairroEmDia.Domain.Entities;
using BairroEmDia.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BairroEmDia.Tests;

public class AutenticacaoApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
    private sealed class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public ApiFactory() => connection.Open();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services => {
                services.RemoveAll<DbContextOptions<BairroEmDiaDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<BairroEmDiaDbContext>>();
                services.AddDbContext<BairroEmDiaDbContext>(o => o.UseSqlite(connection));
            });
        }
        public HttpClient Cliente()
        {
            var client = CreateClient();
            using var scope = Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<BairroEmDiaDbContext>().Database.EnsureCreated();
            return client;
        }
        protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) connection.Dispose(); }
    }
    private static async Task<SessaoResponse> CriarEEntrar(HttpClient client, string email = "laura@example.com")
    {
        var criada = await client.PostAsJsonAsync("/api/auth/registrar", new { nome = "Laura", email, senha = "SenhaDeTeste123!" });
        Assert.Equal(HttpStatusCode.Created, criada.StatusCode);
        var login = await client.PostAsJsonAsync("/api/auth/entrar", new { email, senha = "SenhaDeTeste123!" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var sessao = (await login.Content.ReadFromJsonAsync<SessaoResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessao.Token);
        return sessao;
    }
    private static object Relato(Guid? usuarioId = null) => new { usuarioId, titulo = "Poste apagado", descricao = "Sem luz à noite", cidade = "São Paulo", uf = "SP", bairro = "Centro", localizacao = "Rua de teste, 10", categoria = "Iluminacao" };

    [Fact]
    public async Task ConsultaPublica_E_AcoesExigemSessao()
    {
        using var api = new ApiFactory(); using var client = api.Cliente();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/ocorrencias")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/ocorrencias", Relato())).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync($"/api/ocorrencias/{Guid.NewGuid()}/confirmacoes", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }
    [Fact]
    public async Task SenhaNaoFicaEmTexto_E_TokenNaoFicaNoBanco()
    {
        using var api = new ApiFactory(); using var client = api.Cliente();
        var sessao = await CriarEEntrar(client);
        using var scope = api.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<BairroEmDiaDbContext>();
        var conta = await db.ContasUsuario.SingleAsync(); var salva = await db.SessoesUsuario.SingleAsync();
        Assert.NotEqual("SenhaDeTeste123!", conta.SenhaHash);
        Assert.NotEqual(sessao.Token, salva.TokenHash);
        Assert.Equal(AutenticacaoService.HashToken(sessao.Token!), salva.TokenHash);
        Assert.Equal("Morador", conta.Perfil);
    }
    [Fact]
    public async Task LoginInvalidoNaoRevelaSeEmailExiste()
    {
        using var api = new ApiFactory(); using var client = api.Cliente(); await CriarEEntrar(client);
        foreach (var email in new[] { "laura@example.com", "outra@example.com" })
        {
            var response = await client.PostAsJsonAsync("/api/auth/entrar", new { email, senha = "SenhaErrada123" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Contains("E-mail ou senha incorretos.", await response.Content.ReadAsStringAsync());
        }
    }
    [Fact]
    public async Task EmailEhNormalizado_E_NaoAceitaDuplicata()
    {
        using var api = new ApiFactory(); using var client = api.Cliente(); await CriarEEntrar(client, "LAURA@example.com");
        var response = await client.PostAsJsonAsync("/api/auth/registrar", new { nome = "Outra", email = "laura@example.com", senha = "SenhaDeTeste123!" });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
    [Fact]
    public async Task LogoutRevogaTokenNoServidor()
    {
        using var api = new ApiFactory(); using var client = api.Cliente(); await CriarEEntrar(client);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/auth/sair", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }
    [Fact]
    public async Task SessaoExpiradaOuTokenAdulteradoNaoAutenticam()
    {
        using var api = new ApiFactory(); using var client = api.Cliente(); var sessao = await CriarEEntrar(client);
        var token = new string('A', 64);
        using (var scope = api.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BairroEmDiaDbContext>();
            db.SessoesUsuario.Add(new SessaoUsuario(sessao.Usuario.Id, AutenticacaoService.HashToken(token), DateTime.UtcNow.AddMinutes(-1)));
            await db.SaveChangesAsync();
        }
        foreach (var invalido in new[] { token, new string('B', 64), "invalido" })
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", invalido);
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
        }
    }
    [Fact]
    public async Task IdentidadeVemDaSessao_E_SoAutorEdita_E_GestorAlteraStatus()
    {
        using var api = new ApiFactory(); using var autor = api.Cliente(); var sessao = await CriarEEntrar(autor);
        var criada = await autor.PostAsJsonAsync("/api/ocorrencias", Relato(Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.Created, criada.StatusCode);
        var relato = (await criada.Content.ReadFromJsonAsync<OcorrenciaDetalheResponse>(JsonOptions))!;
        Assert.Equal(sessao.Usuario.Id, relato.UsuarioId);
        using var outro = api.CreateClient(); await CriarEEntrar(outro, "outro@example.com");
        Assert.Equal(HttpStatusCode.Forbidden, (await outro.PutAsJsonAsync($"/api/ocorrencias/{relato.Id}", Relato())).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await autor.PatchAsJsonAsync($"/api/ocorrencias/{relato.Id}/status", new { novoStatus = "EmAnalise", justificativa = "Analisar" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await autor.GetAsync("/api/usuarios")).StatusCode);
    }
    [Fact]
    public async Task ConfirmacaoUsaUsuarioLogado_E_ImpedeDuplicata()
    {
        using var api = new ApiFactory(); using var autor = api.Cliente(); await CriarEEntrar(autor);
        var criada = await autor.PostAsJsonAsync("/api/ocorrencias", Relato()); var relato = (await criada.Content.ReadFromJsonAsync<OcorrenciaDetalheResponse>(JsonOptions))!;
        using var outro = api.CreateClient(); await CriarEEntrar(outro, "vizinho@example.com");
        var url = $"/api/ocorrencias/{relato.Id}/confirmacoes";
        Assert.Equal(HttpStatusCode.OK, (await outro.PostAsync(url, null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await outro.PostAsync(url, null)).StatusCode);
        var estado = await outro.GetFromJsonAsync<JsonElement>($"/api/ocorrencias/{relato.Id}/minha-confirmacao");
        Assert.True(estado.GetProperty("confirmada").GetBoolean());
    }
    [Fact]
    public async Task CookieHttpOnly_NaoExpoeToken_E_BloqueiaOrigemEstranha()
    {
        using var api = new ApiFactory(); using var client = api.Cliente(); await CriarEEntrar(client);
        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.PostAsJsonAsync("/api/auth/entrar?usarCookie=true", new { email = "laura@example.com", senha = "SenhaDeTeste123!" });
        Assert.Contains("httponly", response.Headers.GetValues("Set-Cookie").Single().ToLowerInvariant());
        Assert.Null((await response.Content.ReadFromJsonAsync<SessaoResponse>())!.Token);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);
        client.DefaultRequestHeaders.Add("Origin", "https://malicioso.example");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync("/api/auth/sair", null)).StatusCode);
        client.DefaultRequestHeaders.Remove("Origin"); client.DefaultRequestHeaders.Add("Origin", "http://localhost:8081");
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/auth/sair", null)).StatusCode);
    }
    [Fact]
    public async Task CadastroValidaSenha_E_LimitaTentativas()
    {
        using var api = new ApiFactory(); using var client = api.Cliente();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/registrar", new { nome = "Laura", email = "laura@example.com", senha = "123" })).StatusCode);
        for (var i = 0; i < 10; i++) await client.PostAsJsonAsync("/api/auth/entrar", new { email = "nao@example.com", senha = "SenhaInvalida123" });
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsJsonAsync("/api/auth/entrar", new { email = "nao@example.com", senha = "SenhaInvalida123" })).StatusCode);
    }

    [Fact]
    public async Task MinhasExigeLogin_E_SeparaContas_MesmoComOutroIdNaQuery()
    {
        using var api = new ApiFactory(); using var autor = api.Cliente();
        Assert.Equal(HttpStatusCode.Unauthorized, (await autor.GetAsync("/api/ocorrencias/minhas")).StatusCode);
        var conta = await CriarEEntrar(autor);
        var criada = await autor.PostAsJsonAsync("/api/ocorrencias", Relato());
        var relato = (await criada.Content.ReadFromJsonAsync<OcorrenciaDetalheResponse>(JsonOptions))!;
        using var outra = api.CreateClient(); var outraConta = await CriarEEntrar(outra, "outra@example.com");
        Assert.Empty((await outra.GetFromJsonAsync<List<OcorrenciaResumoResponse>>("/api/ocorrencias/minhas", JsonOptions))!);
        await outra.PostAsJsonAsync("/api/ocorrencias", Relato());
        var resposta = await autor.GetAsync($"/api/ocorrencias/minhas?usuarioId={outraConta.Usuario.Id}");
        var minhas = (await resposta.Content.ReadFromJsonAsync<List<OcorrenciaResumoResponse>>(JsonOptions))!;
        Assert.Single(minhas); Assert.Equal(relato.Id, minhas[0].Id);
        Assert.True(resposta.Headers.CacheControl?.NoStore);
        Assert.Equal(2, (await autor.GetFromJsonAsync<List<OcorrenciaResumoResponse>>("/api/ocorrencias", JsonOptions))!.Count);
    }
    [Fact]
    public async Task AutorEditaMesmaOcorrencia_SemMudarIdentidadeStatusOuConfirmacoes()
    {
        using var api = new ApiFactory(); using var autor = api.Cliente(); var conta = await CriarEEntrar(autor);
        var criada = await autor.PostAsJsonAsync("/api/ocorrencias", Relato());
        var relato = (await criada.Content.ReadFromJsonAsync<OcorrenciaDetalheResponse>(JsonOptions))!;
        using var outra = api.CreateClient(); await CriarEEntrar(outra, "vizinha@example.com");
        await outra.PostAsync($"/api/ocorrencias/{relato.Id}/confirmacoes", null);
        var response = await autor.PutAsJsonAsync($"/api/ocorrencias/{relato.Id}", new {
            titulo = "Poste em frente ao mercado", descricao = "Local corrigido pelo autor",
            cidade = "Campinas", uf = "SP", bairro = "Centro", localizacao = "Rua correta, 50", categoria = "Iluminacao",
            usuarioId = Guid.NewGuid(), status = "Encerrada"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var salva = (await autor.GetFromJsonAsync<OcorrenciaDetalheResponse>($"/api/ocorrencias/{relato.Id}", JsonOptions))!;
        Assert.Equal(relato.Id, salva.Id); Assert.Equal(conta.Usuario.Id, salva.UsuarioId);
        Assert.Equal("Poste em frente ao mercado", salva.Titulo); Assert.Equal("Campinas", salva.Cidade);
        Assert.Equal(relato.Status, salva.Status); Assert.Equal(1, salva.TotalConfirmacoes);
        Assert.Equal(relato.Historico.Count, salva.Historico.Count);
        Assert.Equal(relato.CriadoEm, salva.CriadoEm);
    }
    [Fact]
    public async Task MesmoAutorNaoEditaOcorrenciaEncerrada()
    {
        using var api = new ApiFactory(); using var autor = api.Cliente(); await CriarEEntrar(autor);
        var criada = await autor.PostAsJsonAsync("/api/ocorrencias", Relato());
        var relato = (await criada.Content.ReadFromJsonAsync<OcorrenciaDetalheResponse>(JsonOptions))!;
        using (var scope = api.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BairroEmDiaDbContext>();
            var entity = await db.Ocorrencias.SingleAsync(o => o.Id == relato.Id);
            entity.AlterarStatus(BairroEmDia.Domain.Enums.StatusOcorrencia.Encerrada, "Encerrada pelo acompanhamento");
            await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.BadRequest, (await autor.PutAsJsonAsync($"/api/ocorrencias/{relato.Id}", Relato())).StatusCode);
        var salva = (await autor.GetFromJsonAsync<OcorrenciaDetalheResponse>($"/api/ocorrencias/{relato.Id}", JsonOptions))!;
        Assert.Equal(relato.Titulo, salva.Titulo);
        Assert.Equal(BairroEmDia.Domain.Enums.StatusOcorrencia.Encerrada, salva.Status);
    }

    [Fact]
    public async Task CadastroPublicoNaoConcedeGestorMesmoComPerfilForjado()
    {
        using var api = new ApiFactory(); using var client = api.Cliente();
        var register = await client.PostAsJsonAsync("/api/auth/registrar", new {
            nome = "Morador", email = "morador@example.com", senha = "SenhaDeTeste123!", perfil = "Gestor"
        });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        var login = await client.PostAsJsonAsync("/api/auth/entrar", new { email = "morador@example.com", senha = "SenhaDeTeste123!" });
        var session = (await login.Content.ReadFromJsonAsync<SessaoResponse>())!;
        Assert.Equal("Morador", session.Usuario.Perfil);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        var me = (await client.GetFromJsonAsync<ContaResponse>("/api/auth/me"))!;
        Assert.Equal("Morador", me.Perfil);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PatchAsJsonAsync($"/api/ocorrencias/{Guid.NewGuid()}/status", new { novoStatus = "EmAnalise", justificativa = "Tentativa indevida" })).StatusCode);
    }

    [Fact]
    public async Task ConfiguracaoLocalHabilitaSomenteContaExistente_E_RevogaSessoes()
    {
        using var api = new ApiFactory(); using var client = api.Cliente();
        var session = await CriarEEntrar(client);
        using var other = api.CreateClient(); await CriarEEntrar(other, "outra@example.com");
        Assert.Equal(1, await ConfigurarGestor.ExecutarAsync(api.Services, "ausente@example.com"));
        Assert.Equal(0, await ConfigurarGestor.ExecutarAsync(api.Services, " LAURA@EXAMPLE.COM "));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal("Morador", (await other.GetFromJsonAsync<ContaResponse>("/api/auth/me"))!.Perfil);
        using var scope = api.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<BairroEmDiaDbContext>();
        Assert.Equal(2, await db.ContasUsuario.CountAsync());
        Assert.Equal("Gestor", (await db.ContasUsuario.SingleAsync(c => c.UsuarioId == session.Usuario.Id)).Perfil);
        Assert.False(await db.SessoesUsuario.AnyAsync(s => s.UsuarioId == session.Usuario.Id));
    }

    [Fact]
    public async Task GestorPublicaHistorico_ValidaJustificativa_E_ReabreEncerrada()
    {
        using var api = new ApiFactory(); using var autor = api.Cliente(); var owner = await CriarEEntrar(autor);
        var created = await autor.PostAsJsonAsync("/api/ocorrencias", Relato());
        var initial = (await created.Content.ReadFromJsonAsync<OcorrenciaDetalheResponse>(JsonOptions))!;
        using var gestor = api.CreateClient(); await CriarEEntrar(gestor, "gestor@example.com");
        Assert.Equal(0, await ConfigurarGestor.ExecutarAsync(api.Services, "gestor@example.com"));
        var login = await gestor.PostAsJsonAsync("/api/auth/entrar", new { email = "gestor@example.com", senha = "SenhaDeTeste123!" });
        var session = (await login.Content.ReadFromJsonAsync<SessaoResponse>())!;
        Assert.Equal("Gestor", session.Usuario.Perfil);
        gestor.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        Assert.Equal("Gestor", (await gestor.GetFromJsonAsync<ContaResponse>("/api/auth/me"))!.Perfil);
        var url = $"/api/ocorrencias/{initial.Id}/status";
        Assert.Equal(HttpStatusCode.BadRequest, (await gestor.PatchAsJsonAsync(url, new { novoStatus = "EmAnalise", justificativa = "   " })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await gestor.PatchAsJsonAsync(url, new { novoStatus = "EmAnalise", justificativa = new string('a', 1001) })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await gestor.PatchAsJsonAsync(url, new { novoStatus = "EmAnalise", justificativa = "Verificação iniciada." })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await gestor.PatchAsJsonAsync(url, new { novoStatus = "EmAnalise", justificativa = "Não pode repetir." })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await gestor.PatchAsJsonAsync(url, new { novoStatus = "EmAcompanhamento", justificativa = "Aguardando providências." })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await gestor.PatchAsJsonAsync(url, new { novoStatus = "Encerrada", justificativa = "Problema resolvido." })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await gestor.PatchAsJsonAsync(url, new { novoStatus = "Registrada", justificativa = "Transição inválida." })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await gestor.PatchAsJsonAsync(url, new { novoStatus = "EmAnalise", justificativa = "O problema voltou." })).StatusCode);
        using var publico = api.CreateClient();
        var final = (await publico.GetFromJsonAsync<OcorrenciaDetalheResponse>($"/api/ocorrencias/{initial.Id}", JsonOptions))!;
        Assert.Equal(owner.Usuario.Id, final.UsuarioId); Assert.Equal(initial.Titulo, final.Titulo);
        Assert.Equal(4, final.Historico.Count);
        Assert.Equal(BairroEmDia.Domain.Enums.StatusOcorrencia.EmAnalise, final.Status);
        Assert.Contains(final.Historico, h => h.Justificativa == "O problema voltou.");
    }
}
