using FloraAI.API.DTOs.ScanHistory;
using FloraAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FloraAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScanHistoryController : ControllerBase
{
    private readonly IUserPlantService _userPlantService;
    private readonly ILogger<ScanHistoryController> _logger;

    public ScanHistoryController(IUserPlantService userPlantService, ILogger<ScanHistoryController> logger)
    {
        _userPlantService = userPlantService;
        _logger = logger;
    }

    /// <summary>
    /// Get scan history for a specific user plant.
    /// </summary>
    /// <remarks>
    /// Returns all diagnosis scans performed on a specific plant.
    /// Useful for tracking plant health over time.
    /// </remarks>
    [HttpGet("plant/{userPlantId}")]
    [ProducesResponseType(typeof(IEnumerable<ScanHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetScanHistoryByPlant(int userPlantId)
    {
        try
        {
            if (userPlantId <= 0)
                return BadRequest(new { message = "Valid UserPlantId is required" });

            _logger.LogInformation($"Retrieving scan history for plant {userPlantId}");

            var scanHistory = await _userPlantService.GetScanHistoryAsync(userPlantId);

            if (scanHistory == null)
            {
                _logger.LogWarning($"Plant {userPlantId} not found");
                return NotFound(new { message = "Plant not found" });
            }

            var response = scanHistory.Select(s => new ScanHistoryDto
            {
                Id = s.Id,
                UserPlantId = s.UserPlantId,
                ConditionFound = s.ConditionFound,
                ScanDate = s.ScanDate
            }).OrderByDescending(s => s.ScanDate).ToList();

            _logger.LogInformation($"Retrieved {response.Count} scan records for plant {userPlantId}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error retrieving scan history: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving scan history" });
        }
    }

    /// <summary>
    /// Get scan history for all plants of a user.
    /// </summary>
    /// <remarks>
    /// Returns all diagnosis scans performed by a specific user across all plants.
    /// </remarks>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<ScanHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetScanHistoryByUser(int userId)
    {
        try
        {
            if (userId <= 0)
                return BadRequest(new { message = "Valid UserId is required" });

            _logger.LogInformation($"Retrieving scan history for user {userId}");

            var allScans = await _userPlantService.GetUserScanHistoryAsync(userId);

            if (allScans == null)
            {
                _logger.LogWarning($"User {userId} not found or has no plants");
                return NotFound(new { message = "User not found or has no scan history" });
            }

            var response = allScans.Select(s => new ScanHistoryDto
            {
                Id = s.Id,
                UserPlantId = s.UserPlantId,
                ConditionFound = s.ConditionFound,
                ScanDate = s.ScanDate
            }).OrderByDescending(s => s.ScanDate).ToList();

            _logger.LogInformation($"Retrieved {response.Count} total scans for user {userId}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error retrieving user scan history: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving scan history" });
        }
    }

    /// <summary>
    /// Get latest scan for each plant of a user.
    /// </summary>
    /// <remarks>
    /// Returns the most recent scan result for each of the user's plants.
    /// Useful for dashboard view.
    /// </remarks>
    [HttpGet("user/{userId}/latest")]
    [ProducesResponseType(typeof(Dictionary<int, ScanHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLatestScans(int userId)
    {
        try
        {
            if (userId <= 0)
                return BadRequest(new { message = "Valid UserId is required" });

            _logger.LogInformation($"Retrieving latest scans for user {userId}");

            var latestScans = await _userPlantService.GetLatestScansAsync(userId);

            if (latestScans == null || !latestScans.Any())
            {
                _logger.LogInformation($"No scan history found for user {userId}");
                return Ok(new Dictionary<int, ScanHistoryDto>());
            }

            var response = latestScans.Select(s => new { PlantId = s.UserPlantId, Scan = s }).ToList();

            _logger.LogInformation($"Retrieved {response.Count} latest scans for user {userId}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error retrieving latest scans: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving latest scans" });
        }
    }
}
