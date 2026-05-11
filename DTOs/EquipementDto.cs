namespace EnergyTrackerr.DTOs
{
    public class EquipementDto
    {
        public string Nom { get; set; } = "";
        public string TypeEquipement { get; set; } = "";
        public string Statut { get; set; } = "Actif";
        public double? Puissance { get; set; }
        public string? Localisation { get; set; }
        public string? Description { get; set; }
        public DateTime? DateMiseEnService { get; set; }
        public int? ZoneId { get; set; }
        public int? EnergieId { get; set; }
    }
}
