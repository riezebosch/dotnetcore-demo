using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace catalogus_events.Model
{
    public partial class ProductContext : DbContext
    {
        public virtual DbSet<Categorie> Categorie { get; set; }
        public virtual DbSet<Leverancier> Leverancier { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductCategorie> ProductCategorie { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
            optionsBuilder.UseSqlServer(@"Data Source=.\SQLEXPRESS;Initial Catalog=Product;Integrated Security=SSPI");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categorie>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK_categorie");

                entity.ToTable("categorie");

                entity.Property(e => e.Id).HasColumnName("cat_id");

                entity.Property(e => e.Naam)
                    .IsRequired()
                    .HasColumnName("cat_naam")
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<Leverancier>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK_leverancier");

                entity.ToTable("leverancier");

                entity.Property(e => e.Id).HasColumnName("lev_id");

                entity.Property(e => e.Naam)
                    .IsRequired()
                    .HasColumnName("lev_naam")
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK_product");

                entity.ToTable("product");

                entity.Property(e => e.Id).HasColumnName("prod_id");

                entity.Property(e => e.AfbeeldingUrl)
                    .IsRequired()
                    .HasColumnName("prod_afbeeldingurl")
                    .HasMaxLength(500);

                entity.Property(e => e.Beschrijving)
                    .IsRequired()
                    .HasColumnName("prod_beschrijving");

                entity.Property(e => e.LeverancierId).HasColumnName("prod_lev_id");

                entity.Property(e => e.LeveranciersProductCode)
                    .IsRequired()
                    .HasColumnName("prod_leveranciersproductid")
                    .HasMaxLength(150);

                entity.Property(e => e.LeverbaarTot)
                    .HasColumnName("prod_leverbaartot")
                    .HasColumnType("datetime");

                entity.Property(e => e.LeverbaarVanaf)
                    .HasColumnName("prod_leverbaarvanaf")
                    .HasColumnType("datetime");

                entity.Property(e => e.Naam)
                    .IsRequired()
                    .HasColumnName("prod_naam")
                    .HasMaxLength(250);

                entity.Property(e => e.Prijs)
                    .HasColumnName("prod_prijs")
                    .HasColumnType("money");

                entity.HasOne(d => d.Leverancier)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.LeverancierId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_product_leverancier");
            });

            modelBuilder.Entity<ProductCategorie>(entity =>
            {
                entity.HasKey(e => new { e.ProdcatProdId, e.ProdcatCatId })
                    .HasName("PK__product___3E2432AF1273C1CD");

                entity.ToTable("product_categorie");

                entity.Property(e => e.ProdcatProdId).HasColumnName("prodcat_prod_id");

                entity.Property(e => e.ProdcatCatId).HasColumnName("prodcat_cat_id");

                entity
                    .HasOne(e => e.Product)
                    .WithMany(e => e.Categorieen)
                    .HasForeignKey(e => e.ProdcatProdId);

                entity
                    .HasOne(e => e.Categorie)
                    .WithMany(e => e.Producten)
                    .HasForeignKey(e => e.ProdcatCatId);
            });
        }
    }
}