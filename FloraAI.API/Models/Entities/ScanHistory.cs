namespace FloraAI.API.Models.Entities;

/// <summary>
/// سجل الفحوصات لكل نبتة - Scan History Log
/// Tracks all diagnosis scans performed on each plant
/// </summary>
public class ScanHistory
{
    public int Id { get; set; }
    public int UserPlantId { get; set; }
    public int ConditionsDictionaryId { get; set; }
    public string ConditionFound { get; set; } = string.Empty;
    public DateTime ScanDate { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public UserPlant? UserPlant { get; set; }
    public ConditionsDictionary? ConditionsDictionary { get; set; }
}
