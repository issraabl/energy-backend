public class MesureCreateDto
{
    public double Valeur { get; set; }
    public DateTime DateMesure { get; set; }
    public string SourceDonnee { get; set; } = string.Empty;
    public string? Commentaire { get; set; }  // ✅ ajouté
    public string EnergieNom { get; set; } = string.Empty;
    public int? EquipementId { get; set; }
}

public class MesureReadDto
{
    public int IdMesure { get; set; }
    public double Valeur { get; set; }
    public DateTime DateMesure { get; set; }
    public DateTime DateCreation { get; set; }  // ✅ ajouté
    public string SourceDonnee { get; set; } = string.Empty;
    public string? Commentaire { get; set; }    // ✅ ajouté
    public int EnergieId { get; set; }
    public int? EquipementId { get; set; }      // ✅ ajouté
    public EnergieDto? Energie { get; set; }
    public EquipementDto? Equipement { get; set; }
}

public class EnergieDto
{
    public int IdEnergie { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Unite { get; set; } = string.Empty;
}

public class EquipementDto
{
    public int IdEquipement { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string TypeEquipement { get; set; } = string.Empty;
}