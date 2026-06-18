namespace FloraAI.API.DTOs.Sync;

public class SyncPullResponseDto
{
    public required List<SyncConditionDto> NewConditions { get; set; } = new();
    public DateTime SyncTimestamp { get; set; }
}

public class SyncConditionDto
{
    public int Id { get; set; }
    public required string PlantType { get; set; }
    public required string ConditionName { get; set; }
    public string? Treatment { get; set; }
    public string? WateringAdvice { get; set; }
    public string? LightAdvice { get; set; }
    public string? FertilizingAdvice { get; set; }
    public string? SoilAdvice { get; set; }
    public string? HumidityAdvice { get; set; }
    public DateTime LastUpdated { get; set; }
}
