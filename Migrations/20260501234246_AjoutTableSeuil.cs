using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyTrackerr.Migrations
{
    /// <inheritdoc />
    public partial class AjoutTableSeuil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Seuils",
                columns: table => new
                {
                    IdSeuil = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnergieId = table.Column<int>(type: "int", nullable: false),
                    Valeur = table.Column<double>(type: "float", nullable: false),
                    Periode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seuils", x => x.IdSeuil);
                    table.ForeignKey(
                        name: "FK_Seuils_Energies_EnergieId",
                        column: x => x.EnergieId,
                        principalTable: "Energies",
                        principalColumn: "IdEnergie",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Seuils_EnergieId",
                table: "Seuils",
                column: "EnergieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Seuils");
        }
    }
}
