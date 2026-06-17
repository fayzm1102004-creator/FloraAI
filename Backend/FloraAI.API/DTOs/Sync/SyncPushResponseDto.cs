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
    public string? WateringAdvice { get; set; }
    public string? LightAdvice { get; set; }
    public string? FertilizingAdvice { get; set; }
    public string? SoilAdvice { get; set; }
    public string? HumidityAdvice { get; set; }
}
