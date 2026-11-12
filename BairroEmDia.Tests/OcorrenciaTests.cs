using BairroEmDia.Domain.Entities;
using BairroEmDia.Domain.Enums;
using BairroEmDia.Domain.Exceptions;
using Xunit;

namespace BairroEmDia.Tests;

public class OcorrenciaTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveIniciarComoRegistrada()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        // Act
        var ocorrencia = CriarOcorrencia(usuarioId);

        // Assert
        Assert.NotEqual(Guid.Empty, ocorrencia.Id);
        Assert.Equal(usuarioId, ocorrencia.UsuarioId);
        Assert.Equal(StatusOcorrencia.Registrada, ocorrencia.Status);
        Assert.Empty(ocorrencia.Historico);
        Assert.Empty(ocorrencia.Confirmacoes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_SemTitulo_DeveLancarExcecao(string titulo)
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        // Act
        var excecao = Assert.Throws<RegraDeNegocioException>(() =>
            new Ocorrencia(
                usuarioId,
                titulo,
                "Poste apagado próximo ao ponto de ônibus.",
                "Centro",
                "Rua Exemplo, próximo ao número 100",
                CategoriaOcorrencia.Iluminacao, "São Paulo", "SP"));

        // Assert
        Assert.Equal(
            "O título deve ser preenchido(a).",
            excecao.Message);
    }

    [Fact]
    public void Criar_ComCategoriaInvalida_DeveLancarExcecao()
    {
        // Arrange
        var categoriaInvalida = (CategoriaOcorrencia)999;

        // Act
        var excecao = Assert.Throws<RegraDeNegocioException>(() =>
            new Ocorrencia(
                Guid.NewGuid(),
                "Poste apagado",
                "Poste apagado próximo ao ponto de ônibus.",
                "Centro",
                "Rua Exemplo, próximo ao número 100",
                categoriaInvalida, "São Paulo", "SP"));

        // Assert
        Assert.Equal(
            "A categoria informada é inválida.",
            excecao.Message);
    }

    [Fact]
    public void AlterarStatus_ComJustificativa_DeveRegistrarHistorico()
    {
        // Arrange
        var ocorrencia = CriarOcorrencia();

        // Act
        ocorrencia.AlterarStatus(
            StatusOcorrencia.EmAnalise,
            "A associação iniciou a análise do registro.");

        // Assert
        Assert.Equal(StatusOcorrencia.EmAnalise, ocorrencia.Status);

        var atualizacao = Assert.Single(ocorrencia.Historico);

        Assert.Equal(ocorrencia.Id, atualizacao.OcorrenciaId);
        Assert.Equal(
            StatusOcorrencia.Registrada,
            atualizacao.StatusAnterior);
        Assert.Equal(
            StatusOcorrencia.EmAnalise,
            atualizacao.NovoStatus);
        Assert.Equal(
            "A associação iniciou a análise do registro.",
            atualizacao.Justificativa);
        Assert.Equal(atualizacao.CriadoEm, ocorrencia.AtualizadoEm);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AlterarStatus_SemJustificativa_DeveManterEstado(
        string justificativa)
    {
        // Arrange
        var ocorrencia = CriarOcorrencia();
        var atualizadoEmAnterior = ocorrencia.AtualizadoEm;

        // Act
        Assert.Throws<RegraDeNegocioException>(() =>
            ocorrencia.AlterarStatus(
                StatusOcorrencia.EmAnalise,
                justificativa));

        // Assert
        Assert.Equal(StatusOcorrencia.Registrada, ocorrencia.Status);
        Assert.Empty(ocorrencia.Historico);
        Assert.Equal(atualizadoEmAnterior, ocorrencia.AtualizadoEm);
    }

    [Fact]
    public void AlterarStatus_ParaMesmoStatus_DeveLancarExcecao()
    {
        // Arrange
        var ocorrencia = CriarOcorrencia();

        // Act
        var excecao = Assert.Throws<RegraDeNegocioException>(() =>
            ocorrencia.AlterarStatus(
                StatusOcorrencia.Registrada,
                "Tentativa de repetir o status."));

        // Assert
        Assert.Equal(
            "A ocorrência já está nesse status.",
            excecao.Message);
        Assert.Empty(ocorrencia.Historico);
    }

    [Fact]
    public void Editar_OcorrenciaEncerrada_DeveManterDados()
    {
        // Arrange
        var ocorrencia = CriarOcorrencia();

        ocorrencia.AlterarStatus(
            StatusOcorrencia.Encerrada,
            "O problema foi resolvido.");

        var tituloAnterior = ocorrencia.Titulo;

        // Act
        Assert.Throws<RegraDeNegocioException>(() =>
            ocorrencia.Editar(
                "Novo título",
                "Nova descrição",
                "Outro bairro",
                "Outro endereço",
                CategoriaOcorrencia.Buraco, "São Paulo", "SP"));

        // Assert
        Assert.Equal(tituloAnterior, ocorrencia.Titulo);
        Assert.Equal(StatusOcorrencia.Encerrada, ocorrencia.Status);
    }

    [Fact]
    public void Reabrir_ParaEmAnalise_DevePermitirEdicao()
    {
        // Arrange
        var ocorrencia = CriarOcorrencia();

        ocorrencia.AlterarStatus(
            StatusOcorrencia.Encerrada,
            "O problema foi considerado resolvido.");

        // Act
        ocorrencia.AlterarStatus(
            StatusOcorrencia.EmAnalise,
            "O morador informou que o problema voltou.");

        ocorrencia.Editar(
            "Poste apagado novamente",
            "A iluminação voltou a apresentar falha.",
            "Centro",
            "Rua Exemplo, próximo ao número 100",
            CategoriaOcorrencia.Iluminacao, "São Paulo", "SP");

        // Assert
        Assert.Equal(StatusOcorrencia.EmAnalise, ocorrencia.Status);
        Assert.Equal("Poste apagado novamente", ocorrencia.Titulo);
        Assert.Equal(2, ocorrencia.Historico.Count);
    }

    [Theory]
    [InlineData(StatusOcorrencia.Registrada)]
    [InlineData(StatusOcorrencia.EmAcompanhamento)]
    public void Reabrir_ParaStatusDiferenteDeEmAnalise_DeveLancarExcecao(
        StatusOcorrencia novoStatus)
    {
        // Arrange
        var ocorrencia = CriarOcorrencia();

        ocorrencia.AlterarStatus(
            StatusOcorrencia.Encerrada,
            "O problema foi resolvido.");

        // Act
        Assert.Throws<RegraDeNegocioException>(() =>
            ocorrencia.AlterarStatus(
                novoStatus,
                "Tentativa de reabertura."));

        // Assert
        Assert.Equal(StatusOcorrencia.Encerrada, ocorrencia.Status);
        Assert.Single(ocorrencia.Historico);
    }

    [Fact]
    public void Confirmar_ComOutroUsuario_DeveAdicionarConfirmacao()
    {
        // Arrange
        var ocorrencia = CriarOcorrencia();
        var outroUsuarioId = Guid.NewGuid();

        // Act
        ocorrencia.Confirmar(outroUsuarioId);

        // Assert
        var confirmacao = Assert.Single(ocorrencia.Confirmacoes);

        Assert.Equal(outroUsuarioId, confirmacao.UsuarioId);
        Assert.Equal(ocorrencia.Id, confirmacao.OcorrenciaId);
    }

    [Fact]
    public void Confirmar_PropriaOcorrencia_DeveLancarExcecao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var ocorrencia = CriarOcorrencia(usuarioId);

        // Act
        var excecao = Assert.Throws<RegraDeNegocioException>(() =>
            ocorrencia.Confirmar(usuarioId));

        // Assert
        Assert.Equal(
            "Você não pode confirmar a própria ocorrência.",
            excecao.Message);
        Assert.Empty(ocorrencia.Confirmacoes);
    }

    [Fact]
    public void Confirmar_DuasVezesComMesmoUsuario_DeveLancarExcecao()
    {
        // Arrange
        var ocorrencia = CriarOcorrencia();
        var outroUsuarioId = Guid.NewGuid();

        ocorrencia.Confirmar(outroUsuarioId);

        // Act
        var excecao = Assert.Throws<RegraDeNegocioException>(() =>
            ocorrencia.Confirmar(outroUsuarioId));

        // Assert
        Assert.Equal(
            "Você já confirmou esta ocorrência.",
            excecao.Message);
        Assert.Single(ocorrencia.Confirmacoes);
    }

    [Fact]
    public void Confirmar_ComUsuarioVazio_DeveLancarExcecao()
    {
        // Arrange
        var ocorrencia = CriarOcorrencia();

        // Act
        Assert.Throws<RegraDeNegocioException>(() =>
            ocorrencia.Confirmar(Guid.Empty));

        // Assert
        Assert.Empty(ocorrencia.Confirmacoes);
    }

    private static Ocorrencia CriarOcorrencia(Guid? usuarioId = null)
    {
        return new Ocorrencia(
            usuarioId ?? Guid.NewGuid(),
            "Poste apagado",
            "Poste apagado próximo ao ponto de ônibus.",
            "Centro",
            "Rua Exemplo, próximo ao número 100",
            CategoriaOcorrencia.Iluminacao, "São Paulo", "SP");
    }
}