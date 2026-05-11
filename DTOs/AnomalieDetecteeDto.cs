namespace EnergyTrackerr.DTOs
{
    public class AnomalieDetecteeDto
    {
        public string Type { get; set; } = string.Empty;
        public string Severite { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public double? ValeurReference { get; set; }
        public double? EcartPourcentage { get; set; }
    }
}
