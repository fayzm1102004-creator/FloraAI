using FloraAI.API.DTOs.User;
using FloraAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FloraAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authResponse = await _userService.RegisterAsync(request.FullName, request.Email, request.Password);
            
            return CreatedAtAction(nameof(Register), authResponse);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error during registration: {ex.Message}");
            return StatusCode(500, new { message = "حدث خطأ أثناء التسجيل" });
        }
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authResponse = await _userService.LoginAsync(request.Email, request.Password);
            
            if (authResponse == null)
            {
                return Unauthorized(new { message = "البريد الإلكتروني أو كلمة المرور غير صحيحة" });
            }

            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error during login: {ex.Message}");
            return StatusCode(500, new { message = "حدث خطأ أثناء تسجيل الدخول" });
        }
    }

    /// <summary>
    /// Refresh the access token using a refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto request)
    {
        try
        {
            var authResponse = await _userService.RefreshTokenAsync(request.Token, request.RefreshToken);

            if (authResponse == null)
            {
                return Unauthorized(new { message = "انتهت صلاحية الجلسة، يرجى تسجيل الدخول مرة أخرى" });
            }

            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error refreshing token: {ex.Message}");
            return Unauthorized(new { message = "طلب غير صالح" });
        }
    }
}
