namespace FloraAI.API.Models.Entities;

/// <summary>
/// فهرس عام للأسماء - Plant Lookup Index
/// Global index for common plant names and their default images
/// </summary>
public class PlantLookup
{
    public int Id { get; set; }
    public string CommonName { get; set; } = string.Empty;
    public string? DefaultImage { get; set; }
}
