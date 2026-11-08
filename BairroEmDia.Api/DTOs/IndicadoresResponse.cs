using BairroEmDia.Domain.Enums;

namespace BairroEmDia.Api.DTOs;

public record IndicadoresResponse(
    string? Bairro,
    int TotalOcorrencias,
    int OcorrenciasAbertas,
    int OcorrenciasEncerradas,
    int SemMudancaDeStatus,
    IReadOnlyList<TotalPorCategoriaResponse> PorCategoria,
    IReadOnlyList<TotalPorStatusResponse> PorStatus);

public record TotalPorCategoriaResponse(
    CategoriaOcorrencia Categoria,
    int Total);

public record TotalPorStatusResponse(
    StatusOcorrencia Status,
    int Total);