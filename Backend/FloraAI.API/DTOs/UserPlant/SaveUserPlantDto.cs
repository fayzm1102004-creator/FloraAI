namespace FloraAI.API.DTOs.UserPlant;

public class SaveUserPlantDto
{
    public int UserId { get; set; }
    public required string Nickname { get; set; }
    public required string PlantType { get; set; }
    public required string CurrentStatus { get; set; }
    public string? SavedTreatment { get; set; }
    public FloraAI.API.DTOs.Diagnosis.CareAdviceDto CareAdvice { get; set; } = new();
}
