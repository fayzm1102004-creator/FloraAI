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
    private readonly AutoMapper.IMapper _mapper;

    public AdminController(IConditionService conditionService, ILogger<AdminController> logger, AutoMapper.IMapper mapper)
    {
        _conditionService = conditionService;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpPost("refresh-condition")]
    public async Task<IActionResult> Refresh([FromBody] DiagnosisScanRequestDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.PlantType) || string.IsNullOrWhiteSpace(request.ConditionName))
            return BadRequest(new { message = "plantType and conditionName required" });

        var result = await _conditionService.ForceRefreshConditionAsync(request.PlantType, request.ConditionName, request.DetectedCategory);
        var response = _mapper.Map<FloraAI.API.DTOs.Conditions.ConditionResponseDto>(result);
        return Ok(response);
    }
}
