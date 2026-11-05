namespace BairroEmDia.Domain.Exceptions;

public class RegraDeNegocioException : Exception
{
    public RegraDeNegocioException(string mensagem) : base(mensagem)
    {
    }
}