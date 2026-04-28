using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShoppingZone.Order.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEstimatedDeliveryDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryDate",
                table: "Orders");
        }
    }
}
