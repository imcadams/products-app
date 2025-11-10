using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductCatalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchOptimizationIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_IsActive_Price",
                table: "Products",
                columns: new[] { "CategoryId", "IsActive", "Price" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_CreatedDate",
                table: "Products",
                columns: new[] { "IsActive", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_Price",
                table: "Products",
                columns: new[] { "IsActive", "Price" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_StockQuantity",
                table: "Products",
                columns: new[] { "IsActive", "StockQuantity" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_IsActive_Price",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_CreatedDate",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_Price",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_StockQuantity",
                table: "Products");
        }
    }
}
