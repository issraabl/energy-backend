using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyTrackerr.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipementField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipements_Energies_EnergieId",
                table: "Equipements");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipements_Zones_ZoneId",
                table: "Equipements");

            migrationBuilder.AlterColumn<int>(
                name: "ZoneId",
                table: "Equipements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EnergieId",
                table: "Equipements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateInstallation",
                table: "Equipements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Equipements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Localisation",
                table: "Equipements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Puissance",
                table: "Equipements",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Statut",
                table: "Equipements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipements_Energies_EnergieId",
                table: "Equipements",
                column: "EnergieId",
                principalTable: "Energies",
                principalColumn: "IdEnergie");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipements_Zones_ZoneId",
                table: "Equipements",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "IdZone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipements_Energies_EnergieId",
                table: "Equipements");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipements_Zones_ZoneId",
                table: "Equipements");

            migrationBuilder.DropColumn(
                name: "DateInstallation",
                table: "Equipements");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Equipements");

            migrationBuilder.DropColumn(
                name: "Localisation",
                table: "Equipements");

            migrationBuilder.DropColumn(
                name: "Puissance",
                table: "Equipements");

            migrationBuilder.DropColumn(
                name: "Statut",
                table: "Equipements");

            migrationBuilder.AlterColumn<int>(
                name: "ZoneId",
                table: "Equipements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EnergieId",
                table: "Equipements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipements_Energies_EnergieId",
                table: "Equipements",
                column: "EnergieId",
                principalTable: "Energies",
                principalColumn: "IdEnergie",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipements_Zones_ZoneId",
                table: "Equipements",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "IdZone",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
