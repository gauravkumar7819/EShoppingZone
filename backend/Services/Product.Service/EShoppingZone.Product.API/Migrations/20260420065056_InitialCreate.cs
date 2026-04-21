using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EShoppingZone.Product.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MRP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RatingJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecificationsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "Category", "CreatedAt", "Description", "ImagesJson", "IsActive", "MRP", "MerchantId", "Name", "Price", "RatingJson", "ReviewJson", "SpecificationsJson", "StockQuantity", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Apple", "Electronics", new DateTime(2026, 4, 20, 6, 50, 54, 856, DateTimeKind.Utc).AddTicks(8683), "Latest Apple iPhone with A17 Pro chip", "[]", true, 150000m, 1, "iPhone 15 Pro", 120000m, "{}", "{}", "{}", 50, "Mobile", null },
                    { 2, "Penguin", "Books", new DateTime(2026, 4, 20, 6, 50, 54, 856, DateTimeKind.Utc).AddTicks(8686), "Classic novel by F. Scott Fitzgerald", "[]", true, 499m, 2, "The Great Gatsby", 299m, "{}", "{}", "{}", 100, "Fiction", null },
                    { 3, "Nike", "Apparel", new DateTime(2026, 4, 20, 6, 50, 54, 856, DateTimeKind.Utc).AddTicks(8688), "Comfortable running shoes", "[]", true, 12999m, 2, "Nike Air Max", 8999m, "{}", "{}", "{}", 75, "Shoes", null },
                    { 4, "Dove", "Personal Care", new DateTime(2026, 4, 20, 6, 50, 54, 856, DateTimeKind.Utc).AddTicks(8691), "Nourishing hair care", "[]", true, 450m, 3, "Dove Shampoo", 350m, "{}", "{}", "{}", 200, "Hair Care", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Product_Category",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Category_Active",
                table: "Products",
                columns: new[] { "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Product_MerchantId",
                table: "Products",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Type",
                table: "Products",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
