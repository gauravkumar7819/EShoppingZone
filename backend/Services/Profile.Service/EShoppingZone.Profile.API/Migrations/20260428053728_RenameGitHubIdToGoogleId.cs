using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShoppingZone.Profile.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameGitHubIdToGoogleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_GitHubId",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "GitHubId",
                table: "UserProfiles",
                newName: "GoogleId");

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 28, 5, 37, 26, 586, DateTimeKind.Utc).AddTicks(9333), "AQAAAAIAAYagAAAAEFyjPTyT3XrXkFWad/9uLLmMsTuv2QskCCZ+I946rK6oJSK29A0rVt6VRT6MBNpZWA==" });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_GoogleId",
                table: "UserProfiles",
                column: "GoogleId",
                unique: true,
                filter: "[GoogleId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_GoogleId",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "GoogleId",
                table: "UserProfiles",
                newName: "GitHubId");

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 20, 5, 46, 52, 885, DateTimeKind.Utc).AddTicks(190), "AQAAAAIAAYagAAAAEKxkyGDbAfKsq/5AETGSc0Xf8l+UiUOmcK1IDcBIwdeoESJu37i4XyTLd3hd+oZYGw==" });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_GitHubId",
                table: "UserProfiles",
                column: "GitHubId",
                unique: true,
                filter: "[GitHubId] IS NOT NULL");
        }
    }
}
