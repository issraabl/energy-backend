using System.ComponentModel.DataAnnotations;

namespace EnergyTrackerr.Models
{
    public class AnalyseEnergetique
    {
        [Key]
        public int IdAnalyse { get; set; }

        [Required]
        public string TypeAnalyse { get; set; }

        [Required]
        public string Resultat { get; set; }

        public DateTime DateAnalyse { get; set; } = DateTime.Now;

        public int? AlerteId { get; set; }
        public Alertes? Alerte { get; set; }

        public int? UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }
    }
}