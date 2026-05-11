namespace EnergyTrackerr.DTOs
{
    public class AnomalieResultatDto
    {
        public int MesureId { get; set; }
        public int EquipementId { get; set; }
        public string EquipementNom { get; set; } = string.Empty;
        public double Valeur { get; set; }
        public DateTime DateMesure { get; set; }
        public bool EstAnomalie { get; set; }

        public double? SeuilDynamique { get; set; }

        public List<AnomalieDetecteeDto> Anomalies { get; set; } = new();
        public StatistiqueDto Contexte { get; set; } = new();
    }
}
