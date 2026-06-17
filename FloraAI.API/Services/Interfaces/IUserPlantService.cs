namespace FloraAI.API.Services.Interfaces;

using FloraAI.API.DTOs.UserPlant;
using FloraAI.API.DTOs.ScanHistory;
using FloraAI.API.Models.Entities;

/// <summary>
/// Service for managing user's personal plant library (UserPlant)
/// Handles creation, retrieval, and management of saved plants
/// </summary>
public interface IUserPlantService
{
    /// <summary>
    /// Creates and saves a new plant to user's library
    /// </summary>
    Task<UserPlantResponseDto> SaveUserPlantAsync(int userId, SaveUserPlantDto dto);

    /// <summary>
    /// Retrieves plants saved by a specific user with pagination
    /// </summary>
    Task<FloraAI.API.DTOs.Common.PagedResponse<UserPlantResponseDto>> GetUserPlantsAsync(int userId, int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Retrieves a specific plant by ID
    /// </summary>
    Task<UserPlantResponseDto?> GetUserPlantByIdAsync(int plantId);

    /// <summary>
    /// Updates a user plant's status and care information
    /// </summary>
    Task<UserPlantResponseDto> UpdatePlantStatusAsync(int plantId, string status);

    /// <summary>
    /// Updates the diagnosis/treatment record for a plant profile
    /// </summary>
    Task<UserPlantResponseDto?> UpdatePlantDiagnosisAsync(UpdatePlantDiagnosisDto dto);

    /// <summary>
    /// Deletes a plant from user's library
    /// </summary>
    Task<bool> DeleteUserPlantAsync(int plantId);

    /// <summary>
    /// Get scan history for a specific plant
    /// </summary>
    Task<List<ScanHistoryDto>> GetScanHistoryAsync(int userPlantId);

    /// <summary>
    /// Get all scans for a user
    /// </summary>
    Task<List<ScanHistoryDto>> GetUserScanHistoryAsync(int userId);

    /// <summary>
    /// Get latest scan for each user plant
    /// </summary>
    Task<List<ScanHistoryDto>> GetLatestScansAsync(int userId);

    /// <summary>
    /// Deletes a scan record if it belongs to the specified user.
    /// Ensures users can only delete their own scans.
    /// </summary>
    Task<bool> DeleteScanAsync(int scanId, int userId);

    /// <summary>
    /// Converts a general scan result into a permanent plant profile in user's garden.
    /// Recycles the existing scan payload (disease, treatment, care) without re-fetching.
    /// </summary>
    Task<UserPlantResponseDto?> AddScanToGardenAsync(int scanId, int userId, string nickname);
}
