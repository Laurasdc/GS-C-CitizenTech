using BairroEmDia.Domain.Exceptions;

namespace BairroEmDia.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }

    // Utilizado pelo Entity Framework Core.
    private Usuario()
    {
    }

    public Usuario(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraDeNegocioException(
                "O nome do usuário é obrigatório.");

        var nomeNormalizado = nome.Trim();

        if (nomeNormalizado.Length > 100)
            throw new RegraDeNegocioException(
                "O nome deve ter no máximo 100 caracteres.");

        Id = Guid.NewGuid();
        Nome = nomeNormalizado;
        CriadoEm = DateTime.UtcNow;
    }
}