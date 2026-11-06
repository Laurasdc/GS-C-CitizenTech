namespace BairroEmDia.Domain.Exceptions;

public class RecursoNaoEncontradoException : Exception
{
    public RecursoNaoEncontradoException(string mensagem)
        : base(mensagem)
    {
    }
}