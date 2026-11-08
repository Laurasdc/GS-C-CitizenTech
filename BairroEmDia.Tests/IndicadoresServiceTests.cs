using BairroEmDia.Api.Services;
using BairroEmDia.Domain.Entities;
using BairroEmDia.Domain.Enums;
using BairroEmDia.Domain.Interfaces;
using Moq;
using Xunit;

namespace BairroEmDia.Tests;

public class IndicadoresServiceTests
{
    [Fact]
    public async Task Obter_ComOcorrencias_DeveCalcularTotais()
    {
        // Arrange
        var registrada = CriarOcorrencia(
            CategoriaOcorrencia.Iluminacao);

        var emAnalise = CriarOcorrencia(
            CategoriaOcorrencia.Iluminacao);

        emAnalise.AlterarStatus(
            StatusOcorrencia.EmAnalise,
            "Análise iniciada.");

        var encerrada = CriarOcorrencia(
            CategoriaOcorrencia.Buraco);

        encerrada.AlterarStatus(
            StatusOcorrencia.Encerrada,
            "Encerramento de demonstração.");

        IReadOnlyList<Ocorrencia> ocorrencias = new List<Ocorrencia>
        {
            registrada,
            emAnalise,
            encerrada
        };

        var repository = new Mock<IOcorrenciaRepository>();

        repository
            .Setup(r => r.ListarAsync(
                "Chácara Klabin",
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ocorrencias);

        var service = new IndicadoresService(repository.Object);

        // Act
        var resultado = await service.ObterAsync("Chácara Klabin");

        // Assert
        Assert.Equal(3, resultado.TotalOcorrencias);
        Assert.Equal(2, resultado.OcorrenciasAbertas);
        Assert.Equal(1, resultado.OcorrenciasEncerradas);
        Assert.Equal(1, resultado.SemMudancaDeStatus);

        Assert.Equal(
            2,
            resultado.PorCategoria.Single(indicador =>
                indicador.Categoria == CategoriaOcorrencia.Iluminacao)
                .Total);

        Assert.Equal(
            1,
            resultado.PorCategoria.Single(indicador =>
                indicador.Categoria == CategoriaOcorrencia.Buraco)
                .Total);

        Assert.Equal(
            1,
            resultado.PorStatus.Single(indicador =>
                indicador.Status == StatusOcorrencia.EmAnalise)
                .Total);

        Assert.Equal(
            resultado.TotalOcorrencias,
            resultado.PorCategoria.Sum(indicador => indicador.Total));

        Assert.Equal(
            resultado.TotalOcorrencias,
            resultado.PorStatus.Sum(indicador => indicador.Total));
    }

    [Fact]
    public async Task Obter_SemOcorrencias_DeveRetornarTotaisZerados()
    {
        // Arrange
        var repository = new Mock<IOcorrenciaRepository>();

        repository
            .Setup(r => r.ListarAsync(
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Ocorrencia>());

        var service = new IndicadoresService(repository.Object);

        // Act
        var resultado = await service.ObterAsync();

        // Assert
        Assert.Null(resultado.Bairro);
        Assert.Equal(0, resultado.TotalOcorrencias);
        Assert.Equal(0, resultado.OcorrenciasAbertas);
        Assert.Equal(0, resultado.OcorrenciasEncerradas);
        Assert.Equal(0, resultado.SemMudancaDeStatus);

        Assert.Equal(
            Enum.GetValues<CategoriaOcorrencia>().Length,
            resultado.PorCategoria.Count);

        Assert.Equal(
            Enum.GetValues<StatusOcorrencia>().Length,
            resultado.PorStatus.Count);

        Assert.All(
            resultado.PorCategoria,
            indicador => Assert.Equal(0, indicador.Total));

        Assert.All(
            resultado.PorStatus,
            indicador => Assert.Equal(0, indicador.Total));
    }

    [Fact]
    public async Task Obter_ComBairro_DeveRemoverEspacosERepassarFiltro()
    {
        // Arrange
        var repository = new Mock<IOcorrenciaRepository>();

        repository
            .Setup(r => r.ListarAsync(
                "Chácara Klabin",
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Ocorrencia>());

        var service = new IndicadoresService(repository.Object);

        // Act
        var resultado = await service.ObterAsync(
            "  Chácara Klabin  ");

        // Assert
        Assert.Equal("Chácara Klabin", resultado.Bairro);

        repository.Verify(
            r => r.ListarAsync(
                "Chácara Klabin",
                null,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Ocorrencia CriarOcorrencia(
        CategoriaOcorrencia categoria)
    {
        return new Ocorrencia(
            Guid.NewGuid(),
            "Problema de demonstração",
            "Descrição do problema observado.",
            "Chácara Klabin",
            "Rua de demonstração, número 100",
            categoria);
    }
}