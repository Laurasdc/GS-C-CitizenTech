using BairroEmDia.Domain.Enums;
using BairroEmDia.Domain.Exceptions;

namespace BairroEmDia.Domain.Entities;

public class HistoricoAtualizacao
{
    public Guid Id { get; private set; }
    public Guid OcorrenciaId { get; private set; }
    public StatusOcorrencia StatusAnterior { get; private set; }
    public StatusOcorrencia NovoStatus { get; private set; }
    public string Justificativa { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }

    private HistoricoAtualizacao()
    {
    }

    internal HistoricoAtualizacao(
        Guid ocorrenciaId,
        StatusOcorrencia statusAnterior,
        StatusOcorrencia novoStatus,
        string justificativa)
    {
        if (string.IsNullOrWhiteSpace(justificativa))
            throw new RegraDeNegocioException(
                "A justificativa é obrigatória.");

        var texto = justificativa.Trim();

        if (texto.Length > 1000)
            throw new RegraDeNegocioException(
                "A justificativa deve ter no máximo 1000 caracteres.");

        Id = Guid.NewGuid();
        OcorrenciaId = ocorrenciaId;
        StatusAnterior = statusAnterior;
        NovoStatus = novoStatus;
        Justificativa = texto;
        CriadoEm = DateTime.UtcNow;
    }
}