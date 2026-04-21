namespace FloraAI.API.DTOs.Diagnosis;

public class DiagnosisScanResponseDto
{
    public int ConditionId { get; set; }
    public required string PlantType { get; set; }
    public required string ConditionName { get; set; }
    public string? Treatment { get; set; }
    public required string CareInstructions { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime ScannedAt { get; set; }
    public bool IsNewlyAdded { get; set; }
}
