namespace EnergyTrackerr.DTOs
{
    public class StatistiqueDto
    {
        public string EnergieType { get; set; } = "";
        public double Total { get; set; }
        public double Moyenne { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }
}