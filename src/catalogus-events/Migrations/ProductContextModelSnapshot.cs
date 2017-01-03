using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using catalogus_events.Model;

namespace catalogusevents.Migrations
{
    [DbContext(typeof(ProductContext))]
    partial class ProductContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("catalogus_events.Model.Categorie", b =>
                {
                    b.Property<int>("CatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("cat_id");

                    b.Property<string>("CatNaam")
                        .IsRequired()
                        .HasColumnName("cat_naam")
                        .HasMaxLength(150);

                    b.HasKey("CatId")
                        .HasName("PK_categorie");

                    b.ToTable("categorie");
                });

            modelBuilder.Entity("catalogus_events.Model.Leverancier", b =>
                {
                    b.Property<int>("LevId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("lev_id");

                    b.Property<string>("LevNaam")
                        .IsRequired()
                        .HasColumnName("lev_naam")
                        .HasMaxLength(200);

                    b.HasKey("LevId")
                        .HasName("PK_leverancier");

                    b.ToTable("leverancier");
                });

            modelBuilder.Entity("catalogus_events.Model.Product", b =>
                {
                    b.Property<int>("ProdId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("prod_id");

                    b.Property<string>("ProdAfbeeldingurl")
                        .IsRequired()
                        .HasColumnName("prod_afbeeldingurl")
                        .HasMaxLength(500);

                    b.Property<string>("ProdBeschrijving")
                        .IsRequired()
                        .HasColumnName("prod_beschrijving");

                    b.Property<int>("ProdLevId")
                        .HasColumnName("prod_lev_id");

                    b.Property<string>("ProdLeveranciersproductid")
                        .IsRequired()
                        .HasColumnName("prod_leveranciersproductid")
                        .HasMaxLength(150);

                    b.Property<DateTime?>("ProdLeverbaartot")
                        .HasColumnName("prod_leverbaartot")
                        .HasColumnType("datetime");

                    b.Property<DateTime>("ProdLeverbaarvanaf")
                        .HasColumnName("prod_leverbaarvanaf")
                        .HasColumnType("datetime");

                    b.Property<string>("ProdNaam")
                        .IsRequired()
                        .HasColumnName("prod_naam")
                        .HasMaxLength(250);

                    b.Property<decimal>("ProdPrijs")
                        .HasColumnName("prod_prijs")
                        .HasColumnType("money");

                    b.HasKey("ProdId")
                        .HasName("PK_product");

                    b.HasIndex("ProdLevId");

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

                    b.ToTable("product_categorie");
                });

            modelBuilder.Entity("catalogus_events.Model.Product", b =>
                {
                    b.HasOne("catalogus_events.Model.Leverancier", "ProdLev")
                        .WithMany("Product")
                        .HasForeignKey("ProdLevId")
                        .HasConstraintName("FK_product_leverancier");
                });
        }
    }
}
