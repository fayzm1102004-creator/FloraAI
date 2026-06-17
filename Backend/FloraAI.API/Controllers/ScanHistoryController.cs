using FloraAI.API.DTOs.ScanHistory;
using FloraAI.API.DTOs.UserPlant;
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

            _logger.LogInformation($"Retrieved {scanHistory.Count} scan records for plant {userPlantId}");
            return Ok(scanHistory);
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

            _logger.LogInformation($"Retrieved {allScans.Count} total scans for user {userId}");
            return Ok(allScans);
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

    /// <summary>
    /// Delete a scan record from history.
    /// </summary>
    /// <remarks>
    /// Removes a specific scan from the user's history.
    /// Security: Users can only delete their own scans - ownership is validated server-side.
    /// </remarks>
    [HttpDelete("{scanId}/user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteScan(int scanId, int userId)
    {
        try
        {
            if (scanId <= 0)
                return BadRequest(new { message = "Valid ScanId is required" });

            if (userId <= 0)
                return BadRequest(new { message = "Valid UserId is required" });

            _logger.LogInformation($"User {userId} requesting deletion of scan {scanId}");

            var deleted = await _userPlantService.DeleteScanAsync(scanId, userId);

            if (!deleted)
            {
                _logger.LogWarning($"Scan {scanId} not found or doesn't belong to user {userId}");
                return NotFound(new { message = "الفحص غير موجود أو لا ينتمي لحسابك" });
            }

            _logger.LogInformation($"Scan {scanId} deleted successfully by user {userId}");
            return Ok(new { success = true, message = "تم حذف الفحص بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error deleting scan {scanId}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء حذف الفحص" });
        }
    }

    /// <summary>
    /// Convert a general scan result into a permanent plant in user's garden.
    /// </summary>
    /// <remarks>
    /// One-Click Conversion: Takes an existing scan result and creates a full plant profile
    /// in the user's garden without re-scanning or re-fetching from AI.
    /// The scan's existing payload (disease, treatment, care plan) is recycled perfectly.
    /// Security: Users can only convert their own scans.
    /// </remarks>
    [HttpPost("add-to-garden")]
    [ProducesResponseType(typeof(UserPlantResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddScanToGarden([FromBody] AddScanToGardenDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.ScanId <= 0)
                return BadRequest(new { message = "Valid ScanId is required" });

            if (request.UserId <= 0)
                return BadRequest(new { message = "Valid UserId is required" });

            if (string.IsNullOrWhiteSpace(request.Nickname))
                return BadRequest(new { message = "اسم النبتة مطلوب" });

            _logger.LogInformation($"User {request.UserId} converting scan {request.ScanId} to garden plant '{request.Nickname}'");

            var newPlant = await _userPlantService.AddScanToGardenAsync(
                request.ScanId, request.UserId, request.Nickname);

            if (newPlant == null)
            {
                _logger.LogWarning($"Failed to convert scan {request.ScanId} for user {request.UserId}");
                return NotFound(new { message = "الفحص غير موجود أو لا ينتمي لحسابك" });
            }

            _logger.LogInformation($"Scan {request.ScanId} successfully converted to plant '{request.Nickname}' (Id: {newPlant.Id})");
            return StatusCode(StatusCodes.Status201Created, new
            {
                success = true,
                message = "تمت إضافة النبتة لحديقتك بنجاح 🌿",
                plant = newPlant
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error converting scan to garden plant: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء إضافة النبتة للحديقة" });
        }
    }
}
