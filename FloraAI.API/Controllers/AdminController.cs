using FloraAI.API.DTOs.Diagnosis;
using FloraAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FloraAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IConditionService _conditionService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IConditionService conditionService, ILogger<AdminController> logger)
    {
        _conditionService = conditionService;
        _logger = logger;
    }

    [HttpPost("refresh-condition")]
    public async Task<IActionResult> Refresh([FromBody] DiagnosisScanRequestDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.PlantType) || string.IsNullOrWhiteSpace(request.ConditionName))
            return BadRequest(new { message = "plantType and conditionName required" });

        var result = await _conditionService.ForceRefreshConditionAsync(request.PlantType, request.ConditionName, request.DetectedCategory);
        return Ok(result);
    }
}
