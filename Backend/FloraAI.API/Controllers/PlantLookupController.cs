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
    /// Get available plants with pagination.
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(FloraAI.API.DTOs.Common.PagedResponse<PlantLookupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPlants([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            _logger.LogInformation($"Retrieving plants (Page: {pageNumber}, Size: {pageSize})");

            var pagedResponse = await _conditionService.GetAllPlantsAsync(pageNumber, pageSize);

            return Ok(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error retrieving plants: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while retrieving plants" });
        }
    }

    /// <summary>
    /// Search for a specific plant with pagination.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(FloraAI.API.DTOs.Common.PagedResponse<PlantLookupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchPlants([FromQuery] string query, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query is required" });

            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            _logger.LogInformation($"Searching plants: {query} (Page: {pageNumber}, Size: {pageSize})");

            var pagedResponse = await _conditionService.SearchPlantsAsync(query, pageNumber, pageSize);

            return Ok(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error searching plants: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while searching plants" });
        }
    }
}
