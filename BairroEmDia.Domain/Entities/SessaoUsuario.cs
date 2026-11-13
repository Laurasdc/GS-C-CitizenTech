namespace BairroEmDia.Domain.Entities;

public class SessaoUsuario
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiraEm { get; private set; }
    private SessaoUsuario() { }
    public SessaoUsuario(Guid usuarioId, string tokenHash, DateTime expiraEm)
    { UsuarioId = usuarioId; TokenHash = tokenHash; ExpiraEm = expiraEm; }
}
