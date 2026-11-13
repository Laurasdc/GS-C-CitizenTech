using System.Security.Claims;
using System.Text.Encodings.Web;
using BairroEmDia.Api.Services;
using BairroEmDia.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
namespace BairroEmDia.Api.Authentication;

public class SessaoAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
    UrlEncoder encoder, IAutenticacaoRepository repository, TimeProvider time)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Sessao";
    public const string CookieName = "BairroEmDia.Sessao";
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? token = null;
        if (Request.Headers.TryGetValue("Authorization", out var authorization))
        {
            var valor = authorization.ToString();
            if (!valor.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return AuthenticateResult.Fail("Credencial inválida.");
            token = valor[7..];
        }
        else Request.Cookies.TryGetValue(CookieName, out token);
        if (string.IsNullOrEmpty(token)) return AuthenticateResult.NoResult();
        if (token.Length != 64 || !token.All(Uri.IsHexDigit)) return AuthenticateResult.Fail("Sessão inválida.");
        var dados = await repository.ObterSessaoAsync(AutenticacaoService.HashToken(token), Context.RequestAborted);
        if (dados is null || dados.Value.Sessao.ExpiraEm <= time.GetUtcNow().UtcDateTime) return AuthenticateResult.Fail("Sessão encerrada.");
        var (sessao, conta, usuario) = dados.Value;
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome), new Claim(ClaimTypes.Email, conta.Email),
            new Claim(ClaimTypes.Role, conta.Perfil), new Claim("session_id", sessao.Id.ToString())
        };
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName)), SchemeName));
    }
}
