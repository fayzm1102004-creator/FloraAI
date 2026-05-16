namespace FloraAI.API.DTOs.Diagnosis;

public class DiagnosisScanResponseDto
{
    public int ConditionId { get; set; }
    public required string PlantType { get; set; }
    public required string ConditionName { get; set; }
    public string? Treatment { get; set; }
    public CareAdviceDto CareAdvice { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public DateTime ScannedAt { get; set; }
    public bool IsNewlyAdded { get; set; }
}
