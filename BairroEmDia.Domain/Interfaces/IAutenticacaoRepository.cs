using BairroEmDia.Domain.Entities;
namespace BairroEmDia.Domain.Interfaces;

public interface IAutenticacaoRepository
{
    Task<ContaUsuario?> ObterContaAsync(string email, CancellationToken ct = default);
    Task CriarContaAsync(Usuario usuario, ContaUsuario conta, CancellationToken ct = default);
    Task AdicionarSessaoAsync(SessaoUsuario sessao, CancellationToken ct = default);
    Task<(SessaoUsuario Sessao, ContaUsuario Conta, Usuario Usuario)?> ObterSessaoAsync(string hash, CancellationToken ct = default);
    Task RevogarSessaoAsync(Guid id, CancellationToken ct = default);
}
