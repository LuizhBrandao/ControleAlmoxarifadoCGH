using Almoxarifado.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Almoxarifado.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Catalogo> Catalogos { get; set; }
    public DbSet<Equipamento> Equipamentos { get; set; }
    public DbSet<Agente> Agentes { get; set; }
    public DbSet<Movimentacao> Movimentacoes { get; set; }


    // 1. Adicionamos a nova Raiz de Agregação ao Contexto
    public DbSet<Mochila> Mochilas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Catalogo>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Descricao).HasMaxLength(500);
        });

        modelBuilder.Entity<Equipamento>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumeroSerie).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Agente>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Matricula).IsRequired().HasMaxLength(20);
        });

        // 2. Mapeamento Estrito da Raiz de Agregação Mochila
        modelBuilder.Entity<Mochila>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Numero).IsRequired();
            entity.Property(e => e.Status).IsRequired();

            // Mapeando o Value Object (Owned Type) para a mesma tabela
            entity.OwnsOne(e => e.OperacaoAtual, operacao => {
                operacao.Property(o => o.Turno)
                        .HasColumnName("TurnoAtual") // Nome da coluna no banco
                        .HasMaxLength(10)
                        .IsRequired(false); // Pode ser nulo se a mochila estiver disponível

                operacao.Property(o => o.Dupla)
                        .HasColumnName("DuplaAtual")
                        .HasMaxLength(150)
                        .IsRequired(false);
            });

            // Protegendo a borda do Agregado com Shadow Property
            entity.HasMany(e => e.Equipamentos)
                  .WithOne() // Sem propriedade de navegação inversa em Equipamento
                  .HasForeignKey("MochilaId") // Cria a coluna oculta 'MochilaId' na tabela Equipamentos
                  .OnDelete(DeleteBehavior.Restrict); // Evita deleção acidental do histórico
        });

        modelBuilder.Entity<Movimentacao>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NomeAlmoxarife).IsRequired().HasMaxLength(100);

            entity.HasOne(m => m.Catalogo)
                  .WithMany()
                  .HasForeignKey(m => m.CatalogoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Equipamento)
                  .WithMany()
                  .HasForeignKey(m => m.EquipamentoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Agente)
                  .WithMany()
                  .HasForeignKey(m => m.AgenteId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}