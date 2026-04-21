namespace FloraAI.API.DTOs.Sync;

public class SyncPushResponseDto
{
    public required List<SyncDiagnosisResultDto> DiagnosisResults { get; set; } = new();
    public DateTime SyncTimestamp { get; set; }
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
}

public class SyncDiagnosisResultDto
{
    public required string PlantType { get; set; }
    public required string ConditionName { get; set; }
    public string? Treatment { get; set; }
    public required string CareInstructions { get; set; }
}
