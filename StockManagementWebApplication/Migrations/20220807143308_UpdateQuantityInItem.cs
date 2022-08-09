using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManagementWebApplication.Migrations
{
    public partial class UpdateQuantityInItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Quantity",
                table: "Items",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Items");
        }
    }
}
