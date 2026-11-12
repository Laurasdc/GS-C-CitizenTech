using BairroEmDia.Domain.Entities;
using BairroEmDia.Domain.Enums;
using BairroEmDia.Domain.Exceptions;
using BairroEmDia.Infrastructure.Data;
using BairroEmDia.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace BairroEmDia.Tests;

public class LocalidadesTests
{
    [Fact]
    public void Criar_DeveNormalizarCidadeEUf()
    {
        var ocorrencia = Criar(Guid.NewGuid(), "  Campinas  ", " sp ");
        Assert.Equal("Campinas", ocorrencia.Cidade);
        Assert.Equal("SP", ocorrencia.Uf);
    }

    [Theory]
    [InlineData("", "SP")]
    [InlineData("   ", "SP")]
    [InlineData("Campinas", "")]
    [InlineData("Campinas", "XX")]
    [InlineData("Campinas", "SPO")]
    public void Criar_ComLocalidadeInvalida_DeveRecusar(string cidade, string uf)
    {
        Assert.Throws<RegraDeNegocioException>(() => Criar(Guid.NewGuid(), cidade, uf));
    }

    [Fact]
    public async Task Repositorio_DeveSepararMesmoBairroEmCidadesDiferentes()
    {
        await using var conexao = new SqliteConnection("Data Source=:memory:");
        await conexao.OpenAsync();
        var options = new DbContextOptionsBuilder<BairroEmDiaDbContext>()
            .UseSqlite(conexao).Options;
        await using var context = new BairroEmDiaDbContext(options);
        await context.Database.MigrateAsync();
        var usuario = new Usuario("Morador de teste");
        context.Usuarios.Add(usuario);
        context.Ocorrencias.AddRange(
            Criar(usuario.Id, "São Paulo", "SP"),
            Criar(usuario.Id, "Campinas", "SP"),
            Criar(usuario.Id, "Rio de Janeiro", "RJ"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repository = new OcorrenciaRepository(context);

        var campinas = await repository.ListarAsync(bairro: "Centro", cidade: "campinas", uf: " sp ");
        Assert.Equal("Campinas", Assert.Single(campinas).Cidade);
        var estado = await repository.ListarAsync(uf: "SP");
        Assert.Equal(2, estado.Count);
        var rio = await repository.ListarAsync(bairro: "Centro", cidade: "Rio de Janeiro", uf: "RJ");
        Assert.Equal("RJ", Assert.Single(rio).Uf);
        Assert.Empty(await repository.ListarAsync(cidade: "Campinas", uf: "RJ"));
    }

    [Fact]
    public async Task Migration_DevePreservarOcorrenciaAntigaComoSaoPauloSP()
    {
        await using var conexao = new SqliteConnection("Data Source=:memory:");
        await conexao.OpenAsync();
        var options = new DbContextOptionsBuilder<BairroEmDiaDbContext>()
            .UseSqlite(conexao).Options;
        await using var context = new BairroEmDiaDbContext(options);
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync("20261106152540_Inicial");
        var usuarioId = Guid.NewGuid();
        var ocorrenciaId = Guid.NewGuid();
        var agora = DateTime.UtcNow;
        await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO Usuarios (Id, Nome, CriadoEm) VALUES ({usuarioId}, {"Morador antigo"}, {agora})");
        await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO Ocorrencias (Id, UsuarioId, Titulo, Descricao, Bairro, Localizacao, Categoria, Status, CriadoEm, AtualizadoEm) VALUES ({ocorrenciaId}, {usuarioId}, {"Título antigo"}, {"Descrição antiga"}, {"Chácara Klabin"}, {"Local antigo"}, {"Iluminacao"}, {"Registrada"}, {agora}, {agora})");

        await migrator.MigrateAsync();
        var antiga = await context.Ocorrencias.SingleAsync();
        Assert.Equal(ocorrenciaId, antiga.Id);
        Assert.Equal("Título antigo", antiga.Titulo);
        Assert.Equal("Chácara Klabin", antiga.Bairro);
        Assert.Equal("São Paulo", antiga.Cidade);
        Assert.Equal("SP", antiga.Uf);
    }

    private static Ocorrencia Criar(Guid usuarioId, string cidade, string uf)
        => new(usuarioId, "Ocorrência de teste", "Descrição de teste", "Centro",
            "Rua de teste, 100", CategoriaOcorrencia.Iluminacao, cidade, uf);
}
