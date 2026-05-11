namespace EnergyTrackerr.Models;
using System.Text.Json.Serialization;

public class Site
{
    [JsonPropertyName("idSite")]
    public int IdSite { get; set; }

    [JsonPropertyName("nom")]
    public string Name { get; set; } = "";

    [JsonPropertyName("adresse")]
    public string? Address { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.Now;
    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
}