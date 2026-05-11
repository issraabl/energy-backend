namespace EnergyTrackerr.Models
{
    public class Site
    {
        public int IdSite { get; set; }
        public string Name { get; set; } = "";
        public string? Address { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public ICollection<Zone> Zones { get; set; } = new List<Zone>();
    }
}