using BairroEmDia.Api.DTOs;
using BairroEmDia.Domain.Entities;
using BairroEmDia.Domain.Interfaces;

namespace BairroEmDia.Api.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<UsuarioResponse> CriarAsync(
        CriarUsuarioRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuario = new Usuario(request.Nome);

        await _usuarioRepository.AdicionarAsync(
            usuario,
            cancellationToken);

        return Mapear(usuario);
    }

    public async Task<UsuarioResponse?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(
            id,
            cancellationToken);

        return usuario is null ? null : Mapear(usuario);
    }

    public async Task<IReadOnlyList<UsuarioResponse>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        var usuarios = await _usuarioRepository.ListarAsync(
            cancellationToken);

        return usuarios
            .Select(Mapear)
            .ToList();
    }

    private static UsuarioResponse Mapear(Usuario usuario)
    {
        return new UsuarioResponse(
            usuario.Id,
            usuario.Nome,
            usuario.CriadoEm);
    }
}