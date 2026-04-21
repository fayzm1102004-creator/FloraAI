namespace FloraAI.API.Services;

using FloraAI.API.Data;
using FloraAI.API.DTOs.Sync;
using FloraAI.API.DTOs.Diagnosis;
using FloraAI.API.Services.Interfaces;
using AutoMapper;
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
    private readonly IMapper _mapper;

    public SyncService(
        IConditionService conditionService,
        IDiagnosisService diagnosisService,
        ApplicationDbContext dbContext,
        ILogger<SyncService> logger,
        IMapper mapper)
    {
        _conditionService = conditionService;
        _diagnosisService = diagnosisService;
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
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
                NewConditions = _mapper.Map<List<SyncConditionDto>>(conditions),
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

            var diagnosisResults = new List<SyncDiagnosisResultDto>();
            int processedCount = 0;
            int failedCount = 0;

            foreach (var scan in pendingScans)
            {
                try
                {
                    // Process scan through diagnosis service
                    var diagnosis = await _diagnosisService.ScanPlantAsync(
                        scan.PlantType,
                        scan.ConditionName);

                    diagnosisResults.Add(_mapper.Map<SyncDiagnosisResultDto>(diagnosis));
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing scan {scan.PlantType}/{scan.ConditionName}: {ex.Message}");
                    failedCount++;
                }
            }

            return new SyncPushResponseDto
            {
                DiagnosisResults = diagnosisResults,
                SyncTimestamp = DateTime.UtcNow,
                ProcessedCount = processedCount,
                FailedCount = failedCount
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
