using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using catalogus_events.Model;

namespace catalogusevents.Migrations
{
    [DbContext(typeof(ProductContext))]
    [Migration("20170103153104_foreignkey")]
    partial class foreignkey
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("catalogus_events.Model.Categorie", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("cat_id");

                    b.Property<string>("Naam")
                        .IsRequired()
                        .HasColumnName("cat_naam")
                        .HasMaxLength(150);

                    b.HasKey("Id")
                        .HasName("PK_categorie");

                    b.ToTable("categorie");
                });

            modelBuilder.Entity("catalogus_events.Model.Leverancier", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("lev_id");

                    b.Property<string>("Naam")
                        .IsRequired()
                        .HasColumnName("lev_naam")
                        .HasMaxLength(200);

                    b.HasKey("Id")
                        .HasName("PK_leverancier");

                    b.ToTable("leverancier");
                });

            modelBuilder.Entity("catalogus_events.Model.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("prod_id");

                    b.Property<string>("AfbeeldingUrl")
                        .IsRequired()
                        .HasColumnName("prod_afbeeldingurl")
                        .HasMaxLength(500);

                    b.Property<string>("Beschrijving")
                        .IsRequired()
                        .HasColumnName("prod_beschrijving");

                    b.Property<int>("LeverancierId")
                        .HasColumnName("prod_lev_id");

                    b.Property<string>("LeveranciersProductCode")
                        .IsRequired()
                        .HasColumnName("prod_leveranciersproductid")
                        .HasMaxLength(150);

                    b.Property<DateTime?>("LeverbaarTot")
                        .HasColumnName("prod_leverbaartot")
                        .HasColumnType("datetime");

                    b.Property<DateTime>("LeverbaarVanaf")
                        .HasColumnName("prod_leverbaarvanaf")
                        .HasColumnType("datetime");

                    b.Property<string>("Naam")
                        .IsRequired()
                        .HasColumnName("prod_naam")
                        .HasMaxLength(250);

                    b.Property<decimal>("Prijs")
                        .HasColumnName("prod_prijs")
                        .HasColumnType("money");

                    b.HasKey("Id")
                        .HasName("PK_product");

                    b.HasIndex("LeverancierId");

                    b.ToTable("product");
                });

            modelBuilder.Entity("catalogus_events.Model.ProductCategorie", b =>
                {
                    b.Property<int>("ProdcatProdId")
                        .HasColumnName("prodcat_prod_id");

                    b.Property<int>("ProdcatCatId")
                        .HasColumnName("prodcat_cat_id");

                    b.HasKey("ProdcatProdId", "ProdcatCatId")
                        .HasName("PK__product___3E2432AF1273C1CD");

                    b.HasIndex("ProdcatCatId");

                    b.ToTable("product_categorie");
                });

            modelBuilder.Entity("catalogus_events.Model.Product", b =>
                {
                    b.HasOne("catalogus_events.Model.Leverancier", "Leverancier")
                        .WithMany("Product")
                        .HasForeignKey("LeverancierId")
                        .HasConstraintName("FK_product_leverancier");
                });

            modelBuilder.Entity("catalogus_events.Model.ProductCategorie", b =>
                {
                    b.HasOne("catalogus_events.Model.Categorie", "Categorie")
                        .WithMany("Producten")
                        .HasForeignKey("ProdcatCatId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("catalogus_events.Model.Product", "Product")
                        .WithMany("Categorieen")
                        .HasForeignKey("ProdcatProdId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
