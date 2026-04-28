using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShoppingZone.Product.API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeImagesToImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagesJson",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ImageUrl" },
                values: new object[] { new DateTime(2026, 4, 27, 5, 50, 50, 972, DateTimeKind.Utc).AddTicks(8372), "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ImageUrl" },
                values: new object[] { new DateTime(2026, 4, 27, 5, 50, 50, 972, DateTimeKind.Utc).AddTicks(8375), "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "ImageUrl" },
                values: new object[] { new DateTime(2026, 4, 27, 5, 50, 50, 972, DateTimeKind.Utc).AddTicks(8378), "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "ImageUrl" },
                values: new object[] { new DateTime(2026, 4, 27, 5, 50, 50, 972, DateTimeKind.Utc).AddTicks(8380), "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "ImagesJson",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ImagesJson" },
                values: new object[] { new DateTime(2026, 4, 20, 6, 50, 54, 856, DateTimeKind.Utc).AddTicks(8683), "[]" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ImagesJson" },
                values: new object[] { new DateTime(2026, 4, 20, 6, 50, 54, 856, DateTimeKind.Utc).AddTicks(8686), "[]" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "ImagesJson" },
                values: new object[] { new DateTime(2026, 4, 20, 6, 50, 54, 856, DateTimeKind.Utc).AddTicks(8688), "[]" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "ImagesJson" },
                values: new object[] { new DateTime(2026, 4, 20, 6, 50, 54, 856, DateTimeKind.Utc).AddTicks(8691), "[]" });
        }
    }
}
