namespace FloraAI.API.DTOs.ScanHistory;

/// <summary>
/// DTO for converting a general scan result into a plant profile in user's garden.
/// Enables the "Add to My Garden" one-click conversion from scan history.
/// </summary>
public class AddScanToGardenDto
{
    /// <summary>
    /// The scan history record ID to convert
    /// </summary>
    public int ScanId { get; set; }

    /// <summary>
    /// User ID who owns the scan and will own the new plant
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Custom nickname the user gives to the plant (e.g., "مانجة الحديقة الخلفية")
    /// </summary>
    public required string Nickname { get; set; }
}
