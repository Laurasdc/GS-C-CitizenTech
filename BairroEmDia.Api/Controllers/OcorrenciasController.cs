using BairroEmDia.Api.DTOs;
using BairroEmDia.Api.Services;
using BairroEmDia.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BairroEmDia.Api.Controllers;

[ApiController]
[Route("api/ocorrencias")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public class OcorrenciasController : ControllerBase
{
    private readonly IOcorrenciaService _ocorrenciaService;

    public OcorrenciasController(IOcorrenciaService ocorrenciaService)
    {
        _ocorrenciaService = ocorrenciaService;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(OcorrenciaDetalheResponse),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<OcorrenciaDetalheResponse>> Criar(
        [FromBody] CriarOcorrenciaRequest request,
        CancellationToken cancellationToken)
    {
        var ocorrencia = await _ocorrenciaService.CriarAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = ocorrencia.Id },
            ocorrencia);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(OcorrenciaDetalheResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<OcorrenciaDetalheResponse>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var ocorrencia = await _ocorrenciaService.ObterPorIdAsync(
            id,
            cancellationToken);

        return Ok(ocorrencia);
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<OcorrenciaResumoResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OcorrenciaResumoResponse>>> Listar(
        [FromQuery] string? bairro = null,
        [FromQuery] CategoriaOcorrencia? categoria = null,
        [FromQuery] StatusOcorrencia? status = null,
        string? cidade = null,
        string? uf = null,
        CancellationToken cancellationToken = default)
    {
        var ocorrencias = await _ocorrenciaService.ListarAsync(
            bairro,
            categoria,
            status,
            cidade,
            uf,
            cancellationToken);

        return Ok(ocorrencias);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(
        typeof(OcorrenciaDetalheResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<OcorrenciaDetalheResponse>> Editar(
        Guid id,
        [FromBody] EditarOcorrenciaRequest request,
        CancellationToken cancellationToken)
    {
        var ocorrencia = await _ocorrenciaService.EditarAsync(
            id,
            request,
            cancellationToken);

        return Ok(ocorrencia);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(
        typeof(OcorrenciaDetalheResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<OcorrenciaDetalheResponse>> AlterarStatus(
        Guid id,
        [FromBody] AlterarStatusRequest request,
        CancellationToken cancellationToken)
    {
        var ocorrencia = await _ocorrenciaService.AlterarStatusAsync(
            id,
            request,
            cancellationToken);

        return Ok(ocorrencia);
    }

    [HttpPost("{id:guid}/confirmacoes")]
    [ProducesResponseType(
        typeof(OcorrenciaDetalheResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<OcorrenciaDetalheResponse>> Confirmar(
        Guid id,
        [FromBody] ConfirmarOcorrenciaRequest request,
        CancellationToken cancellationToken)
    {
        var ocorrencia = await _ocorrenciaService.ConfirmarAsync(
            id,
            request,
            cancellationToken);

        return Ok(ocorrencia);
    }
}