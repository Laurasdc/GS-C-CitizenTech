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

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();
    public DbSet<Confirmacao> Confirmacoes => Set<Confirmacao>();

    public DbSet<HistoricoAtualizacao> HistoricosAtualizacao
        => Set<HistoricoAtualizacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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