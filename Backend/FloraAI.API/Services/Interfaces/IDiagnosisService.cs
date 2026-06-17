namespace FloraAI.API.Services.Interfaces;

using FloraAI.API.DTOs.Diagnosis;
using FloraAI.API.Models.Entities;

/// <summary>
/// Service for handling plant diagnosis workflow
/// Coordinates between Gemini API and database
/// </summary>
public interface IDiagnosisService
{
    /// <summary>
    /// Performs a plant diagnosis scan
    /// Returns treatment and care instructions from cache or Gemini API
    /// </summary>
    Task<DiagnosisScanResponseDto> ScanPlantAsync(string plantType, string conditionName);

    /// <summary>
    /// Records a scan in the history
    /// </summary>
    Task<ScanHistory> RecordScanAsync(int userPlantId, string conditionFound);
}
