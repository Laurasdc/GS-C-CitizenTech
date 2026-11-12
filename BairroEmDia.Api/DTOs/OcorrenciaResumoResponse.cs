using BairroEmDia.Domain.Enums;

namespace BairroEmDia.Api.DTOs;

public record OcorrenciaResumoResponse(
    Guid Id,
    string Titulo,
    string Bairro,
    string Cidade,
    string Uf,
    CategoriaOcorrencia Categoria,
    StatusOcorrencia Status,
    DateTime CriadoEm,
    DateTime AtualizadoEm);