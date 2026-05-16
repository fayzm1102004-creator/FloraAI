namespace FloraAI.API.Models.Entities;

/// <summary>
/// المخزن الذكي العام للتعلم - Central Dictionary for Plant Conditions
/// Stores discovered plant diseases and their treatments
/// </summary>
public class ConditionsDictionary
{
    public int Id { get; set; }
    public string PlantType { get; set; } = string.Empty;
    public string ConditionName { get; set; } = string.Empty;
    public string? Treatment { get; set; }
    public string? WateringAdvice { get; set; }
    public string? LightAdvice { get; set; }
    public string? FertilizingAdvice { get; set; }
    public string? SoilAdvice { get; set; }
    public string? HumidityAdvice { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<ScanHistory> ScanHistories { get; set; } = new List<ScanHistory>();
}
