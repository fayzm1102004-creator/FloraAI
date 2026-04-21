namespace FloraAI.API.DTOs.Diagnosis;

public class DiagnosisScanRequestDto
{
    public required string PlantType { get; set; }
    public required string ConditionName { get; set; }
    public string? DetectedCategory { get; set; }
}
