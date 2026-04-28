using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShoppingZone.Wallet.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    WalletId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.WalletId);
                });

            migrationBuilder.CreateTable(
                name: "Statements",
                columns: table => new
                {
                    StatementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    TransactionRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BalanceAfterTransaction = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RazorpayPaymentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RazorpayOrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WalletId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statements", x => x.StatementId);
                    table.ForeignKey(
                        name: "FK_Statements_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Statement_OrderId",
                table: "Statements",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Statement_RazorpayPaymentId",
                table: "Statements",
                column: "RazorpayPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Statement_TransactionDate",
                table: "Statements",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Statement_WalletId",
                table: "Statements",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_CustomerId",
                table: "Wallets",
                column: "CustomerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Statements");

            migrationBuilder.DropTable(
                name: "Wallets");
        }
    }
}
