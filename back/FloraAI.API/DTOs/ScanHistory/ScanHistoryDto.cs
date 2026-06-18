namespace FloraAI.API.DTOs.ScanHistory;

public class ScanHistoryDto
{
    public int Id { get; set; }
    public int UserPlantId { get; set; }
    public required string ConditionFound { get; set; }
    public DateTime ScanDate { get; set; }
}
