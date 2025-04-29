using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subster.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionsFull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientSsn",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Subscriptions",
                newName: "StartDate");

            migrationBuilder.AddColumn<int>(
                name: "DurationInMonths",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Subscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationInMonths",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Subscriptions",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "ClientSsn",
                table: "Subscriptions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
