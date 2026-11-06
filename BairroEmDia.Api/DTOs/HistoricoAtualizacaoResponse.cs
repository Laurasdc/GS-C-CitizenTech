using BairroEmDia.Domain.Enums;

namespace BairroEmDia.Api.DTOs;

public record HistoricoAtualizacaoResponse(
    Guid Id,
    StatusOcorrencia StatusAnterior,
    StatusOcorrencia NovoStatus,
    string Justificativa,
    DateTime CriadoEm);