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
    public required string CareInstructions { get; set; }
    public DateTime LastUpdated { get; set; }
}
