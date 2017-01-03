using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace catalogusevents.Migrations
{
    public partial class foreignkey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_product_categorie_prodcat_cat_id",
                table: "product_categorie",
                column: "prodcat_cat_id");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_categorie_categorie_prodcat_cat_id",
                table: "product_categorie");

            migrationBuilder.DropForeignKey(
                name: "FK_product_categorie_product_prodcat_prod_id",
                table: "product_categorie");

            migrationBuilder.DropIndex(
                name: "IX_product_categorie_prodcat_cat_id",
                table: "product_categorie");
        }
    }
}
