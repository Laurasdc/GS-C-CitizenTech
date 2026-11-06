using BairroEmDia.Api.DTOs;

namespace BairroEmDia.Api.Services;

public interface IUsuarioService
{
    Task<UsuarioResponse> CriarAsync(
        CriarUsuarioRequest request,
        CancellationToken cancellationToken = default);

    Task<UsuarioResponse?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UsuarioResponse>> ListarAsync(
        CancellationToken cancellationToken = default);
}