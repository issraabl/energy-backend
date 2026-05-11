using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyTrackerr.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Energies",
                columns: table => new
                {
                    IdEnergie = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnergieType = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Debit = table.Column<double>(type: "float", nullable: true),
                    Tension = table.Column<double>(type: "float", nullable: true),
                    Puissance = table.Column<double>(type: "float", nullable: true),
                    Volume = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Energies", x => x.IdEnergie);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.IdUtilisateur);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                columns: table => new
                {
                    IdZone = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.IdZone);
                    table.ForeignKey(
                        name: "FK_Zones_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Equipements",
                columns: table => new
                {
                    IdEquipement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeEquipement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    EnergieId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipements", x => x.IdEquipement);
                    table.ForeignKey(
                        name: "FK_Equipements_Energies_EnergieId",
                        column: x => x.EnergieId,
                        principalTable: "Energies",
                        principalColumn: "IdEnergie",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Equipements_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "IdZone",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alertes",
                columns: table => new
                {
                    IdAlerte = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Seuil = table.Column<double>(type: "float", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EquipementId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alertes", x => x.IdAlerte);
                    table.ForeignKey(
                        name: "FK_Alertes_Equipements_EquipementId",
                        column: x => x.EquipementId,
                        principalTable: "Equipements",
                        principalColumn: "IdEquipement",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Mesures",
                columns: table => new
                {
                    IdMesure = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Valeur = table.Column<double>(type: "float", nullable: false),
                    DateMesure = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceDonnee = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnergieId = table.Column<int>(type: "int", nullable: false),
                    EquipementId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesures", x => x.IdMesure);
                    table.ForeignKey(
                        name: "FK_Mesures_Energies_EnergieId",
                        column: x => x.EnergieId,
                        principalTable: "Energies",
                        principalColumn: "IdEnergie",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mesures_Equipements_EquipementId",
                        column: x => x.EquipementId,
                        principalTable: "Equipements",
                        principalColumn: "IdEquipement",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Analyses",
                columns: table => new
                {
                    IdAnalyse = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeAnalyse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resultat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateAnalyse = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AlerteId = table.Column<int>(type: "int", nullable: true),
                    UtilisateurId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analyses", x => x.IdAnalyse);
                    table.ForeignKey(
                        name: "FK_Analyses_Alertes_AlerteId",
                        column: x => x.AlerteId,
                        principalTable: "Alertes",
                        principalColumn: "IdAlerte");
                    table.ForeignKey(
                        name: "FK_Analyses_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                });

            migrationBuilder.CreateTable(
                name: "Rapports",
                columns: table => new
                {
                    IdRapport = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contenu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnalyseId = table.Column<int>(type: "int", nullable: true),
                    UtilisateurId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rapports", x => x.IdRapport);
                    table.ForeignKey(
                        name: "FK_Rapports_Analyses_AnalyseId",
                        column: x => x.AnalyseId,
                        principalTable: "Analyses",
                        principalColumn: "IdAnalyse");
                    table.ForeignKey(
                        name: "FK_Rapports_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                });

            migrationBuilder.CreateTable(
                name: "Recommandations",
                columns: table => new
                {
                    IdRecommandation = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Texte = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AlerteId = table.Column<int>(type: "int", nullable: true),
                    AnalyseId = table.Column<int>(type: "int", nullable: true),
                    UtilisateurId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommandations", x => x.IdRecommandation);
                    table.ForeignKey(
                        name: "FK_Recommandations_Alertes_AlerteId",
                        column: x => x.AlerteId,
                        principalTable: "Alertes",
                        principalColumn: "IdAlerte");
                    table.ForeignKey(
                        name: "FK_Recommandations_Analyses_AnalyseId",
                        column: x => x.AnalyseId,
                        principalTable: "Analyses",
                        principalColumn: "IdAnalyse");
                    table.ForeignKey(
                        name: "FK_Recommandations_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alertes_EquipementId",
                table: "Alertes",
                column: "EquipementId");

            migrationBuilder.CreateIndex(
                name: "IX_Analyses_AlerteId",
                table: "Analyses",
                column: "AlerteId");

            migrationBuilder.CreateIndex(
                name: "IX_Analyses_UtilisateurId",
                table: "Analyses",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipements_EnergieId",
                table: "Equipements",
                column: "EnergieId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipements_ZoneId",
                table: "Equipements",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Mesures_EnergieId",
                table: "Mesures",
                column: "EnergieId");

            migrationBuilder.CreateIndex(
                name: "IX_Mesures_EquipementId",
                table: "Mesures",
                column: "EquipementId");

            migrationBuilder.CreateIndex(
                name: "IX_Rapports_AnalyseId",
                table: "Rapports",
                column: "AnalyseId");

            migrationBuilder.CreateIndex(
                name: "IX_Rapports_UtilisateurId",
                table: "Rapports",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommandations_AlerteId",
                table: "Recommandations",
                column: "AlerteId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommandations_AnalyseId",
                table: "Recommandations",
                column: "AnalyseId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommandations_UtilisateurId",
                table: "Recommandations",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Zones_SiteId",
                table: "Zones",
                column: "SiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mesures");

            migrationBuilder.DropTable(
                name: "Rapports");

            migrationBuilder.DropTable(
                name: "Recommandations");

            migrationBuilder.DropTable(
                name: "Analyses");

            migrationBuilder.DropTable(
                name: "Alertes");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Equipements");

            migrationBuilder.DropTable(
                name: "Energies");

            migrationBuilder.DropTable(
                name: "Zones");

            migrationBuilder.DropTable(
                name: "Sites");
        }
    }
}
