using FloraAI.API.DTOs.Conditions;
using FloraAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FloraAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConditionsController : ControllerBase
{
    private readonly IConditionService _conditionService;
    private readonly ILogger<ConditionsController> _logger;
    private readonly AutoMapper.IMapper _mapper;

    public ConditionsController(IConditionService conditionService, ILogger<ConditionsController> logger, AutoMapper.IMapper mapper)
    {
        _conditionService = conditionService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all conditions for a specific plant type.
    /// </summary>
    /// <remarks>
    /// Returns all known conditions and treatments for a given plant type.
    /// Useful for browsing reference information.
    /// </remarks>
    [HttpGet("plant/{plantType}")]
    [ProducesResponseType(typeof(IEnumerable<ConditionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPlantConditions(string plantType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(plantType))
                return BadRequest(new { message = "PlantType is required" });

            _logger.LogInformation($"Retrieving conditions for plant type: {plantType}");

            var conditions = await _conditionService.GetConditionsByPlantTypeAsync(plantType);

            if (conditions == null || !conditions.Any())
            {
                _logger.LogInformation($"No conditions found for plant type: {plantType}");
                return Ok(new List<ConditionResponseDto>());
            }

            var response = _mapper.Map<List<ConditionResponseDto>>(conditions);

            _logger.LogInformation($"Retrieved {response.Count} conditions for plant type: {plantType}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error retrieving conditions: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving conditions" });
        }
    }

    /// <summary>
    /// Get a specific condition.
    /// </summary>
    /// <remarks>
    /// Returns detailed information about a specific plant condition.
    /// </remarks>
    [HttpGet("{plantType}/{conditionName}")]
    [ProducesResponseType(typeof(ConditionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCondition(string plantType, string conditionName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(plantType))
                return BadRequest(new { message = "PlantType is required" });

            if (string.IsNullOrWhiteSpace(conditionName))
                return BadRequest(new { message = "ConditionName is required" });

            _logger.LogInformation($"Retrieving condition - PlantType: {plantType}, Condition: {conditionName}");

            var condition = await _conditionService.GetConditionAsync(plantType, conditionName);

            if (condition == null)
            {
                _logger.LogWarning($"Condition not found - PlantType: {plantType}, Condition: {conditionName}");
                return NotFound(new { message = "Condition not found" });
            }

            var response = _mapper.Map<ConditionResponseDto>(condition);

            _logger.LogInformation($"Retrieved condition successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error retrieving condition: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving the condition" });
        }
    }

    /// <summary>
    /// Get all conditions in the database.
    /// </summary>
    /// <remarks>
    /// Returns complete ConditionsDictionary for reference.
    /// </remarks>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<ConditionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllConditions()
    {
        try
        {
            _logger.LogInformation("Retrieving all conditions");

            var conditions = await _conditionService.GetAllConditionsAsync();

            if (conditions == null)
                conditions = new List<Models.Entities.ConditionsDictionary>();

            var response = _mapper.Map<List<ConditionResponseDto>>(conditions);

            _logger.LogInformation($"Retrieved {response.Count} total conditions");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error retrieving all conditions: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving conditions" });
        }
    }
}
