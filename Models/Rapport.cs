using System.ComponentModel.DataAnnotations;

namespace EnergyTrackerr.Models
{
    public class Rapport
    {
        [Key]
        public int IdRapport { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        [Required]
        public string Type { get; set; }

        [Required]
        public string Contenu { get; set; }

        public int? AnalyseId { get; set; }
        public AnalyseEnergetique? Analyse { get; set; }

        public int? UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }
    }
}