using System.ComponentModel.DataAnnotations;
namespace BairroEmDia.Api.DTOs;

public class EntrarRequest
{
    [Required(ErrorMessage = "Informe seu e-mail."), EmailAddress(ErrorMessage = "Informe um e-mail válido."), StringLength(254)]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Informe sua senha."), StringLength(128, MinimumLength = 8, ErrorMessage = "Use uma senha entre 8 e 128 caracteres.")]
    public string Senha { get; set; } = string.Empty;
}
public class RegistrarContaRequest : EntrarRequest
{
    [Required(ErrorMessage = "Informe seu nome."), StringLength(100)]
    public string Nome { get; set; } = string.Empty;
}
public record ContaResponse(Guid Id, string Nome, string Email);
public record SessaoResponse(ContaResponse Usuario, DateTime ExpiraEm, string? Token = null);
