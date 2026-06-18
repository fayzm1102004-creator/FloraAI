using FloraAI.API.DTOs.Diagnosis;
using FloraAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FloraAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IConditionService _conditionService;
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;
    private readonly AutoMapper.IMapper _mapper;

    public AdminController(IConditionService conditionService, IAdminService adminService, ILogger<AdminController> logger, AutoMapper.IMapper mapper)
    {
        _conditionService = conditionService;
        _adminService = adminService;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            _logger.LogInformation("Admin requesting dashboard stats");
            var stats = await _adminService.GetDashboardStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving dashboard stats: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while calculating statistics" });
        }
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
