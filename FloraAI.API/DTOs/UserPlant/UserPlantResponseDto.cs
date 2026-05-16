namespace FloraAI.API.DTOs.UserPlant;

public class UserPlantResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Nickname { get; set; }
    public required string PlantType { get; set; }
    public required string CurrentStatus { get; set; }
    public string? SavedTreatment { get; set; }
    public string? SavedWateringAdvice { get; set; }
    public string? SavedLightAdvice { get; set; }
    public string? SavedFertilizingAdvice { get; set; }
    public string? SavedSoilAdvice { get; set; }
    public string? SavedHumidityAdvice { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ScanCount { get; set; }
}
