namespace FloraAI.API.Services;

using Microsoft.EntityFrameworkCore;
using FloraAI.API.Data;
using FloraAI.API.DTOs.UserPlant;
using FloraAI.API.DTOs.ScanHistory;
using FloraAI.API.Models.Entities;
using FloraAI.API.Services.Interfaces;
using AutoMapper;

/// <summary>
/// Implementation of UserPlantService - manages user's personal plant library
/// </summary>
public class UserPlantService : IUserPlantService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UserPlantService> _logger;
    private readonly IMapper _mapper;

    public UserPlantService(
        ApplicationDbContext dbContext,
        ILogger<UserPlantService> logger,
        IMapper mapper)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<UserPlantResponseDto> SaveUserPlantAsync(int userId, SaveUserPlantDto dto)
    {
        try
        {
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
                SavedWateringAdvice = dto.CareAdvice.Watering,
                SavedLightAdvice = dto.CareAdvice.Light,
                SavedFertilizingAdvice = dto.CareAdvice.Fertilizing,
                SavedSoilAdvice = dto.CareAdvice.Soil,
                SavedHumidityAdvice = dto.CareAdvice.Humidity,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.UserPlants.Add(userPlant);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Plant saved to library for User {userId}: {dto.Nickname}");

            return _mapper.Map<UserPlantResponseDto>(userPlant);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving plant to library: {ex.Message}");
            throw;
        }
    }

    public async Task<FloraAI.API.DTOs.Common.PagedResponse<UserPlantResponseDto>> GetUserPlantsAsync(int userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _dbContext.UserPlants
                .AsNoTracking()
                .Where(up => up.UserId == userId);

            var totalRecords = await query.CountAsync();
            
            var userPlants = await query
                .OrderByDescending(up => up.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(up => up.ScanHistories)
                .ToListAsync();

            var dtos = _mapper.Map<List<UserPlantResponseDto>>(userPlants);
            return new FloraAI.API.DTOs.Common.PagedResponse<UserPlantResponseDto>(dtos, pageNumber, pageSize, totalRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving user plants with pagination: {ex.Message}");
            throw;
        }
    }

    public async Task<UserPlantResponseDto?> GetUserPlantByIdAsync(int plantId)
    {
        try
        {
            var userPlant = await _dbContext.UserPlants
                .Include(up => up.ScanHistories)
                .FirstOrDefaultAsync(up => up.Id == plantId);

            return _mapper.Map<UserPlantResponseDto?>(userPlant);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving user plant: {ex.Message}");
            throw;
        }
    }

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

            return _mapper.Map<UserPlantResponseDto>(userPlant);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating user plant status: {ex.Message}");
            throw;
        }
    }

    public async Task<UserPlantResponseDto?> UpdatePlantDiagnosisAsync(UpdatePlantDiagnosisDto dto)
    {
        try
        {
            var userPlant = await _dbContext.UserPlants
                .Include(up => up.ScanHistories)
                .FirstOrDefaultAsync(up => up.Id == dto.PlantId);

            if (userPlant == null)
            {
                _logger.LogWarning($"UserPlant with ID {dto.PlantId} not found for diagnosis update");
                return null;
            }

            userPlant.SavedTreatment = dto.Treatment;
            userPlant.SavedWateringAdvice = dto.CareAdvice.Watering;
            userPlant.SavedLightAdvice = dto.CareAdvice.Light;
            userPlant.SavedFertilizingAdvice = dto.CareAdvice.Fertilizing;
            userPlant.SavedSoilAdvice = dto.CareAdvice.Soil;
            userPlant.SavedHumidityAdvice = dto.CareAdvice.Humidity;

            _dbContext.UserPlants.Update(userPlant);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Diagnosis record updated for UserPlant {dto.PlantId}");

            return _mapper.Map<UserPlantResponseDto>(userPlant);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating user plant diagnosis: {ex.Message}");
            throw;
        }
    }

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

    public async Task<List<ScanHistoryDto>> GetScanHistoryAsync(int userPlantId)
    {
        try
        {
            var scans = await _dbContext.ScanHistories
                .Where(sh => sh.UserPlantId == userPlantId)
                .OrderByDescending(sh => sh.ScanDate)
                .ToListAsync();

            return _mapper.Map<List<ScanHistoryDto>>(scans);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving scan history: {ex.Message}");
            throw;
        }
    }

    public async Task<List<ScanHistoryDto>> GetUserScanHistoryAsync(int userId)
    {
        try
        {
            var scans = await _dbContext.ScanHistories
                .Include(sh => sh.UserPlant)
                .Where(sh => sh.UserPlant.UserId == userId)
                .OrderByDescending(sh => sh.ScanDate)
                .ToListAsync();

            return _mapper.Map<List<ScanHistoryDto>>(scans);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving user scan history: {ex.Message}");
            throw;
        }
    }

    public async Task<List<ScanHistoryDto>> GetLatestScansAsync(int userId)
    {
        try
        {
            var userPlantIds = await _dbContext.UserPlants
                .Where(up => up.UserId == userId)
                .Select(up => up.Id)
                .ToListAsync();

            var latestScans = await _dbContext.ScanHistories
                .Where(sh => userPlantIds.Contains(sh.UserPlantId))
                .GroupBy(sh => sh.UserPlantId)
                .Select(g => g.OrderByDescending(sh => sh.ScanDate).First())
                .ToListAsync();

            return _mapper.Map<List<ScanHistoryDto>>(latestScans);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving latest scans: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Deletes a scan record only if it belongs to the specified user.
    /// كل يوزر بيتحكم في ملفه بس - مستحيل يمسح فحص حد تاني
    /// </summary>
    public async Task<bool> DeleteScanAsync(int scanId, int userId)
    {
        try
        {
            var scan = await _dbContext.ScanHistories
                .Include(sh => sh.UserPlant)
                .FirstOrDefaultAsync(sh => sh.Id == scanId);

            if (scan == null)
            {
                _logger.LogWarning($"Scan {scanId} not found");
                return false;
            }

            // Security: Verify the scan belongs to this user
            if (scan.UserPlant == null || scan.UserPlant.UserId != userId)
            {
                _logger.LogWarning($"User {userId} attempted to delete scan {scanId} that doesn't belong to them");
                return false;
            }

            _dbContext.ScanHistories.Remove(scan);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Scan {scanId} deleted successfully by user {userId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting scan {scanId}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Converts a general scan result into a permanent plant profile.
    /// One-Click Conversion: Recycles the existing payload without re-fetching from Gemini.
    /// تحويل فحص عابر لبروفايل نبتة دائم في حديقة اليوزر بضغطة واحدة
    /// </summary>
    public async Task<UserPlantResponseDto?> AddScanToGardenAsync(int scanId, int userId, string nickname)
    {
        try
        {
            // 1. Find the scan record with its linked condition data
            var scan = await _dbContext.ScanHistories
                .Include(sh => sh.ConditionsDictionary)
                .Include(sh => sh.UserPlant)
                .FirstOrDefaultAsync(sh => sh.Id == scanId);

            if (scan == null)
            {
                _logger.LogWarning($"Scan {scanId} not found for garden conversion");
                return null;
            }

            // Security: Verify the scan belongs to this user
            if (scan.UserPlant == null || scan.UserPlant.UserId != userId)
            {
                _logger.LogWarning($"User {userId} attempted to convert scan {scanId} that doesn't belong to them");
                return null;
            }

            // 2. Recycle the existing payload from ConditionsDictionary (no re-fetch needed!)
            var condition = scan.ConditionsDictionary;
            if (condition == null)
            {
                _logger.LogWarning($"Scan {scanId} has no linked condition data");
                return null;
            }

            // 3. Create a new permanent plant profile with all the recycled data
            var newPlant = new UserPlant
            {
                UserId = userId,
                Nickname = nickname,
                PlantType = condition.PlantType,
                CurrentStatus = condition.ConditionName,
                SavedTreatment = condition.Treatment,
                SavedWateringAdvice = condition.WateringAdvice,
                SavedLightAdvice = condition.LightAdvice,
                SavedFertilizingAdvice = condition.FertilizingAdvice,
                SavedSoilAdvice = condition.SoilAdvice,
                SavedHumidityAdvice = condition.HumidityAdvice,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.UserPlants.Add(newPlant);
            await _dbContext.SaveChangesAsync();

            // 4. Link the original scan to the new plant profile
            scan.UserPlantId = newPlant.Id;
            _dbContext.ScanHistories.Update(scan);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Scan {scanId} converted to garden plant '{nickname}' (PlantId: {newPlant.Id}) for user {userId}");

            return _mapper.Map<UserPlantResponseDto>(newPlant);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error converting scan {scanId} to garden plant: {ex.Message}");
            throw;
        }
    }
}
