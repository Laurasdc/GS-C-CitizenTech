using BairroEmDia.Domain.Entities;
using BairroEmDia.Domain.Interfaces;
using BairroEmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BairroEmDia.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly BairroEmDiaDbContext _context;

    public UsuarioRepository(BairroEmDiaDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                usuario => usuario.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Usuario>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(
        Usuario usuario,
        CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(
            usuario,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}