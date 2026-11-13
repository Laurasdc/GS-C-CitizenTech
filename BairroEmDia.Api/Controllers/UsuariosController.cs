using BairroEmDia.Api.DTOs;
using BairroEmDia.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BairroEmDia.Api.Controllers;

[Authorize(Roles = "Gestor")]
[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioResponse>> Criar(
        [FromBody] CriarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioService.CriarAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = usuario.Id },
            usuario);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioService.ObterPorIdAsync(
            id,
            cancellationToken);

        if (usuario is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Recurso não encontrado",
                Detail = "O usuário não foi encontrado."
            });
        }

        return Ok(usuario);
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<UsuarioResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UsuarioResponse>>> Listar(
        CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioService.ListarAsync(
            cancellationToken);

        return Ok(usuarios);
    }
}