namespace FloraAI.API.DTOs.PlantLookup;

public class PlantLookupDto
{
    public int Id { get; set; }
    public required string CommonName { get; set; }
    public string? DefaultImage { get; set; }
}
