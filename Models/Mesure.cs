using EnergyTrackerr.Models;

public class Mesure
{
    public int IdMesure { get; set; }
    public double Valeur { get; set; }


    public DateTime DateMesure { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.Now;

    public string SourceDonnee { get; set; }

    public int EnergieId { get; set; }
    public Energie? Energie { get; set; }

    public int? EquipementId { get; set; }
    public Equipement? Equipement { get; set; }
}