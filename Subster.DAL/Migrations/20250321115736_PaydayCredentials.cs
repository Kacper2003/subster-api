using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subster.DAL.Migrations
{
    /// <inheritdoc />
    public partial class PaydayCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaydayClientId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaydayClientSecret",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaydayClientId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PaydayClientSecret",
                table: "Users");
        }
    }
}
