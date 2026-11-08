using BairroEmDia.Api.DTOs;

namespace BairroEmDia.Api.Services;

public interface IIndicadoresService
{
    Task<IndicadoresResponse> ObterAsync(
        string? bairro = null,
        CancellationToken cancellationToken = default);
}