using Microsoft.EntityFrameworkCore;
using MeuSite.Models.Entities;

namespace MeuSite.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ControleAno> ControleAnos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Subcategoria> Subcategorias { get; set; }
        public DbSet<Receita> Receitas { get; set; }
        public DbSet<Despesa> Despesas { get; set; }
        public DbSet<Divida> Dividas { get; set; }
        public DbSet<ParcelaDivida> ParcelaDividas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração de ControleAno
            modelBuilder.Entity<ControleAno>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Ano).IsRequired();
                entity.Property(e => e.Ativo).HasDefaultValue(true);
            });

            // Configuração de Categoria
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Cor).HasMaxLength(7); // #RRGGBB
                entity.HasOne(e => e.ControleAno).WithMany(c => c.Categorias).HasForeignKey(e => e.ControleAnoId);
            });

            // Configuração de Subcategoria
            modelBuilder.Entity<Subcategoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Categoria).WithMany(c => c.Subcategorias).HasForeignKey(e => e.CategoriaId);
            });

            // Configuração de Receita
            modelBuilder.Entity<Receita>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descricao).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Valor).HasPrecision(18, 2);
                entity.Property(e => e.Data).IsRequired();
                entity.HasOne(e => e.ControleAno).WithMany(c => c.Receitas).HasForeignKey(e => e.ControleAnoId);
                entity.HasOne(e => e.Categoria).WithMany().HasForeignKey(e => e.CategoriaId);
                entity.HasOne(e => e.Subcategoria).WithMany().HasForeignKey(e => e.SubcategoriaId);
            });

            // Configuração de Despesa
            modelBuilder.Entity<Despesa>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descricao).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Valor).HasPrecision(18, 2);
                entity.Property(e => e.Data).IsRequired();
                entity.HasOne(e => e.ControleAno).WithMany(c => c.Despesas).HasForeignKey(e => e.ControleAnoId);
                entity.HasOne(e => e.Categoria).WithMany().HasForeignKey(e => e.CategoriaId);
                entity.HasOne(e => e.Subcategoria).WithMany().HasForeignKey(e => e.SubcategoriaId);
            });

            // Configuração de Divida
            modelBuilder.Entity<Divida>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descricao).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ValorTotal).HasPrecision(18, 2);
                entity.Ignore(e => e.ParcelasDividas); // Ignorar a propriedade que conflita com o nome da coleção
                entity.Property(e => e.NumeroParcelas).IsRequired();
                entity.Property(e => e.DataPrimeiroVencimento).IsRequired();
                entity.HasOne(e => e.ControleAno).WithMany(c => c.Dividas).HasForeignKey(e => e.ControleAnoId);
                entity.HasOne(e => e.Categoria).WithMany().HasForeignKey(e => e.CategoriaId);
            });

            // Configuração de ParcelaDivida
            modelBuilder.Entity<ParcelaDivida>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Valor).HasPrecision(18, 2);
                entity.Property(e => e.DataVencimento).IsRequired();
                entity.Property(e => e.Paga).HasDefaultValue(false);
                entity.HasOne(e => e.Divida).WithMany(d => d.ParcelasDividas).HasForeignKey(e => e.DividaId);
            });
        }
    }
}
