using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShoppingZone.Order.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantIdToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MerchantId",
                table: "Orders",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "Orders");
        }
    }
}
