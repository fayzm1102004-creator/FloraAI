namespace FloraAI.API.DTOs.UserPlant;

public class UserPlantResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Nickname { get; set; }
    public required string PlantType { get; set; }
    public required string CurrentStatus { get; set; }
    public string? SavedTreatment { get; set; }
    public string? SavedCareInstructions { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ScanCount { get; set; }
}
