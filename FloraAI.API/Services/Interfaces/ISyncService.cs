namespace FloraAI.API.Services.Interfaces;

using FloraAI.API.DTOs.Sync;
using FloraAI.API.DTOs.Diagnosis;

/// <summary>
/// Service for handling offline-first sync operations
/// Manages Pull (ConditionsDictionary updates) and Push (scan history)
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Pulls all conditions updated since lastSyncDate
    /// Used by mobile to update local SQLite cache
    /// </summary>
    Task<SyncPullResponseDto> PullConditionsAsync(DateTime lastSyncDate);

    /// <summary>
    /// Pushes pending diagnosis scans from mobile
    /// Server processes them and returns results
    /// </summary>
    Task<SyncPushResponseDto> PushPendingScansAsync(List<DiagnosisScanRequestDto> pendingScans);

    /// <summary>
    /// Get sync statistics
    /// </summary>
    Task<object> GetSyncStatusAsync();
}
