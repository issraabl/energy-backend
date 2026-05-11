namespace EnergyTrackerr.Models
{
    public abstract class Energie
    {
        public int IdEnergie { get; set; }
        public string Nom { get; set; } = "";
        public string Unite { get; set; } = "";

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public List<Mesure> Mesures { get; set; } = new List<Mesure>();
    }

    public class Electricite : Energie
    {
        public double Tension { get; set; }
        public double Puissance { get; set; }
    }

    public class Gasoil : Energie
    {
        public double Volume { get; set; }
    }

    public class Eau : Energie
    {
        public double Debit { get; set; }
    }
}