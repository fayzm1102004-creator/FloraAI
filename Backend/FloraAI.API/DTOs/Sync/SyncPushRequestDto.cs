namespace FloraAI.API.DTOs.Sync;

public class SyncPushRequestDto
{
    public required List<PendingScanDto> PendingScans { get; set; }
}

public class PendingScanDto
{
    public required string PlantType { get; set; }
    public required string ConditionName { get; set; }
}
