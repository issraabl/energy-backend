namespace EnergyTrackerr.DTOs
{
    public class ParametresDetectionDto
    {
        public double SeuilEcartMoyenne { get; set; } = 30.0;

        public double SeuilPicSoudain { get; set; } = 50.0;

        public int NombreMesuresContexte { get; set; } = 30;
    }
}