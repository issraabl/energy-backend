namespace EnergyTrackerr.Models
{
    public class Anomalie
    {
        public int Id { get; set; }
        public int MesureId { get; set; } 
        public Mesure Mesure { get; set; }

        public string Description { get; set; } = ""; 
        public DateTime DateDetection { get; set; } = DateTime.Now;

        public bool Resolu { get; set; } = false;   
    }
}
