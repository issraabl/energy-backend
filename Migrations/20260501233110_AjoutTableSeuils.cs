using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyTrackerr.Migrations
{
    /// <inheritdoc />
    public partial class AjoutTableSeuils : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnergieId",
                table: "Alertes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Severite",
                table: "Alertes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Traite",
                table: "Alertes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Alertes_EnergieId",
                table: "Alertes",
                column: "EnergieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alertes_Energies_EnergieId",
                table: "Alertes",
                column: "EnergieId",
                principalTable: "Energies",
                principalColumn: "IdEnergie");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alertes_Energies_EnergieId",
                table: "Alertes");

            migrationBuilder.DropIndex(
                name: "IX_Alertes_EnergieId",
                table: "Alertes");

            migrationBuilder.DropColumn(
                name: "EnergieId",
                table: "Alertes");

            migrationBuilder.DropColumn(
                name: "Severite",
                table: "Alertes");

            migrationBuilder.DropColumn(
                name: "Traite",
                table: "Alertes");
        }
    }
}
