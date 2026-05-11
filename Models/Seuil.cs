namespace EnergyTrackerr.Models
{
    public class Seuil
    {
        public int IdSeuil { get; set; }
        public int EnergieId { get; set; }
        public double Valeur { get; set; }       
        public string Periode { get; set; }       
        public Energie? Energie { get; set; }
    }
}
