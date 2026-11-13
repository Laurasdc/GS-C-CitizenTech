using BairroEmDia.Domain.Entities;
using BairroEmDia.Domain.Interfaces;
using BairroEmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace BairroEmDia.Infrastructure.Repositories;

public class AutenticacaoRepository(BairroEmDiaDbContext db) : IAutenticacaoRepository
{
    public Task<ContaUsuario?> ObterContaAsync(string email, CancellationToken ct = default)
        => db.ContasUsuario.AsNoTracking().SingleOrDefaultAsync(c => c.Email == email, ct);
    public async Task CriarContaAsync(Usuario usuario, ContaUsuario conta, CancellationToken ct = default)
    {
        db.Usuarios.Add(usuario);
        db.ContasUsuario.Add(conta);
        await db.SaveChangesAsync(ct);
    }
    public async Task AdicionarSessaoAsync(SessaoUsuario sessao, CancellationToken ct = default)
    { db.SessoesUsuario.Add(sessao); await db.SaveChangesAsync(ct); }
    public async Task<(SessaoUsuario Sessao, ContaUsuario Conta, Usuario Usuario)?> ObterSessaoAsync(string hash, CancellationToken ct = default)
    {
        var sessao = await db.SessoesUsuario.AsNoTracking().SingleOrDefaultAsync(s => s.TokenHash == hash, ct);
        if (sessao is null) return null;
        var conta = await db.ContasUsuario.AsNoTracking().SingleOrDefaultAsync(c => c.UsuarioId == sessao.UsuarioId, ct);
        var usuario = await db.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.Id == sessao.UsuarioId, ct);
        return conta is null || usuario is null ? null : (sessao, conta, usuario);
    }
    public async Task RevogarSessaoAsync(Guid id, CancellationToken ct = default)
    {
        var sessao = await db.SessoesUsuario.FindAsync([id], ct);
        if (sessao is not null) { db.SessoesUsuario.Remove(sessao); await db.SaveChangesAsync(ct); }
    }
}
