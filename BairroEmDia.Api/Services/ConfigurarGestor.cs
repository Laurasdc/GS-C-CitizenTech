using BairroEmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BairroEmDia.Api.Services;

public static class ConfigurarGestor
{
    // Não é exposto por controller: requer acesso ao ambiente da API e ao banco.
    public static async Task<int> ExecutarAsync(IServiceProvider services, string email)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BairroEmDiaDbContext>();
        var normalizado = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizado))
        {
            Console.Error.WriteLine("Informe o e-mail de uma conta já cadastrada no app.");
            return 1;
        }
        await using var transaction = await db.Database.BeginTransactionAsync();
        var conta = await db.ContasUsuario.SingleOrDefaultAsync(c => c.Email == normalizado);
        if (conta is null)
        {
            Console.Error.WriteLine("Conta não encontrada. Cadastre-a no app antes de habilitar a gestão.");
            return 1;
        }
        conta.PromoverParaGestor();
        // Obriga um novo login após a concessão de acesso administrativo.
        db.SessoesUsuario.RemoveRange(await db.SessoesUsuario.Where(s => s.UsuarioId == conta.UsuarioId).ToListAsync());
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        Console.WriteLine("Perfil Gestor habilitado. Entre novamente no app com essa conta.");
        return 0;
    }
}
