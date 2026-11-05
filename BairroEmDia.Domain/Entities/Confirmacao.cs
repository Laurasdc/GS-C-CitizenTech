namespace BairroEmDia.Domain.Entities;

public class Confirmacao
{
    public Guid Id { get; private set; }
    public Guid OcorrenciaId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Confirmacao()
    {
    }

    internal Confirmacao(Guid ocorrenciaId, Guid usuarioId)
    {
        Id = Guid.NewGuid();
        OcorrenciaId = ocorrenciaId;
        UsuarioId = usuarioId;
        CriadoEm = DateTime.UtcNow;
    }
}