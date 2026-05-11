using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using Microsoft.EntityFrameworkCore;

namespace EnergyTrackerr.Services
{
    public class AlerteService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<AlerteService> _logger;

        public AlerteService(
            AppDbContext context,
            EmailService emailService,
            ILogger<AlerteService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Alertes?> VerifierMesureEtCreerAlerte(Mesure mesure)
        {
            Console.WriteLine($"🔍 VerifierMesureEtCreerAlerte — Valeur={mesure.Valeur}, EnergieId={mesure.EnergieId}");

            // ✅ 1. Chercher un seuil fixe pour cette énergie
            var seuilFixe = await _context.Seuils
                .FirstOrDefaultAsync(s => s.EnergieId == mesure.EnergieId);

            double seuilUtilise;
            string methode;

            if (seuilFixe != null)
            {
                // ✅ Seuil fixe trouvé → on l'utilise directement
                seuilUtilise = seuilFixe.Valeur;
                methode = "fixe";
                Console.WriteLine($"📌 Seuil fixe trouvé : {seuilUtilise}");
            }
            else
            {
                // 🔁 Fallback : calcul dynamique
                Console.WriteLine("⚠️ Aucun seuil fixe — fallback calcul dynamique");

                var mesuresHistoriques = await _context.Mesures
                    .Where(m => m.EnergieId == mesure.EnergieId &&
                                m.IdMesure != mesure.IdMesure)
                    .Select(m => m.Valeur)
                    .ToListAsync();

                if (!mesuresHistoriques.Any())
                {
                    Console.WriteLine("⚠️ Pas assez d'historique — aucune alerte créée");
                    _logger.LogInformation(
                        "Pas assez d'historique pour EnergieId={EnergieId}, aucune alerte créée.",
                        mesure.EnergieId);
                    return null;
                }

                double moyenne = mesuresHistoriques.Average();
                double ecartType = Math.Sqrt(
                    mesuresHistoriques.Average(v => Math.Pow(v - moyenne, 2)));
                seuilUtilise = moyenne + ecartType;
                methode = "dynamique";

                Console.WriteLine($"📈 Moyenne={Math.Round(moyenne, 2)} | " +
                                  $"Écart-type={Math.Round(ecartType, 2)} | " +
                                  $"Seuil={Math.Round(seuilUtilise, 2)}");
            }

            Console.WriteLine($"📊 Valeur={mesure.Valeur} | " +
                              $"Seuil ({methode})={Math.Round(seuilUtilise, 2)} | " +
                              $"Dépasse={mesure.Valeur > seuilUtilise}");

            // ✅ 2. Comparer
            if (mesure.Valeur <= seuilUtilise)
            {
                Console.WriteLine("✅ Valeur normale — pas d'alerte");
                return null;
            }

            // ✅ 3. Créer l'alerte en DB
            var energie = await _context.Energies.FindAsync(mesure.EnergieId);

            var alerte = new Alertes
            {
                Type = "Anomalie Consommation",
                Seuil = Math.Round(seuilUtilise, 2),
                Message = methode == "fixe"
                    ? $"Consommation anormale détectée : {mesure.Valeur} " +
                      $"(seuil fixe défini : {seuilUtilise})"
                    : $"Consommation anormale détectée : {mesure.Valeur} " +
                      $"(seuil dynamique : {Math.Round(seuilUtilise, 2)})",
                EquipementId = mesure.EquipementId,
                DateCreation = DateTime.Now
            };

            _context.Alertes.Add(alerte);
            await _context.SaveChangesAsync();

            Console.WriteLine($"🚨 Alerte créée IdAlerte={alerte.IdAlerte} via seuil {methode}");
            _logger.LogInformation(
                "Alerte créée IdAlerte={IdAlerte} via seuil {Methode}",
                alerte.IdAlerte, methode);

            // ✅ 4. Envoyer les emails
            await EnvoyerEmailsAlerteAsync(
                alerte,
                mesure.Valeur,
                seuilUtilise,
                energie?.Nom ?? $"Énergie {mesure.EnergieId}");

            return alerte;
        }

        private async Task EnvoyerEmailsAlerteAsync(
            Alertes alerte,
            double valeur,
            double seuil,
            string energieNom)
        {
            Console.WriteLine("📬 EnvoyerEmailsAlerteAsync appelé");

            var utilisateurs = await _context.Utilisateurs
                .Where(u => u.Role == "responsable_energie" ||
                            u.Role == "administrateur")
                .ToListAsync();

            Console.WriteLine($"👥 Destinataires trouvés : {utilisateurs.Count}");

            if (!utilisateurs.Any())
            {
                Console.WriteLine("⚠️ Aucun destinataire trouvé !");
                _logger.LogWarning(
                    "Alerte {IdAlerte} créée mais aucun responsable/admin trouvé.",
                    alerte.IdAlerte);
                return;
            }

            foreach (var utilisateur in utilisateurs)
            {
                try
                {
                    Console.WriteLine($"📧 Envoi vers {utilisateur.Email}...");

                    await _emailService.EnvoyerEmailAlerteAsync(
                        destinataireEmail: utilisateur.Email,
                        destinataireNom: utilisateur.Nom,
                        typeAlerte: alerte.Type,
                        valeur: valeur,
                        seuil: seuil,
                        energieNom: energieNom);

                    Console.WriteLine($"✅ Email envoyé à {utilisateur.Email}");
                    _logger.LogInformation(
                        "Email envoyé à {Email} pour alerte {IdAlerte}.",
                        utilisateur.Email, alerte.IdAlerte);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ ERREUR envoi à {utilisateur.Email} : {ex.Message}");
                    _logger.LogError(ex,
                        "Échec envoi email à {Email} pour alerte {IdAlerte}.",
                        utilisateur.Email, alerte.IdAlerte);
                }
            }
        }
    }
}