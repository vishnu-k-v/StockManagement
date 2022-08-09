using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManagementWebApplication.Migrations
{
    public partial class AddedOrderIdAndRate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Rate",
                table: "OrderItems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1,
                column: "Quantity",
                value: 10.0);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2,
                column: "Quantity",
                value: 10.0);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "OrderItems");

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1,
                column: "Quantity",
                value: 0.0);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2,
                column: "Quantity",
                value: 0.0);
        }
    }
}
