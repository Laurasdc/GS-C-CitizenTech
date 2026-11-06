using BairroEmDia.Domain.Enums;

namespace BairroEmDia.Api.DTOs;

public record OcorrenciaResumoResponse(
    Guid Id,
    string Titulo,
    string Bairro,
    CategoriaOcorrencia Categoria,
    StatusOcorrencia Status,
    DateTime CriadoEm,
    DateTime AtualizadoEm);