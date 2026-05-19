using ClosedXML.Excel;
using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using Microsoft.EntityFrameworkCore;

namespace EnergyTrackerr.Services
{
    public class ExcelImportService
    {
        private readonly AppDbContext _db;

        public ExcelImportService(AppDbContext db) => _db = db;

        public async Task<int> ImporterFichier(IFormFile file)
        {
            // Charger les lookups depuis la base
            var energies = await _db.Energies.ToListAsync();
            var equipements = await _db.Equipements.ToListAsync();

            var mesures = new List<Mesure>();

            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheet("Mesures");

            foreach (var row in ws.RowsUsed().Skip(1)) // ignorer l'en-tête
            {
                var energieNom = row.Cell(4).GetString().Trim();  // colonne Énergie
                var equipementNom = row.Cell(7).GetString().Trim();  // colonne Équipement
                var dateStr = row.Cell(8).GetString().Trim();  // colonne Date mesure

                // Résoudre l'énergie par son Nom
                var energie = energies.FirstOrDefault(e =>
                    e.Nom.Equals(energieNom, StringComparison.OrdinalIgnoreCase));

                if (energie == null) continue; // ligne ignorée si énergie inconnue

                // Résoudre l'équipement par son Nom (nullable)
                var equipement = equipements.FirstOrDefault(e =>
                    e.Nom.Equals(equipementNom, StringComparison.OrdinalIgnoreCase));

                // Parser la date
                if (!DateTime.TryParseExact(dateStr, "dd/MM/yyyy HH:mm",
                        null, System.Globalization.DateTimeStyles.None, out var date))
                    continue; // ligne ignorée si date invalide

                mesures.Add(new Mesure
                {
                    Valeur = row.Cell(2).GetDouble(),
                    DateMesure = date,
                    DateCreation = DateTime.Now,
                    SourceDonnee = row.Cell(6).GetString().Trim(),
                    EnergieId = energie.IdEnergie,
                    EquipementId = equipement?.IdEquipement
                });
            }

            await _db.Mesures.AddRangeAsync(mesures);
            await _db.SaveChangesAsync();
            return mesures.Count;
        }
    }
}