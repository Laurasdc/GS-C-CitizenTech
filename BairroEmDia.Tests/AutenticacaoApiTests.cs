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
}
