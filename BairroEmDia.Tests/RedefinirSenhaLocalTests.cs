using BairroEmDia.Api.Services;
using BairroEmDia.Domain.Entities;
using BairroEmDia.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BairroEmDia.Tests;

public class RedefinirSenhaLocalTests
{
    [Fact]
    public async Task RedefinirMantemIdentidadePerfilEOutrasContasERevogaSessoesDoAlvo()
    {
        await using var conexao = new SqliteConnection("Data Source=:memory:");
        await conexao.OpenAsync();
        await using var db = new BairroEmDiaDbContext(new DbContextOptionsBuilder<BairroEmDiaDbContext>().UseSqlite(conexao).Options);
        await db.Database.EnsureCreatedAsync();
        var usuario = new Usuario("Gestor");
        var outro = new Usuario("Morador");
        var hasher = new PasswordHasher<ContaUsuario>();
        var conta = new ContaUsuario(usuario.Id, "gestor@example.com", "antigo");
        conta.PromoverParaGestor();
        var outra = new ContaUsuario(outro.Id, "outro@example.com", "inalterado");
        db.Usuarios.AddRange(usuario, outro);
        db.ContasUsuario.AddRange(conta, outra);
        db.SessoesUsuario.AddRange(new SessaoUsuario(usuario.Id,"alvo",DateTime.UtcNow.AddHours(1)),new SessaoUsuario(outro.Id,"outra",DateTime.UtcNow.AddHours(1)));
        await db.SaveChangesAsync();

        Assert.True(await RedefinirSenhaLocal.RedefinirAsync(db, hasher, " GESTOR@example.com ", "NovaSenha123!"));
        db.ChangeTracker.Clear();

        var atual = await db.ContasUsuario.SingleAsync(c => c.UsuarioId == usuario.Id);
        Assert.Equal("Gestor", atual.Perfil);
        Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(atual,atual.SenhaHash,"NovaSenha123!"));
        Assert.Equal(2, await db.Usuarios.CountAsync());
        Assert.False(await db.SessoesUsuario.AnyAsync(s=>s.UsuarioId==usuario.Id));
        Assert.True(await db.SessoesUsuario.AnyAsync(s=>s.UsuarioId==outro.Id));
        Assert.Equal("inalterado", (await db.ContasUsuario.SingleAsync(c=>c.UsuarioId==outro.Id)).SenhaHash);
        Assert.False(await RedefinirSenhaLocal.RedefinirAsync(db,hasher,"ausente@example.com","OutraSenha123!"));
        Assert.Equal(2,await db.ContasUsuario.CountAsync());
    }
}
