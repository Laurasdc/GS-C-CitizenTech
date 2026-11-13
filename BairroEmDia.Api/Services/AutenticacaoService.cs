using System.Security.Cryptography;
using System.Text;
using BairroEmDia.Api.DTOs;
using BairroEmDia.Domain.Entities;
using BairroEmDia.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BairroEmDia.Api.Services;

public class AutenticacaoService(
    IAutenticacaoRepository repository,
    IUsuarioRepository usuarios,
    IPasswordHasher<ContaUsuario> hasher,
    TimeProvider time)
{
    public static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    // Hash com o mesmo custo para e-mails inexistentes: evita uma saída rápida na verificação.
    private static readonly ContaUsuario ContaInexistente = new(Guid.Empty, "", "");
    private static readonly string HashInexistente = new PasswordHasher<ContaUsuario>().HashPassword(ContaInexistente, Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));

    public async Task<bool> RegistrarAsync(RegistrarContaRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await repository.ObterContaAsync(email, ct) is not null) return false;
        var usuario = new Usuario(request.Nome);
        var provisoria = new ContaUsuario(usuario.Id, email, "");
        var conta = new ContaUsuario(usuario.Id, email, hasher.HashPassword(provisoria, request.Senha));
        await repository.CriarContaAsync(usuario, conta, ct);
        return true;
    }

    public async Task<SessaoResponse?> EntrarAsync(EntrarRequest request, CancellationToken ct)
    {
        var conta = await repository.ObterContaAsync(request.Email.Trim().ToLowerInvariant(), ct);
        var resultado = hasher.VerifyHashedPassword(conta ?? ContaInexistente, conta?.SenhaHash ?? HashInexistente, request.Senha);
        if (conta is null || resultado == PasswordVerificationResult.Failed) return null;
        var usuario = await usuarios.ObterPorIdAsync(conta.UsuarioId, ct);
        if (usuario is null) return null;
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiraEm = time.GetUtcNow().UtcDateTime.AddHours(8);
        await repository.AdicionarSessaoAsync(new SessaoUsuario(usuario.Id, HashToken(token), expiraEm), ct);
        return new SessaoResponse(new ContaResponse(usuario.Id, usuario.Nome, conta.Email), expiraEm, token);
    }
}
