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
    /// <remarks>
    /// Creates a new user with email and password.
    /// Returns user details on success.
    /// </remarks>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.RegisterAsync(request.FullName, request.Email, request.Password);
            
            _logger.LogInformation($"User registered successfully: {user.Email}");
            return CreatedAtAction(nameof(Register), user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Registration failed: {ex.Message}");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error during registration: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "حدث خطأ أثناء التسجيل" });
        }
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    /// <remarks>
    /// Authenticates user credentials and returns user details if valid.
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.LoginAsync(request.Email, request.Password);
            
            if (user == null)
            {
                _logger.LogWarning($"Login failed for email: {request.Email}");
                return Unauthorized(new { message = "البريد الإلكتروني أو كلمة المرور غير صحيحة" });
            }

            _logger.LogInformation($"User logged in successfully: {user.Email}");
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error during login: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "حدث خطأ أثناء تسجيل الدخول" });
        }
    }
}
