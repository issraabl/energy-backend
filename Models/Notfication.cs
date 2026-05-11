using EnergyTrackerr.Models;
using System.ComponentModel.DataAnnotations;

public class Notfication
{
    [Key]
    public int Id { get; set; }

    public int IdUtilisateur { get; set; }

    [Required]
    public string Message { get; set; }

    public bool IsRead { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.Now;

    public Utilisateur Utilisateur { get; set; }
}