namespace FloraAI.API.Services;

using Microsoft.EntityFrameworkCore;
using FloraAI.API.Data;
using FloraAI.API.DTOs.UserPlant;
using FloraAI.API.Models.Entities;
using FloraAI.API.Services.Interfaces;

/// <summary>
/// Implementation of UserPlantService - manages user's personal plant library
/// </summary>
public class UserPlantService : IUserPlantService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UserPlantService> _logger;

    public UserPlantService(
        ApplicationDbContext dbContext,
        ILogger<UserPlantService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Saves a new plant to user's library
    /// This happens after user approves a diagnosis result
    /// </summary>
    public async Task<UserPlantResponseDto> SaveUserPlantAsync(int userId, SaveUserPlantDto dto)
    {
        try
        {
            // Verify user exists
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var userPlant = new UserPlant
            {
                UserId = userId,
                Nickname = dto.Nickname,
                PlantType = dto.PlantType,
                CurrentStatus = dto.CurrentStatus,
                SavedTreatment = dto.SavedTreatment,
                SavedCareInstructions = dto.SavedCareInstructions,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.UserPlants.Add(userPlant);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Plant saved to library for User {userId}: {dto.Nickname}");

            return MapToResponseDto(userPlant);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving plant to library: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Retrieves all plants in user's library
    /// </summary>
    public async Task<List<UserPlantResponseDto>> GetUserPlantsAsync(int userId)
    {
        try
        {
            var userPlants = await _dbContext.UserPlants
                .Where(up => up.UserId == userId)
                .OrderByDescending(up => up.CreatedAt)
                .Include(up => up.ScanHistories)
                .ToListAsync();

            return userPlants.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving user plants: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a specific plant by ID
    /// </summary>
    public async Task<UserPlantResponseDto?> GetUserPlantByIdAsync(int plantId)
    {
        try
        {
            var userPlant = await _dbContext.UserPlants
                .Include(up => up.ScanHistories)
                .FirstOrDefaultAsync(up => up.Id == plantId);

            return userPlant != null ? MapToResponseDto(userPlant) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving user plant: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Updates a plant's status
    /// </summary>
    public async Task<UserPlantResponseDto> UpdatePlantStatusAsync(int plantId, string status)
    {
        try
        {
            var userPlant = await _dbContext.UserPlants
                .Include(up => up.ScanHistories)
                .FirstOrDefaultAsync(up => up.Id == plantId);

            if (userPlant == null)
            {
                throw new KeyNotFoundException($"UserPlant with ID {plantId} not found");
            }

            userPlant.CurrentStatus = status;

            _dbContext.UserPlants.Update(userPlant);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"UserPlant {plantId} status updated to {status}");

            return MapToResponseDto(userPlant);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating user plant status: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Deletes a plant from user's library
    /// </summary>
    public async Task<bool> DeleteUserPlantAsync(int plantId)
    {
        try
        {
            var userPlant = await _dbContext.UserPlants.FindAsync(plantId);

            if (userPlant == null)
            {
                return false;
            }

            _dbContext.UserPlants.Remove(userPlant);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"UserPlant {plantId} deleted");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting user plant: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get scan history for a specific plant
    /// </summary>
    public async Task<List<DTOs.ScanHistory.ScanHistoryDto>> GetScanHistoryAsync(int userPlantId)
    {
        try
        {
            var scans = await _dbContext.ScanHistories
                .Where(sh => sh.UserPlantId == userPlantId)
                .OrderByDescending(sh => sh.ScanDate)
                .ToListAsync();

            return scans.Select(s => new DTOs.ScanHistory.ScanHistoryDto
            {
                Id = s.Id,
                UserPlantId = s.UserPlantId,
                ConditionFound = s.ConditionFound,
                ScanDate = s.ScanDate
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving scan history: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get all scans for a user
    /// </summary>
    public async Task<List<DTOs.ScanHistory.ScanHistoryDto>> GetUserScanHistoryAsync(int userId)
    {
        try
        {
            var scans = await _dbContext.ScanHistories
                .Include(sh => sh.UserPlant)
                .Where(sh => sh.UserPlant.UserId == userId)
                .OrderByDescending(sh => sh.ScanDate)
                .ToListAsync();

            return scans.Select(s => new DTOs.ScanHistory.ScanHistoryDto
            {
                Id = s.Id,
                UserPlantId = s.UserPlantId,
                ConditionFound = s.ConditionFound,
                ScanDate = s.ScanDate
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving user scan history: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get latest scan for each user plant
    /// </summary>
    public async Task<List<DTOs.ScanHistory.ScanHistoryDto>> GetLatestScansAsync(int userId)
    {
        try
        {
            var userPlants = await _dbContext.UserPlants
                .Where(up => up.UserId == userId)
                .Select(up => up.Id)
                .ToListAsync();

            var latestScans = new List<DTOs.ScanHistory.ScanHistoryDto>();

            foreach (var plantId in userPlants)
            {
                var latestScan = await _dbContext.ScanHistories
                    .Where(sh => sh.UserPlantId == plantId)
                    .OrderByDescending(sh => sh.ScanDate)
                    .FirstOrDefaultAsync();

                if (latestScan != null)
                {
                    latestScans.Add(new DTOs.ScanHistory.ScanHistoryDto
                    {
                        Id = latestScan.Id,
                        UserPlantId = latestScan.UserPlantId,
                        ConditionFound = latestScan.ConditionFound,
                        ScanDate = latestScan.ScanDate
                    });
                }
            }

            return latestScans;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving latest scans: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Maps UserPlant entity to response DTO
    /// </summary>
    private UserPlantResponseDto MapToResponseDto(UserPlant userPlant)
    {
        return new UserPlantResponseDto
        {
            Id = userPlant.Id,
            UserId = userPlant.UserId,
            Nickname = userPlant.Nickname,
            PlantType = userPlant.PlantType,
            CurrentStatus = userPlant.CurrentStatus,
            SavedTreatment = userPlant.SavedTreatment,
            SavedCareInstructions = userPlant.SavedCareInstructions ?? string.Empty,
            CreatedAt = userPlant.CreatedAt,
            ScanCount = userPlant.ScanHistories?.Count ?? 0
        };
    }
}
