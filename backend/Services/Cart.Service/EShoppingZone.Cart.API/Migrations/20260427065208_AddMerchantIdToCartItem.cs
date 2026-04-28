using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShoppingZone.Cart.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantIdToCartItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MerchantId",
                table: "CartItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "CartItems");
        }
    }
}
