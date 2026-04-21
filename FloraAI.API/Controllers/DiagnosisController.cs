using FloraAI.API.DTOs.Diagnosis;
using FloraAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FloraAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosisController : ControllerBase
{
    private readonly IDiagnosisService _diagnosisService;
    private readonly IConditionService _conditionService;
    private readonly ILogger<DiagnosisController> _logger;
    private readonly AutoMapper.IMapper _mapper;

    public DiagnosisController(
        IDiagnosisService diagnosisService,
        IConditionService conditionService,
        ILogger<DiagnosisController> logger,
        AutoMapper.IMapper mapper)
    {
        _diagnosisService = diagnosisService;
        _conditionService = conditionService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Scan a plant and diagnose its condition.
    /// </summary>
    /// <remarks>
    /// Searches ConditionsDictionary for the plant condition.
    /// If not found, calls Gemini API and caches the result.
    /// Returns treatment and care instructions.
    /// </remarks>
    [HttpPost("scan")]
    [ProducesResponseType(typeof(DiagnosisScanResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Scan([FromBody] DiagnosisScanRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.PlantType))
                return BadRequest(new { message = "نوع النبات مطلوب" });

            if (string.IsNullOrWhiteSpace(request.ConditionName))
                return BadRequest(new { message = "اسم الحالة مطلوب" });

            _logger.LogInformation($"Diagnosis scan requested - PlantType: {request.PlantType}, Condition: {request.ConditionName}");

            // Get or fetch condition from Gemini
            var condition = await _conditionService.GetOrFetchConditionAsync(
                request.PlantType,
                request.ConditionName,
                request.DetectedCategory);

            if (condition == null)
            {
                _logger.LogWarning($"Failed to retrieve condition data for {request.PlantType}/{request.ConditionName}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "فشل في الحصول على معلومات التشخيص" });
            }

            var response = _mapper.Map<DiagnosisScanResponseDto>(condition);
            if (string.IsNullOrWhiteSpace(response.CareInstructions))
            {
                 response.CareInstructions = "لا توجد تعليمات رعاية متاحة";
            }

            _logger.LogInformation($"Diagnosis completed successfully for {request.PlantType}/{request.ConditionName}");
            return Ok(response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Gemini API error during diagnosis: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "خدمة الذكاء الاصطناعي الخارجية غير متاحة مؤقتاً" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error during diagnosis scan: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء التشخيص" });
        }
    }
}
