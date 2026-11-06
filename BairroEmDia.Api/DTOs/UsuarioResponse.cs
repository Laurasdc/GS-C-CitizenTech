namespace BairroEmDia.Api.DTOs;

public record UsuarioResponse(
    Guid Id,
    string Nome,
    DateTime CriadoEm);