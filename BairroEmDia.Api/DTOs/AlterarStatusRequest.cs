using System.ComponentModel.DataAnnotations;
using BairroEmDia.Domain.Enums;

namespace BairroEmDia.Api.DTOs;

public class AlterarStatusRequest
{
    [EnumDataType(
        typeof(StatusOcorrencia),
        ErrorMessage = "O status informado é inválido.")]
    public StatusOcorrencia NovoStatus { get; set; }

    [Required(ErrorMessage = "A justificativa é obrigatória.")]
    [StringLength(1000)]
    public string Justificativa { get; set; } = string.Empty;
}