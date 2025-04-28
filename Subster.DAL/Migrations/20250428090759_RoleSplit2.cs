using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subster.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RoleSplit2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationInMonths",
                table: "Program",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "Program",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationInMonths",
                table: "Program");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Program");
        }
    }
}
