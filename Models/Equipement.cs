namespace EnergyTrackerr.Models
{
    public class Equipement
    {
        public int IdEquipement { get; set; }
        public string Nom { get; set; } = "";
        public string TypeEquipement { get; set; } = "";
        public string Statut { get; set; } = "Actif";           
        public double? Puissance { get; set; }                 
        public string? Localisation { get; set; }               
        public string? Description { get; set; }               
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateMiseEnService { get; set; }
        public DateTime? DateInstallation { get; set; }        
        public int? ZoneId { get; set; }                       
        public Zone? Zone { get; set; }
        public int? EnergieId { get; set; }                    
        public Energie? Energie { get; set; }
        public List<Mesure> Mesures { get; set; } = new();
        public List<Alertes> Alertes { get; set; } = new();
    }
}