using BairroEmDia.Domain.Entities;
using BairroEmDia.Domain.Enums;

namespace BairroEmDia.Domain.Interfaces;

public interface IOcorrenciaRepository
{
    Task<Ocorrencia?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ocorrencia>> ListarAsync(
        string? bairro = null,
        CategoriaOcorrencia? categoria = null,
        StatusOcorrencia? status = null,
        string? cidade = null,
        string? uf = null,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(
        Ocorrencia ocorrencia,
        CancellationToken cancellationToken = default);

    Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default);
}