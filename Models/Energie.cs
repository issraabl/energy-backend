namespace EnergyTrackerr.Models
{
    // ─── Classe de base (NON abstraite pour permettre l'instanciation directe) ───
    public class Energie
    {
        public int IdEnergie { get; set; }
        public string Nom { get; set; } = "";
        public string Unite { get; set; } = "";
        public string? Description { get; set; }
        public double? FacteurConversion { get; set; } = 1;
        public string? Couleur { get; set; } = "#6366f1";
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Navigation
        public List<Mesure> Mesures { get; set; } = new List<Mesure>();
    }

    // ─── Sous-classes existantes (inchangées) ────────────────────────────────────
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

    // ─── NOUVELLE sous-classe pour les énergies personnalisées ───────────────────
    // Permet de créer n'importe quel type d'énergie (Solaire, Biomasse, Vapeur…)
    // sans modifier le modèle à chaque fois.
    public class EnergieGenerique : Energie
    {
        // Aucun champ supplémentaire requis.
        // Le type est stocké dans la colonne discriminante "Discriminator"
        // de la table Energies (EF Core TPH).
    }
}