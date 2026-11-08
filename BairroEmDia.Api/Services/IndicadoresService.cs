using BairroEmDia.Api.DTOs;
using BairroEmDia.Domain.Enums;
using BairroEmDia.Domain.Interfaces;

namespace BairroEmDia.Api.Services;

public class IndicadoresService : IIndicadoresService
{
    private readonly IOcorrenciaRepository _ocorrenciaRepository;

    public IndicadoresService(
        IOcorrenciaRepository ocorrenciaRepository)
    {
        _ocorrenciaRepository = ocorrenciaRepository;
    }

    public async Task<IndicadoresResponse> ObterAsync(
        string? bairro = null,
        CancellationToken cancellationToken = default)
    {
        var bairroFiltrado = string.IsNullOrWhiteSpace(bairro)
            ? null
            : bairro.Trim();

        var ocorrencias = await _ocorrenciaRepository.ListarAsync(
            bairro: bairroFiltrado,
            cancellationToken: cancellationToken);

        var encerradas = ocorrencias.Count(ocorrencia =>
            ocorrencia.Status == StatusOcorrencia.Encerrada);

        var semMudancaDeStatus = ocorrencias.Count(ocorrencia =>
            ocorrencia.Status == StatusOcorrencia.Registrada);

        var porCategoria = Enum.GetValues<CategoriaOcorrencia>()
            .Select(categoria => new TotalPorCategoriaResponse(
                categoria,
                ocorrencias.Count(ocorrencia =>
                    ocorrencia.Categoria == categoria)))
            .OrderByDescending(indicador => indicador.Total)
            .ThenBy(indicador => indicador.Categoria)
            .ToList();

        var porStatus = Enum.GetValues<StatusOcorrencia>()
            .Select(status => new TotalPorStatusResponse(
                status,
                ocorrencias.Count(ocorrencia =>
                    ocorrencia.Status == status)))
            .ToList();

        return new IndicadoresResponse(
            bairroFiltrado,
            ocorrencias.Count,
            ocorrencias.Count - encerradas,
            encerradas,
            semMudancaDeStatus,
            porCategoria,
            porStatus);
    }
}