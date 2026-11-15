namespace BairroEmDia.Domain.Entities;

public class RecuperacaoSenha
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }
    public DateTime ExpiraEm { get; private set; }
    private RecuperacaoSenha() { }
    public RecuperacaoSenha(Guid usuarioId, string tokenHash, DateTime agora)
    {
        Id = Guid.NewGuid(); UsuarioId = usuarioId; TokenHash = tokenHash;
        CriadoEm = agora; ExpiraEm = agora.AddMinutes(30);
    }
}
