using BairroEmDia.Domain.Entities;
using BairroEmDia.Domain.Enums;
using BairroEmDia.Domain.Interfaces;
using BairroEmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BairroEmDia.Infrastructure.Repositories;

public class OcorrenciaRepository : IOcorrenciaRepository
{
    private readonly BairroEmDiaDbContext _context;

    public OcorrenciaRepository(BairroEmDiaDbContext context)
    {
        _context = context;
    }

    public async Task<Ocorrencia?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Ocorrencias
            .Include(ocorrencia => ocorrencia.Historico)
            .Include(ocorrencia => ocorrencia.Confirmacoes)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                ocorrencia => ocorrencia.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Ocorrencia>> ListarAsync(
        string? bairro = null,
        CategoriaOcorrencia? categoria = null,
        StatusOcorrencia? status = null,
        string? cidade = null,
        string? uf = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.Ocorrencias
            .AsNoTracking()
            .AsQueryable();

        var cidadeFiltrada = Localidade.NormalizarFiltro(cidade);
        var ufFiltrada = Localidade.NormalizarFiltro(uf);

        if (ufFiltrada is not null)
        {
            Localidade.ValidarUf(ufFiltrada);
            ufFiltrada = ufFiltrada.ToUpperInvariant();
            consulta = consulta.Where(o => o.Uf == ufFiltrada);
        }

        if (cidadeFiltrada is not null)
            consulta = consulta.Where(o =>
                EF.Functions.Collate(o.Cidade, "NOCASE") == cidadeFiltrada);

        if (!string.IsNullOrWhiteSpace(bairro))
        {
            var bairroFiltrado = bairro.Trim();

            consulta = consulta.Where(ocorrencia =>
                EF.Functions.Collate(ocorrencia.Bairro, "NOCASE")
                    == bairroFiltrado);
        }

        if (categoria.HasValue)
        {
            consulta = consulta.Where(ocorrencia =>
                ocorrencia.Categoria == categoria.Value);
        }

        if (status.HasValue)
        {
            consulta = consulta.Where(ocorrencia =>
                ocorrencia.Status == status.Value);
        }

        return await consulta
            .OrderByDescending(ocorrencia => ocorrencia.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(
        Ocorrencia ocorrencia,
        CancellationToken cancellationToken = default)
    {
        await _context.Ocorrencias.AddAsync(
            ocorrencia,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}