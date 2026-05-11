using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using Microsoft.EntityFrameworkCore;

namespace EnergyTrackerr.Services
{
    public class StatistiqueService
    {
        private readonly AppDbContext _context;
        private readonly AlerteService _alerteService;

        // Facteur multiplicatif pour le seuil dynamique (moyenne + facteur × écart type)
        private readonly double facteur = 1.5;

        public StatistiqueService(AppDbContext context, AlerteService alerteService)
        {
            _context = context;
            _alerteService = alerteService;
        }

        // Vérifie une mesure et crée une alerte si elle dépasse le seuil statistique
        public async Task VerifierMesureEtCreerAlerteStatistique(Mesure mesure)
        {
            // Récupérer toutes les mesures précédentes pour la même énergie et équipement
            var mesuresHistoriques = await _context.Mesures
                .Where(m => m.EnergieId == mesure.EnergieId && m.EquipementId == mesure.EquipementId && m.IdMesure != mesure.IdMesure)
                .Select(m => m.Valeur)
                .ToListAsync();

            if (!mesuresHistoriques.Any())
            {
                // Pas assez de données historiques pour calculer statistique
                return;
            }

            // Calcul de la moyenne et de l’écart type
            double moyenne = mesuresHistoriques.Average();
            double variance = mesuresHistoriques.Average(v => Math.Pow(v - moyenne, 2));
            double ecartType = Math.Sqrt(variance);

            // Seuil dynamique
            double seuilDynamique = moyenne + facteur * ecartType;

            // Création d’alerte si la mesure dépasse le seuil
            if (mesure.Valeur > seuilDynamique)
            {
                await _alerteService.VerifierMesureEtCreerAlerte(mesure);
                // Ici, ton AlerteService créera automatiquement l’alerte
            }
        }
    }
}