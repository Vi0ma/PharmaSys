using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaSys.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderNumberToSupplierOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "SupplierOrders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "SupplierOrders");
        }
    }
}
