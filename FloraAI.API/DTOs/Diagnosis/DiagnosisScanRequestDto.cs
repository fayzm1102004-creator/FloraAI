namespace FloraAI.API.DTOs.Diagnosis;

public class DiagnosisScanRequestDto
{
    public required string PlantType { get; set; }
    public required string ConditionName { get; set; }
    public string? DetectedCategory { get; set; }
    
    /// <summary>
    /// Optional: If provided, the backend will automatically update this specific plant's medical record with the new diagnosis
    /// </summary>
    public int? UserPlantId { get; set; }
}
