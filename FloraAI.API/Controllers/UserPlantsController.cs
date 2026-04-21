using FloraAI.API.DTOs.UserPlant;
using FloraAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FloraAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserPlantsController : ControllerBase
{
    private readonly IUserPlantService _userPlantService;
    private readonly ILogger<UserPlantsController> _logger;

    public UserPlantsController(IUserPlantService userPlantService, ILogger<UserPlantsController> logger)
    {
        _userPlantService = userPlantService;
        _logger = logger;
    }

    /// <summary>
    /// Save a diagnosed plant to user's library.
    /// </summary>
    /// <remarks>
    /// Creates a new UserPlant entry with saved treatment and care instructions.
    /// This becomes the user's personal plant profile.
    /// </remarks>
    [HttpPost("save")]
    [ProducesResponseType(typeof(UserPlantResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SavePlant([FromBody] SaveUserPlantDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.UserId <= 0)
                return BadRequest(new { message = "Valid UserId is required" });

            if (string.IsNullOrWhiteSpace(request.PlantType))
                return BadRequest(new { message = "PlantType is required" });

            if (string.IsNullOrWhiteSpace(request.Nickname))
                return BadRequest(new { message = "Nickname is required" });

            _logger.LogInformation($"Saving plant for user {request.UserId} - Type: {request.PlantType}, Nickname: {request.Nickname}");

            var userPlant = await _userPlantService.SaveUserPlantAsync(
                request.UserId,
                new DTOs.UserPlant.SaveUserPlantDto
                {
                    Nickname = request.Nickname,
                    PlantType = request.PlantType,
                    CurrentStatus = request.CurrentStatus ?? "Healthy",
                    SavedTreatment = request.SavedTreatment,
                    SavedCareInstructions = request.SavedCareInstructions
                });

            if (userPlant == null)
            {
                _logger.LogWarning($"Failed to save plant for user {request.UserId}");
                return NotFound(new { message = "User not found" });
            }

            _logger.LogInformation($"Plant saved successfully: {userPlant.Id}");
            return CreatedAtAction(nameof(GetUserPlants), new { userId = request.UserId }, userPlant);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error while saving plant: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while saving the plant" });
        }
    }

    /// <summary>
    /// Get all plants in user's library.
    /// </summary>
    /// <remarks>
    /// Returns all UserPlant entries for the specified user with their saved data.
    /// </remarks>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<UserPlantResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserPlants(int userId)
    {
        try
        {
            if (userId <= 0)
                return BadRequest(new { message = "Valid UserId is required" });

            _logger.LogInformation($"Retrieving plants for user {userId}");

            var userPlants = await _userPlantService.GetUserPlantsAsync(userId);

            if (userPlants == null)
            {
                _logger.LogWarning($"User {userId} not found");
                return NotFound(new { message = "User not found" });
            }

            _logger.LogInformation($"Retrieved {userPlants.Count} plants for user {userId}");
            return Ok(userPlants);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error retrieving user plants: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving plants" });
        }
    }

    /// <summary>
    /// Get a specific plant from user's library.
    /// </summary>
    /// <remarks>
    /// Returns detailed information about a specific UserPlant entry.
    /// </remarks>
    [HttpGet("{plantId}")]
    [ProducesResponseType(typeof(UserPlantResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPlantById(int plantId)
    {
        try
        {
            if (plantId <= 0)
                return BadRequest(new { message = "Valid PlantId is required" });

            _logger.LogInformation($"Retrieving plant {plantId}");

            var userPlant = await _userPlantService.GetUserPlantByIdAsync(plantId);

            if (userPlant == null)
            {
                _logger.LogWarning($"Plant {plantId} not found");
                return NotFound(new { message = "Plant not found" });
            }

            var response = new UserPlantResponseDto
            {
                Id = userPlant.Id,
                UserId = userPlant.UserId,
                Nickname = userPlant.Nickname,
                PlantType = userPlant.PlantType,
                CurrentStatus = userPlant.CurrentStatus,
                SavedTreatment = userPlant.SavedTreatment,
                SavedCareInstructions = userPlant.SavedCareInstructions,
                CreatedAt = userPlant.CreatedAt
            };

            _logger.LogInformation($"Retrieved plant {plantId} successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error retrieving plant: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving the plant" });
        }
    }

    /// <summary>
    /// Update plant status.
    /// </summary>
    /// <remarks>
    /// Updates the current status of a saved plant.
    /// </remarks>
    [HttpPut("{plantId}/status")]
    [ProducesResponseType(typeof(UserPlantResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePlantStatus(int plantId, [FromBody] Dictionary<string, string> request)
    {
        try
        {
            if (plantId <= 0)
                return BadRequest(new { message = "Valid PlantId is required" });

            if (!request.ContainsKey("status") || string.IsNullOrWhiteSpace(request["status"]))
                return BadRequest(new { message = "Status is required" });

            _logger.LogInformation($"Updating plant {plantId} status to {request["status"]}");

            var updated = await _userPlantService.UpdatePlantStatusAsync(plantId, request["status"]);

            if (updated == null)
            {
                _logger.LogWarning($"Plant {plantId} not found");
                return NotFound(new { message = "Plant not found" });
            }

            var response = new UserPlantResponseDto
            {
                Id = updated.Id,
                UserId = updated.UserId,
                Nickname = updated.Nickname,
                PlantType = updated.PlantType,
                CurrentStatus = updated.CurrentStatus,
                SavedTreatment = updated.SavedTreatment,
                SavedCareInstructions = updated.SavedCareInstructions,
                CreatedAt = updated.CreatedAt
            };

            _logger.LogInformation($"Plant {plantId} status updated successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error updating plant status: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating plant status" });
        }
    }
}
