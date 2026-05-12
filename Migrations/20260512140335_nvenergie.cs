using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyTrackerr.Migrations
{
    /// <inheritdoc />
    public partial class nvenergie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EnergieType",
                table: "Energies",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13);

            migrationBuilder.AddColumn<string>(
                name: "Couleur",
                table: "Energies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Energies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FacteurConversion",
                table: "Energies",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Couleur",
                table: "Energies");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Energies");

            migrationBuilder.DropColumn(
                name: "FacteurConversion",
                table: "Energies");

            migrationBuilder.AlterColumn<string>(
                name: "EnergieType",
                table: "Energies",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(21)",
                oldMaxLength: 21);
        }
    }
}
