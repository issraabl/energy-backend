using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyTrackerr.Migrations
{
    /// <inheritdoc />
    public partial class sitenv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Sites",
                newName: "IdSite");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdSite",
                table: "Sites",
                newName: "Id");
        }
    }
}
