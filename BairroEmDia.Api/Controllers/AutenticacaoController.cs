using System.Security.Claims;
using BairroEmDia.Api.Authentication;
using BairroEmDia.Api.DTOs;
using BairroEmDia.Api.Services;
using BairroEmDia.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace BairroEmDia.Api.Controllers;

[ApiController, Route("api/auth")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AutenticacaoController(AutenticacaoService service, IAutenticacaoRepository repository, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("registrar"), AllowAnonymous, EnableRateLimiting("autenticacao")]
    public async Task<IActionResult> Registrar(RegistrarContaRequest request, CancellationToken ct)
    {
        bool criada;
        try { criada = await service.RegistrarAsync(request, ct); }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 }) { criada = false; }
        return criada ? StatusCode(201, new { mensagem = "Conta criada. Entre com seu e-mail e senha." })
            : Conflict(new ProblemDetails { Status = 409, Title = "Conta não criada", Detail = "Já existe uma conta com esse e-mail." });
    }
    [HttpPost("entrar"), AllowAnonymous, EnableRateLimiting("autenticacao")]
    public async Task<IActionResult> Entrar(EntrarRequest request, [FromQuery] bool usarCookie = false, CancellationToken ct = default)
    {
        var sessao = await service.EntrarAsync(request, ct);
        if (sessao is null) return Unauthorized(new ProblemDetails { Status = 401, Title = "Não foi possível entrar", Detail = "E-mail ou senha incorretos." });
        if (usarCookie)
        {
            Response.Cookies.Append(SessaoAuthenticationHandler.CookieName, sessao.Token!, CookieOptions(sessao.ExpiraEm));
            sessao = sessao with { Token = null };
        }
        return Ok(sessao);
    }
    [HttpGet("me"), Authorize]
    public IActionResult Me() => Ok(new ContaResponse(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), User.Identity!.Name!, User.FindFirstValue(ClaimTypes.Email)!));
    [HttpPost("sair"), Authorize]
    public async Task<IActionResult> Sair(CancellationToken ct)
    {
        await repository.RevogarSessaoAsync(Guid.Parse(User.FindFirstValue("session_id")!), ct);
        Response.Cookies.Delete(SessaoAuthenticationHandler.CookieName, CookieOptions(null));
        return NoContent();
    }
    private CookieOptions CookieOptions(DateTime? expiraEm) => new() {
        HttpOnly = true, Secure = !env.IsDevelopment(), SameSite = SameSiteMode.Lax,
        Path = "/api", Expires = expiraEm is null ? null : new DateTimeOffset(expiraEm.Value),
        IsEssential = true
    };
}
