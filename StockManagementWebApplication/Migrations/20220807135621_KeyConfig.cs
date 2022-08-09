using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManagementWebApplication.Migrations
{
    public partial class KeyConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "Name", "Rate" },
                values: new object[] { 1, "Apple", 50.0 });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "Name", "Rate" },
                values: new object[] { 2, "Orange", 30.0 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
