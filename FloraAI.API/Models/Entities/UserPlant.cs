namespace FloraAI.API.Models.Entities;

/// <summary>
/// المكتبة الشخصية لكل مستخدم - User's Personal Plant Library
/// Stores saved plant profiles with their care protocols
/// </summary>
public class UserPlant
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string PlantType { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public string? SavedTreatment { get; set; }
    public string? SavedWateringAdvice { get; set; }
    public string? SavedLightAdvice { get; set; }
    public string? SavedFertilizingAdvice { get; set; }
    public string? SavedSoilAdvice { get; set; }
    public string? SavedHumidityAdvice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public User? User { get; set; }

    // Navigation Properties
    public ICollection<ScanHistory> ScanHistories { get; set; } = new List<ScanHistory>();
}
