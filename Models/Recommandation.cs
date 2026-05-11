using System.ComponentModel.DataAnnotations;

namespace EnergyTrackerr.Models
{
    public class Recommandation
    {
        [Key]
        public int IdRecommandation { get; set; }

        [Required]
        public string Texte { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public int? AlerteId { get; set; }
        public Alertes? Alerte { get; set; }

        public int? AnalyseId { get; set; }
        public AnalyseEnergetique? Analyse { get; set; }

        public int? UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }
    }
}