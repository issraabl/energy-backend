using EnergyTrackerr.Models;

public class Zone
{
    public int IdZone { get; set; }
    public string Nom { get; set; } = "";
    public string? Description { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.Now;

    public int SiteId { get; set; }
    public Site? Site { get; set; }
}