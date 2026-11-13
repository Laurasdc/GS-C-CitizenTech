using BairroEmDia.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BairroEmDia.Api.Exceptions;

public class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(
        ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, titulo, detalhe) = exception switch
        {
            DbUpdateException { InnerException: SqliteException { SqliteExtendedErrorCode: 2067 } } => (
                StatusCodes.Status409Conflict,
                "Registro já existente",
                "Esta operação já foi registrada. Atualize a página antes de tentar novamente."),

            RecursoNaoEncontradoException => (
                StatusCodes.Status404NotFound,
                "Recurso não encontrado",
                exception.Message),

            RegraDeNegocioException => (
                StatusCodes.Status400BadRequest,
                "Regra de negócio não atendida",
                exception.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno",
                "Ocorreu um erro inesperado ao processar a solicitação.")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Erro inesperado ao processar {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = status;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = detalhe,
                Instance = httpContext.Request.Path.Value
            },
            cancellationToken: cancellationToken);

        return true;
    }
}