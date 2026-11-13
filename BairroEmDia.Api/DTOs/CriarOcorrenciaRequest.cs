using System.ComponentModel.DataAnnotations;
using BairroEmDia.Domain.Enums;

namespace BairroEmDia.Api.DTOs;

public class CriarOcorrenciaRequest
{
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid UsuarioId { get; set; }

    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(120)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(2000)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    [StringLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "A UF é obrigatória.")]
    [StringLength(2, MinimumLength = 2)]
    public string Uf { get; set; } = string.Empty;

    [Required(ErrorMessage = "O bairro é obrigatório.")]
    [StringLength(100)]
    public string Bairro { get; set; } = string.Empty;

    [Required(ErrorMessage = "A localização é obrigatória.")]
    [StringLength(250)]
    public string Localizacao { get; set; } = string.Empty;

    [EnumDataType(
        typeof(CategoriaOcorrencia),
        ErrorMessage = "A categoria informada é inválida.")]
    public CategoriaOcorrencia Categoria { get; set; }
}