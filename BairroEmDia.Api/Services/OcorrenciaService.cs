using BairroEmDia.Api.DTOs;
using BairroEmDia.Domain.Entities;
using BairroEmDia.Domain.Enums;
using BairroEmDia.Domain.Exceptions;
using BairroEmDia.Domain.Interfaces;

namespace BairroEmDia.Api.Services;

public class OcorrenciaService : IOcorrenciaService
{
    private readonly IOcorrenciaRepository _ocorrenciaRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public OcorrenciaService(
        IOcorrenciaRepository ocorrenciaRepository,
        IUsuarioRepository usuarioRepository)
    {
        _ocorrenciaRepository = ocorrenciaRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<OcorrenciaDetalheResponse> CriarAsync(
        CriarOcorrenciaRequest request,
        CancellationToken cancellationToken = default)
    {
        var ocorrencia = new Ocorrencia(
            request.UsuarioId,
            request.Titulo,
            request.Descricao,
            request.Bairro,
            request.Localizacao,
            request.Categoria);

        await ValidarUsuarioAsync(
            request.UsuarioId,
            cancellationToken);

        await _ocorrenciaRepository.AdicionarAsync(
            ocorrencia,
            cancellationToken);

        return MapearDetalhe(ocorrencia);
    }

    public async Task<OcorrenciaDetalheResponse> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var ocorrencia = await BuscarOcorrenciaAsync(
            id,
            cancellationToken);

        return MapearDetalhe(ocorrencia);
    }

    public async Task<IReadOnlyList<OcorrenciaResumoResponse>> ListarAsync(
        string? bairro = null,
        CategoriaOcorrencia? categoria = null,
        StatusOcorrencia? status = null,
        CancellationToken cancellationToken = default)
    {
        if (categoria.HasValue &&
            !Enum.IsDefined(
                typeof(CategoriaOcorrencia),
                categoria.Value))
        {
            throw new RegraDeNegocioException(
                "A categoria informada é inválida.");
        }

        if (status.HasValue &&
            !Enum.IsDefined(
                typeof(StatusOcorrencia),
                status.Value))
        {
            throw new RegraDeNegocioException(
                "O status informado é inválido.");
        }

        var ocorrencias = await _ocorrenciaRepository.ListarAsync(
            bairro,
            categoria,
            status,
            cancellationToken);

        return ocorrencias
            .Select(ocorrencia => new OcorrenciaResumoResponse(
                ocorrencia.Id,
                ocorrencia.Titulo,
                ocorrencia.Bairro,
                ocorrencia.Categoria,
                ocorrencia.Status,
                ocorrencia.CriadoEm,
                ocorrencia.AtualizadoEm))
            .ToList();
    }

    public async Task<OcorrenciaDetalheResponse> EditarAsync(
        Guid id,
        EditarOcorrenciaRequest request,
        CancellationToken cancellationToken = default)
    {
        var ocorrencia = await BuscarOcorrenciaAsync(
            id,
            cancellationToken);

        ocorrencia.Editar(
            request.Titulo,
            request.Descricao,
            request.Bairro,
            request.Localizacao,
            request.Categoria);

        await _ocorrenciaRepository.SalvarAlteracoesAsync(
            cancellationToken);

        return MapearDetalhe(ocorrencia);
    }

    public async Task<OcorrenciaDetalheResponse> AlterarStatusAsync(
        Guid id,
        AlterarStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var ocorrencia = await BuscarOcorrenciaAsync(
            id,
            cancellationToken);

        ocorrencia.AlterarStatus(
            request.NovoStatus,
            request.Justificativa);

        await _ocorrenciaRepository.SalvarAlteracoesAsync(
            cancellationToken);

        return MapearDetalhe(ocorrencia);
    }

    public async Task<OcorrenciaDetalheResponse> ConfirmarAsync(
        Guid id,
        ConfirmarOcorrenciaRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidarUsuarioAsync(
            request.UsuarioId,
            cancellationToken);

        var ocorrencia = await BuscarOcorrenciaAsync(
            id,
            cancellationToken);

        ocorrencia.Confirmar(request.UsuarioId);

        await _ocorrenciaRepository.SalvarAlteracoesAsync(
            cancellationToken);

        return MapearDetalhe(ocorrencia);
    }

    private async Task<Ocorrencia> BuscarOcorrenciaAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var ocorrencia = await _ocorrenciaRepository.ObterPorIdAsync(
            id,
            cancellationToken);

        return ocorrencia
            ?? throw new RecursoNaoEncontradoException(
                "A ocorrência não foi encontrada.");
    }

    private async Task ValidarUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new RegraDeNegocioException(
                "O usuário é obrigatório.");
        }

        var usuario = await _usuarioRepository.ObterPorIdAsync(
            usuarioId,
            cancellationToken);

        if (usuario is null)
        {
            throw new RecursoNaoEncontradoException(
                "O usuário não foi encontrado.");
        }
    }

    private static OcorrenciaDetalheResponse MapearDetalhe(
        Ocorrencia ocorrencia)
    {
        var historico = ocorrencia.Historico
            .OrderBy(atualizacao => atualizacao.CriadoEm)
            .Select(atualizacao => new HistoricoAtualizacaoResponse(
                atualizacao.Id,
                atualizacao.StatusAnterior,
                atualizacao.NovoStatus,
                atualizacao.Justificativa,
                atualizacao.CriadoEm))
            .ToList();

        return new OcorrenciaDetalheResponse(
            ocorrencia.Id,
            ocorrencia.UsuarioId,
            ocorrencia.Titulo,
            ocorrencia.Descricao,
            ocorrencia.Bairro,
            ocorrencia.Localizacao,
            ocorrencia.Categoria,
            ocorrencia.Status,
            ocorrencia.CriadoEm,
            ocorrencia.AtualizadoEm,
            ocorrencia.Confirmacoes.Count,
            historico);
    }
}