using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subster.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Programs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Program");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Program",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPriceExcludingVat",
                table: "Program",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPriceIncludingVat",
                table: "Program",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatPercentage",
                table: "Program",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Program");

            migrationBuilder.DropColumn(
                name: "UnitPriceExcludingVat",
                table: "Program");

            migrationBuilder.DropColumn(
                name: "UnitPriceIncludingVat",
                table: "Program");

            migrationBuilder.DropColumn(
                name: "VatPercentage",
                table: "Program");

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "Program",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
