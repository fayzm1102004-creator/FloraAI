namespace FloraAI.API.Services;

using FloraAI.API.Data;
using FloraAI.API.DTOs.Sync;
using FloraAI.API.DTOs.Diagnosis;
using FloraAI.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Implementation of SyncService - handles offline-first sync operations
/// Manages Pull (condition updates) and Push (pending scans)
/// </summary>
public class SyncService : ISyncService
{
    private readonly IConditionService _conditionService;
    private readonly IDiagnosisService _diagnosisService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SyncService> _logger;

    public SyncService(
        IConditionService conditionService,
        IDiagnosisService diagnosisService,
        ApplicationDbContext dbContext,
        ILogger<SyncService> logger)
    {
        _conditionService = conditionService;
        _diagnosisService = diagnosisService;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Pulls all conditions updated since lastSyncDate for mobile SQLite update
    /// </summary>
    public async Task<SyncPullResponseDto> PullConditionsAsync(DateTime lastSyncDate)
    {
        try
        {
            _logger.LogInformation($"Pull sync requested since: {lastSyncDate:O}");

            // Get all conditions updated since lastSyncDate
            var conditions = await _conditionService.GetConditionsSinceAsync(lastSyncDate);

            _logger.LogInformation($"Returning {conditions.Count} updated conditions");

            return new SyncPullResponseDto
            {
                NewConditions = conditions
                    .Select(c => new DTOs.Sync.SyncConditionDto
                    {
                        Id = c.Id,
                        PlantType = c.PlantType,
                        ConditionName = c.ConditionName,
                        Treatment = c.Treatment,
                        CareInstructions = c.CareInstructions,
                        LastUpdated = c.LastUpdated
                    })
                    .ToList(),
                SyncTimestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during pull sync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Pushes pending diagnosis scans from mobile
    /// Server processes them through Gemini API if needed and returns results
    /// </summary>
    public async Task<SyncPushResponseDto> PushPendingScansAsync(List<DiagnosisScanRequestDto> pendingScans)
    {
        try
        {
            _logger.LogInformation($"Push sync received with {pendingScans.Count} scans");

            var results = new List<object>();

            foreach (var scan in pendingScans)
            {
                try
                {
                    // Process scan through diagnosis service
                    var diagnosis = await _diagnosisService.ScanPlantAsync(
                        scan.PlantType,
                        scan.ConditionName);

                    results.Add(new
                    {
                        scan.PlantType,
                        scan.ConditionName,
                        diagnosis.Treatment,
                        diagnosis.CareInstructions,
                        diagnosis.ScannedAt,
                        Success = true
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing scan {scan.PlantType}/{scan.ConditionName}: {ex.Message}");
                    results.Add(new
                    {
                        scan.PlantType,
                        scan.ConditionName,
                        Error = ex.Message,
                        Success = false
                    });
                }
            }

            var diagnosisResults = results
                .Where(r => (bool)((dynamic)r).Success)
                .Select(r => new DTOs.Sync.SyncDiagnosisResultDto
                {
                    PlantType = (string)((dynamic)r).PlantType,
                    ConditionName = (string)((dynamic)r).ConditionName,
                    Treatment = (string)((dynamic)r).Treatment,
                    CareInstructions = (string)((dynamic)r).CareInstructions
                })
                .ToList();

            return new SyncPushResponseDto
            {
                DiagnosisResults = diagnosisResults,
                SyncTimestamp = DateTime.UtcNow,
                ProcessedCount = results.Count(r => (bool)((dynamic)r).Success),
                FailedCount = results.Count(r => !(bool)((dynamic)r).Success)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during push sync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get sync statistics
    /// </summary>
    public async Task<object> GetSyncStatusAsync()
    {
        try
        {
            var totalConditions = await _dbContext.ConditionsDictionary.CountAsync();
            var cachedConditions = await _dbContext.ConditionsDictionary
                .Where(c => !string.IsNullOrEmpty(c.Treatment))
                .CountAsync();

            return new
            {
                TotalConditionsInDatabase = totalConditions,
                CachedConditions = cachedConditions,
                UncachedConditions = totalConditions - cachedConditions,
                LastSyncTime = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting sync status: {ex.Message}");
            throw;
        }
    }
}
