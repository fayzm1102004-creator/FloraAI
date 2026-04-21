using FloraAI.API.DTOs.PlantLookup;
using FloraAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FloraAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlantLookupController : ControllerBase
{
    private readonly IConditionService _conditionService;
    private readonly ILogger<PlantLookupController> _logger;

    public PlantLookupController(IConditionService conditionService, ILogger<PlantLookupController> logger)
    {
        _conditionService = conditionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available plants in the system.
    /// </summary>
    /// <remarks>
    /// Returns a list of all plant types with their common names.
    /// Useful for mobile app UI plant selection lists.
    /// </remarks>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<PlantLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllPlants()
    {
        try
        {
            _logger.LogInformation("Retrieving all plants");

            var plants = await _conditionService.GetAllPlantsAsync();

            if (plants == null || plants.Count == 0)
                plants = new List<PlantLookupDto>();

            _logger.LogInformation($"Retrieved {plants.Count} unique plants");
            return Ok(plants);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error retrieving plants: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving plants" });
        }
    }

    /// <summary>
    /// Search for a specific plant by common name.
    /// </summary>
    /// <remarks>
    /// Returns plant information if found.
    /// </remarks>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<PlantLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchPlants([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query is required" });

            _logger.LogInformation($"Searching plants with query: {query}");

            var plants = await _conditionService.SearchPlantsAsync(query);

            if (plants == null || plants.Count == 0)
                plants = new List<PlantLookupDto>();

            _logger.LogInformation($"Found {plants.Count} plants matching query: {query}");
            return Ok(plants);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error searching plants: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while searching plants" });
        }
    }
}
