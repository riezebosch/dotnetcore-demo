using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace catalogusevents.Migrations
{
    public partial class foreignkeyfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_categorie_categorie_prodcat_cat_id",
                table: "product_categorie");

            migrationBuilder.DropForeignKey(
                name: "FK_product_categorie_product_prodcat_prod_id",
                table: "product_categorie");

            migrationBuilder.DropPrimaryKey(
                name: "PK__product___3E2432AF1273C1CD",
                table: "product_categorie");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_categorie",
                table: "product_categorie",
                columns: new[] { "prodcat_prod_id", "prodcat_cat_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategorie_Categorie",
                table: "product_categorie",
                column: "prodcat_cat_id",
                principalTable: "categorie",
                principalColumn: "cat_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategorie_Product",
                table: "product_categorie",
                column: "prodcat_prod_id",
                principalTable: "product",
                principalColumn: "prod_id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategorie_Categorie",
                table: "product_categorie");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategorie_Product",
                table: "product_categorie");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_categorie",
                table: "product_categorie");

            migrationBuilder.AddPrimaryKey(
                name: "PK__product___3E2432AF1273C1CD",
                table: "product_categorie",
                columns: new[] { "prodcat_prod_id", "prodcat_cat_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_product_categorie_categorie_prodcat_cat_id",
                table: "product_categorie",
                column: "prodcat_cat_id",
                principalTable: "categorie",
                principalColumn: "cat_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_categorie_product_prodcat_prod_id",
                table: "product_categorie",
                column: "prodcat_prod_id",
                principalTable: "product",
                principalColumn: "prod_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
