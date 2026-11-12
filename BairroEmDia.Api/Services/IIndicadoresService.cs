using BairroEmDia.Api.DTOs;

namespace BairroEmDia.Api.Services;

public interface IIndicadoresService
{
    Task<IndicadoresResponse> ObterAsync(
        string? bairro = null,
        string? cidade = null,
        string? uf = null,
        CancellationToken cancellationToken = default);
}