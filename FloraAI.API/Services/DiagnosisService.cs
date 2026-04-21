namespace FloraAI.API.Services;

using Microsoft.EntityFrameworkCore;
using FloraAI.API.Data;
using FloraAI.API.DTOs.Diagnosis;
using FloraAI.API.Models.Entities;
using FloraAI.API.Services.Interfaces;

/// <summary>
/// Implementation of DiagnosisService - orchestrates the diagnosis workflow
/// </summary>
public class DiagnosisService : IDiagnosisService
{
    private readonly IConditionService _conditionService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DiagnosisService> _logger;

    public DiagnosisService(
        IConditionService conditionService,
        ApplicationDbContext dbContext,
        ILogger<DiagnosisService> logger)
    {
        _conditionService = conditionService;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Performs a plant diagnosis scan
    /// Returns treatment and care from cache or Gemini API
    /// </summary>
    public async Task<DiagnosisScanResponseDto> ScanPlantAsync(string plantType, string conditionName)
    {
        _logger.LogInformation($"Scanning plant: {plantType}/{conditionName}");

        // Get or create condition (with Gemini fallback)
        var condition = await _conditionService.GetOrFetchConditionAsync(plantType, conditionName);

        return new DiagnosisScanResponseDto
        {
            PlantType = condition.PlantType,
            ConditionName = condition.ConditionName,
            Treatment = condition.Treatment ?? "لا توجد معالجة متاحة",
            CareInstructions = condition.CareInstructions ?? "لا توجد تعليمات رعاية متاحة",
            ScannedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Records a scan in the ScanHistory
    /// </summary>
    public async Task<ScanHistory> RecordScanAsync(int userPlantId, string conditionFound)
    {
        try
        {
            // Verify user plant exists
            var userPlant = await _dbContext.UserPlants.FindAsync(userPlantId);
            if (userPlant == null)
            {
                throw new KeyNotFoundException($"نبات المستخدم برقم {userPlantId} غير موجود");
            }

            var scanHistory = new ScanHistory
            {
                UserPlantId = userPlantId,
                ConditionFound = conditionFound,
                ScanDate = DateTime.UtcNow
            };

            _dbContext.ScanHistories.Add(scanHistory);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Scan recorded for UserPlant {userPlantId}: {conditionFound}");

            return scanHistory;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error recording scan: {ex.Message}");
            throw;
        }
    }
}
