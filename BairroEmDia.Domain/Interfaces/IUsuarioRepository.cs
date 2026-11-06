using BairroEmDia.Domain.Entities;

namespace BairroEmDia.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Usuario>> ListarAsync(
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(
        Usuario usuario,
        CancellationToken cancellationToken = default);
}