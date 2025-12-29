using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaSys.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitPriceToSupplierOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "SupplierOrderItems");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "SupplierOrderItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "SupplierOrderItems");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "SupplierOrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
