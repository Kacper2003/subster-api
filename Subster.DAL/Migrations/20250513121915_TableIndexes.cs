using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subster.DAL.Migrations
{
    /// <inheritdoc />
    public partial class TableIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Trainer_Ssn",
                table: "Trainers",
                column: "Ssn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Client_Ssn",
                table: "Clients",
                column: "Ssn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trainer_Ssn",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Client_Ssn",
                table: "Clients");
        }
    }
}
