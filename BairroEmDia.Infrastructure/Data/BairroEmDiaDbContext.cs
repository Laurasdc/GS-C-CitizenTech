using BairroEmDia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BairroEmDia.Infrastructure.Data;

public class BairroEmDiaDbContext : DbContext
{
    public BairroEmDiaDbContext(
        DbContextOptions<BairroEmDiaDbContext> options)
        : base(options)
    {
    }

    public DbSet<ContaUsuario> ContasUsuario => Set<ContaUsuario>();
    public DbSet<SessaoUsuario> SessoesUsuario => Set<SessaoUsuario>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();
    public DbSet<Confirmacao> Confirmacoes => Set<Confirmacao>();

    public DbSet<HistoricoAtualizacao> HistoricosAtualizacao
        => Set<HistoricoAtualizacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ContaUsuario>(entity =>
        {
            entity.ToTable("ContasUsuario");
            entity.HasKey(c => c.UsuarioId);
            entity.Property(c => c.UsuarioId).ValueGeneratedNever();
            entity.Property(c => c.Email).IsRequired().HasMaxLength(254);
            entity.HasIndex(c => c.Email).IsUnique();
            entity.Property(c => c.SenhaHash).IsRequired().HasMaxLength(1024);
            entity.Property(c => c.Perfil).IsRequired().HasMaxLength(30);
            entity.HasOne<Usuario>().WithOne().HasForeignKey<ContaUsuario>(c => c.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<SessaoUsuario>(entity =>
        {
            entity.ToTable("SessoesUsuario");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).ValueGeneratedNever();
            entity.Property(s => s.TokenHash).IsRequired().HasMaxLength(64);
            entity.HasIndex(s => s.TokenHash).IsUnique();
            entity.HasIndex(s => s.ExpiraEm);
            entity.HasOne<ContaUsuario>().WithMany().HasForeignKey(s => s.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                .ValueGeneratedNever();

            entity.Property(u => u.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.CriadoEm)
                .IsRequired();
        });

        modelBuilder.Entity<Ocorrencia>(entity =>
        {
            entity.ToTable("Ocorrencias");

            entity.HasKey(o => o.Id);

            entity.Property(o => o.Id)
                .ValueGeneratedNever();

            entity.Property(o => o.Titulo)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(o => o.Descricao)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(o => o.Cidade).IsRequired().HasMaxLength(100);
            entity.Property(o => o.Uf).IsRequired().HasMaxLength(2);

            entity.Property(o => o.Bairro)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(o => o.Localizacao)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(o => o.Categoria)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(o => o.CriadoEm)
                .IsRequired();

            entity.Property(o => o.AtualizadoEm)
                .IsRequired();

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(o => o.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(o => o.Historico)
                .WithOne()
                .HasForeignKey(h => h.OcorrenciaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(o => o.Historico)
                .HasField("_historico")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.HasMany(o => o.Confirmacoes)
                .WithOne()
                .HasForeignKey(c => c.OcorrenciaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(o => o.Confirmacoes)
                .HasField("_confirmacoes")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.HasIndex(o => new { o.Bairro, o.Status });
            entity.HasIndex(o => o.Categoria);
            entity.HasIndex(o => new { o.Uf, o.Cidade, o.Bairro, o.Status });
        });

        modelBuilder.Entity<Confirmacao>(entity =>
        {
            entity.ToTable("Confirmacoes");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id)
                .ValueGeneratedNever();

            entity.Property(c => c.CriadoEm)
                .IsRequired();

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => new
            {
                c.OcorrenciaId,
                c.UsuarioId
            })
                .IsUnique();
        });

        modelBuilder.Entity<HistoricoAtualizacao>(entity =>
        {
            entity.ToTable("HistoricosAtualizacao");

            entity.HasKey(h => h.Id);

            entity.Property(h => h.Id)
                .ValueGeneratedNever();

            entity.Property(h => h.StatusAnterior)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(h => h.NovoStatus)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(h => h.Justificativa)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(h => h.CriadoEm)
                .IsRequired();
        });
    }
}