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
                SavedCareInstructions = dto.SavedCareInstructions,
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

    public async Task<List<UserPlantResponseDto>> GetUserPlantsAsync(int userId)
    {
        try
        {
            var userPlants = await _dbContext.UserPlants
                .Where(up => up.UserId == userId)
                .OrderByDescending(up => up.CreatedAt)
                .Include(up => up.ScanHistories)
                .ToListAsync();

            return _mapper.Map<List<UserPlantResponseDto>>(userPlants);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving user plants: {ex.Message}");
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
}
