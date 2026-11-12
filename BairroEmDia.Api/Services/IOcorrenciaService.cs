using BairroEmDia.Api.DTOs;
using BairroEmDia.Domain.Enums;

namespace BairroEmDia.Api.Services;

public interface IOcorrenciaService
{
    Task<OcorrenciaDetalheResponse> CriarAsync(
        CriarOcorrenciaRequest request,
        CancellationToken cancellationToken = default);

    Task<OcorrenciaDetalheResponse> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OcorrenciaResumoResponse>> ListarAsync(
        string? bairro = null,
        CategoriaOcorrencia? categoria = null,
        StatusOcorrencia? status = null,
        string? cidade = null,
        string? uf = null,
        CancellationToken cancellationToken = default);

    Task<OcorrenciaDetalheResponse> EditarAsync(
        Guid id,
        EditarOcorrenciaRequest request,
        CancellationToken cancellationToken = default);

    Task<OcorrenciaDetalheResponse> AlterarStatusAsync(
        Guid id,
        AlterarStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<OcorrenciaDetalheResponse> ConfirmarAsync(
        Guid id,
        ConfirmarOcorrenciaRequest request,
        CancellationToken cancellationToken = default);
}