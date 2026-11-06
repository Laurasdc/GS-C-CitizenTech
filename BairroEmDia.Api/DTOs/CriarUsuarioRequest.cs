using System.ComponentModel.DataAnnotations;

namespace BairroEmDia.Api.DTOs;

public class CriarUsuarioRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(
        100,
        ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;
}