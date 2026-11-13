namespace BairroEmDia.Domain.Entities;

public class ContaUsuario
{
    public Guid UsuarioId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public string Perfil { get; private set; } = "Morador";
    private ContaUsuario() { }
    public ContaUsuario(Guid usuarioId, string email, string senhaHash)
    {
        UsuarioId = usuarioId;
        Email = email;
        SenhaHash = senhaHash;
    }
}
