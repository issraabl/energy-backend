using System.ComponentModel.DataAnnotations;

namespace EnergyTrackerr.Models
{
    public class Utilisateur
    {
        [Key]
        public int IdUtilisateur { get; set; }

        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // ✅ Correction : était "Roles.Employe" (string littérale) au lieu de la constante
        public string Role { get; set; } = Roles.Employe;

        public string? Telephone { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public ICollection<Notfication> Notifications { get; set; }
            = new List<Notfication>();
    }
}