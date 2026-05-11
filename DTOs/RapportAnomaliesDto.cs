namespace EnergyTrackerr.DTOs
{
    public class RapportAnomaliesDto
    {
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }

        public int TotalMesuresAnalysees { get; set; }
        public int TotalAnomaliesDetectees { get; set; }
        public double TauxAnomalies { get; set; }

        public List<AnomalieResultatDto> Anomalies { get; set; } = new();

        public Dictionary<string, int> AnomaliesParType { get; set; } = new();
        public Dictionary<string, int> AnomaliesParSeverite { get; set; } = new();
    }
}
