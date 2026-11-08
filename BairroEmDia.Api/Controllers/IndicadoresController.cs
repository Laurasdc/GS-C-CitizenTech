using BairroEmDia.Api.DTOs;
using BairroEmDia.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BairroEmDia.Api.Controllers;

[ApiController]
[Route("api/indicadores")]
public class IndicadoresController : ControllerBase
{
    private readonly IIndicadoresService _indicadoresService;

    public IndicadoresController(
        IIndicadoresService indicadoresService)
    {
        _indicadoresService = indicadoresService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IndicadoresResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IndicadoresResponse>> Obter(
        [FromQuery] string? bairro = null,
        CancellationToken cancellationToken = default)
    {
        var indicadores = await _indicadoresService.ObterAsync(
            bairro,
            cancellationToken);

        return Ok(indicadores);
    }
}