namespace EnergyTrackerr.Models
{
    public class Alertes
    {
        public int IdAlerte { get; set; }
        public string Type { get; set; } = "";
        public double Seuil { get; set; }
        public string Message { get; set; } = "";
        public string Severite { get; set; } = "Normale";
        public bool Traite { get; set; } = false;
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // ✅ AJOUT — lien vers l'énergie concernée
        public int? EnergieId { get; set; }
        public Energie? Energie { get; set; }

        // Relation optionnelle avec Equipement
        public int? EquipementId { get; set; }
        public Equipement? Equipement { get; set; }
    }
}