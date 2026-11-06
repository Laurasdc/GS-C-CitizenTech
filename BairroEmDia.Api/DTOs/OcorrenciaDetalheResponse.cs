using BairroEmDia.Domain.Enums;

namespace BairroEmDia.Api.DTOs;

public record OcorrenciaDetalheResponse(
    Guid Id,
    Guid UsuarioId,
    string Titulo,
    string Descricao,
    string Bairro,
    string Localizacao,
    CategoriaOcorrencia Categoria,
    StatusOcorrencia Status,
    DateTime CriadoEm,
    DateTime AtualizadoEm,
    int TotalConfirmacoes,
    IReadOnlyList<HistoricoAtualizacaoResponse> Historico);