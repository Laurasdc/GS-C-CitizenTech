using BairroEmDia.Api.DTOs;
using BairroEmDia.Api.Services;
using BairroEmDia.Domain.Entities;
using BairroEmDia.Domain.Enums;
using BairroEmDia.Domain.Exceptions;
using BairroEmDia.Domain.Interfaces;
using Moq;
using Xunit;

namespace BairroEmDia.Tests;

public class OcorrenciaServiceTests
{
    private readonly Mock<IOcorrenciaRepository> _ocorrenciaRepository;
    private readonly Mock<IUsuarioRepository> _usuarioRepository;
    private readonly OcorrenciaService _service;

    public OcorrenciaServiceTests()
    {
        _ocorrenciaRepository = new Mock<IOcorrenciaRepository>();
        _usuarioRepository = new Mock<IUsuarioRepository>();

        _service = new OcorrenciaService(
            _ocorrenciaRepository.Object,
            _usuarioRepository.Object);
    }

    [Fact]
    public async Task Criar_ComUsuarioExistente_DeveSalvarOcorrencia()
    {
        // Arrange
        var usuario = new Usuario("Morador de Teste");
        var request = CriarRequest(usuario.Id);

        _usuarioRepository
            .Setup(repository => repository.ObterPorIdAsync(
                usuario.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _ocorrenciaRepository
            .Setup(repository => repository.AdicionarAsync(
                It.IsAny<Ocorrencia>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.CriarAsync(request);

        // Assert
        Assert.Equal(usuario.Id, resultado.UsuarioId);
        Assert.Equal(request.Titulo, resultado.Titulo);
        Assert.Equal(StatusOcorrencia.Registrada, resultado.Status);

        _usuarioRepository.Verify(
            repository => repository.ObterPorIdAsync(
                usuario.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _ocorrenciaRepository.Verify(
            repository => repository.AdicionarAsync(
                It.Is<Ocorrencia>(ocorrencia =>
                    ocorrencia.Id == resultado.Id &&
                    ocorrencia.UsuarioId == usuario.Id &&
                    ocorrencia.Titulo == request.Titulo),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Criar_ComUsuarioInexistente_NaoDeveSalvar()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var request = CriarRequest(usuarioId);

        _usuarioRepository
            .Setup(repository => repository.ObterPorIdAsync(
                usuarioId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        // Act
        var excecao =
            await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => _service.CriarAsync(request));

        // Assert
        Assert.Equal(
            "O usuário não foi encontrado.",
            excecao.Message);

        _usuarioRepository.Verify(
            repository => repository.ObterPorIdAsync(
                usuarioId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _ocorrenciaRepository.Verify(
            repository => repository.AdicionarAsync(
                It.IsAny<Ocorrencia>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AlterarStatus_ComDadosValidos_DeveSalvarHistorico()
    {
        // Arrange
        var ocorrencia = CriarOcorrencia();

        var request = new AlterarStatusRequest
        {
            NovoStatus = StatusOcorrencia.EmAnalise,
            Justificativa = "Análise iniciada pela associação."
        };

        _ocorrenciaRepository
            .Setup(repository => repository.ObterPorIdAsync(
                ocorrencia.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ocorrencia);

        _ocorrenciaRepository
            .Setup(repository => repository.SalvarAlteracoesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.AlterarStatusAsync(
            ocorrencia.Id,
            request);

        // Assert
        Assert.Equal(StatusOcorrencia.EmAnalise, resultado.Status);

        var historico = Assert.Single(resultado.Historico);

        Assert.Equal(
            StatusOcorrencia.Registrada,
            historico.StatusAnterior);

        Assert.Equal(
            StatusOcorrencia.EmAnalise,
            historico.NovoStatus);

        Assert.Equal(request.Justificativa, historico.Justificativa);

        _ocorrenciaRepository.Verify(
            repository => repository.SalvarAlteracoesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AlterarStatus_SemJustificativa_NaoDeveSalvar()
    {
        // Arrange
        var ocorrencia = CriarOcorrencia();

        var request = new AlterarStatusRequest
        {
            NovoStatus = StatusOcorrencia.EmAnalise,
            Justificativa = ""
        };

        _ocorrenciaRepository
            .Setup(repository => repository.ObterPorIdAsync(
                ocorrencia.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ocorrencia);

        // Act
        await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => _service.AlterarStatusAsync(
                ocorrencia.Id,
                request));

        // Assert
        Assert.Equal(StatusOcorrencia.Registrada, ocorrencia.Status);
        Assert.Empty(ocorrencia.Historico);

        _ocorrenciaRepository.Verify(
            repository => repository.SalvarAlteracoesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AlterarStatus_ComOcorrenciaInexistente_NaoDeveSalvar()
    {
        // Arrange
        var ocorrenciaId = Guid.NewGuid();

        var request = new AlterarStatusRequest
        {
            NovoStatus = StatusOcorrencia.EmAnalise,
            Justificativa = "Análise iniciada."
        };

        _ocorrenciaRepository
            .Setup(repository => repository.ObterPorIdAsync(
                ocorrenciaId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ocorrencia?)null);

        // Act
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => _service.AlterarStatusAsync(
                ocorrenciaId,
                request));

        // Assert
        _ocorrenciaRepository.Verify(
            repository => repository.SalvarAlteracoesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static CriarOcorrenciaRequest CriarRequest(Guid usuarioId)
    {
        return new CriarOcorrenciaRequest
        {
            UsuarioId = usuarioId,
            Titulo = "Poste apagado",
            Descricao = "Iluminação apagada próximo ao ponto de ônibus.",
            Bairro = "Chácara Klabin",
            Localizacao = "Rua de demonstração, número 100",
            Categoria = CategoriaOcorrencia.Iluminacao
        };
    }

    private static Ocorrencia CriarOcorrencia()
    {
        var request = CriarRequest(Guid.NewGuid());

        return new Ocorrencia(
            request.UsuarioId,
            request.Titulo,
            request.Descricao,
            request.Bairro,
            request.Localizacao,
            request.Categoria);
    }
}