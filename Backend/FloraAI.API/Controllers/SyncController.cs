using FloraAI.API.DTOs.Sync;
using FloraAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FloraAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;
    private readonly ILogger<SyncController> _logger;
    private readonly AutoMapper.IMapper _mapper;

    public SyncController(ISyncService syncService, ILogger<SyncController> logger, AutoMapper.IMapper mapper)
    {
        _syncService = syncService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Pull new plant conditions from server for offline synchronization.
    /// </summary>
    /// <remarks>
    /// Returns all new conditions added since the lastSyncDate.
    /// Mobile app uses this to update local SQLite database.
    /// </remarks>
    [HttpGet("pull")]
    [ProducesResponseType(typeof(SyncPullResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Pull([FromQuery] DateTime? lastSyncDate)
    {
        try
        {
            // Use DateTime.MinValue if no lastSyncDate provided (first sync)
            var syncDate = lastSyncDate ?? DateTime.MinValue;

            _logger.LogInformation($"Pull sync requested - LastSyncDate: {syncDate:O}");

            var result = await _syncService.PullConditionsAsync(syncDate);

            if (result == null)
            {
                _logger.LogWarning("Failed to retrieve conditions for sync");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Failed to retrieve sync data" });
            }

            _logger.LogInformation($"Pull sync completed - {result.NewConditions.Count()} conditions returned");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error during pull sync: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred during sync" });
        }
    }

    /// <summary>
    /// Push pending plant scans to server for processing and diagnosis.
    /// </summary>
    /// <remarks>
    /// Mobile app sends pending scans that couldn't be processed offline.
    /// Server gets treatment/care from Gemini and returns results.
    /// </remarks>
    [HttpPost("push")]
    [ProducesResponseType(typeof(SyncPushResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Push([FromBody] SyncPushRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.PendingScans == null || !request.PendingScans.Any())
                return BadRequest(new { message = "PendingScans array is required" });

            _logger.LogInformation($"Push sync requested with {request.PendingScans.Count()} pending scans");

            var result = await _syncService.PushPendingScansAsync(
                _mapper.Map<List<FloraAI.API.DTOs.Diagnosis.DiagnosisScanRequestDto>>(request.PendingScans));

            if (result == null)
            {
                _logger.LogWarning("Failed to process pending scans");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Failed to process pending scans" });
            }

            _logger.LogInformation($"Push sync completed - {result.DiagnosisResults.Count()} diagnosis results returned");
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Gemini API error during push sync: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "External AI service is temporarily unavailable. Changes will sync later." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error during push sync: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred during sync" });
        }
    }

    /// <summary>
    /// Get sync status and statistics.
    /// </summary>
    /// <remarks>
    /// Returns information about total conditions in database and last sync timestamp.
    /// Helps mobile app optimize sync strategy.
    /// </remarks>
    [HttpGet("status")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSyncStatus()
    {
        try
        {
            _logger.LogInformation("Sync status requested");

            var status = await _syncService.GetSyncStatusAsync();

            _logger.LogInformation("Sync status retrieved successfully");
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error retrieving sync status: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving sync status" });
        }
    }
}
