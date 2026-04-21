namespace FloraAI.API.Services.Interfaces;

using FloraAI.API.Models.Entities;
using FloraAI.API.DTOs.PlantLookup;

/// <summary>
/// Service for managing the ConditionsDictionary
/// Handles searching, caching, and updating plant conditions
/// </summary>
public interface IConditionService
{
    /// <summary>
    /// Searches for a plant condition in the dictionary
    /// If not found, calls Gemini API and caches the result
    /// </summary>
    Task<ConditionsDictionary> GetOrFetchConditionAsync(string plantType, string conditionName, string? detectedCategory = null);

    /// <summary>
    /// Searches for a condition without creating if not found
    /// </summary>
    Task<ConditionsDictionary?> FindConditionAsync(string plantType, string conditionName);

    /// <summary>
    /// Get all conditions (for sync operations)
    /// </summary>
    Task<List<ConditionsDictionary>> GetAllConditionsAsync();

    /// <summary>
    /// Get conditions updated after a specific date (for sync purposes)
    /// </summary>
    Task<List<ConditionsDictionary>> GetConditionsSinceAsync(DateTime lastSyncDate);

    /// <summary>
    /// Get conditions by plant type
    /// </summary>
    Task<List<ConditionsDictionary>> GetConditionsByPlantTypeAsync(string plantType);

    /// <summary>
    /// Get a specific condition by plant type and name
    /// </summary>
    Task<ConditionsDictionary?> GetConditionAsync(string plantType, string conditionName);

    /// <summary>
    /// Force refresh a condition (re-generate and overwrite cached values)
    /// </summary>
    Task<ConditionsDictionary> ForceRefreshConditionAsync(string plantType, string conditionName, string? detectedCategory = null);

    /// <summary>
    /// Get unique plants from lookup with pagination
    /// </summary>
    Task<FloraAI.API.DTOs.Common.PagedResponse<PlantLookupDto>> GetAllPlantsAsync(int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Search plants by name with pagination
    /// </summary>
    Task<FloraAI.API.DTOs.Common.PagedResponse<PlantLookupDto>> SearchPlantsAsync(string query, int pageNumber = 1, int pageSize = 10);
}
