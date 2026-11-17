using System.Text;
using BairroEmDia.Domain.Entities;
using BairroEmDia.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BairroEmDia.Api.Services;

public static class RedefinirSenhaLocal
{
    public static async Task<int> ExecutarAsync(IServiceProvider services, string email)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BairroEmDiaDbContext>();
        var normalizado = email.Trim().ToLowerInvariant();
        if (!await db.ContasUsuario.AnyAsync(c => c.Email == normalizado))
        {
            Console.Error.WriteLine("Conta não encontrada no banco configurado para esta API. Nenhuma alteração foi feita.");
            return 1;
        }
        if (Console.IsInputRedirected)
        {
            Console.Error.WriteLine("Execute diretamente em um terminal para digitar a nova senha.");
            return 1;
        }
        Console.WriteLine($"Redefinindo senha de {normalizado}. Relatos e perfil serão mantidos.");
        Console.Write("Nova senha (8 a 128 caracteres): ");
        var senha = LerSenha();
        Console.Write("Confirme a nova senha: ");
        var confirmacao = LerSenha();
        if (senha is null || senha != confirmacao || senha.Length < 8 || senha.Length > 128)
        {
            Console.Error.WriteLine("Senhas diferentes, canceladas ou fora do limite de 8 a 128 caracteres. Nenhuma alteração foi feita.");
            return 1;
        }
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ContaUsuario>>();
        if (!await RedefinirAsync(db, hasher, normalizado, senha))
        {
            Console.Error.WriteLine("Conta não encontrada. Nenhuma alteração foi feita.");
            return 1;
        }
        Console.WriteLine("Senha redefinida. As sessões anteriores dessa conta foram encerradas. Inicie a API e entre com a nova senha.");
        return 0;
    }

    public static async Task<bool> RedefinirAsync(BairroEmDiaDbContext db,
        IPasswordHasher<ContaUsuario> hasher, string email, string senha)
    {
        if (senha.Length < 8 || senha.Length > 128)
            throw new ArgumentException("Use uma senha entre 8 e 128 caracteres.", nameof(senha));
        await using var transaction = await db.Database.BeginTransactionAsync();
        var normalizado = email.Trim().ToLowerInvariant();
        var conta = await db.ContasUsuario.SingleOrDefaultAsync(c => c.Email == normalizado);
        if (conta is null) return false;
        db.Entry(conta).Property(c => c.SenhaHash).CurrentValue = hasher.HashPassword(conta, senha);
        db.SessoesUsuario.RemoveRange(await db.SessoesUsuario.Where(s => s.UsuarioId == conta.UsuarioId).ToListAsync());
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    private static string? LerSenha()
    {
        var texto = new StringBuilder();
        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);
            if (tecla.Key == ConsoleKey.Enter) { Console.WriteLine(); return texto.ToString(); }
            if (tecla.Key == ConsoleKey.Escape) { Console.WriteLine(); return null; }
            if (tecla.Key == ConsoleKey.Backspace)
            {
                if (texto.Length > 0) texto.Length--;
            }
            else if (!char.IsControl(tecla.KeyChar)) texto.Append(tecla.KeyChar);
        }
    }
}
