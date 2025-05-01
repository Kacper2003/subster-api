using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subster.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Invoicev2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CycleNumber",
                table: "SubscriptionInvoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "SubscriptionInvoices",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CycleNumber",
                table: "SubscriptionInvoices");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "SubscriptionInvoices");
        }
    }
}
