using FloraAI.API.DTOs.User;
using FloraAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace FloraAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthLimit")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;
    private readonly ITokenBlacklistService _tokenBlacklistService;

    public AuthController(IUserService userService, ILogger<AuthController> logger, ITokenBlacklistService tokenBlacklistService)
    {
        _userService = userService;
        _logger = logger;
        _tokenBlacklistService = tokenBlacklistService;
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]

    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto request, [FromServices] IWebHostEnvironment env)
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
            if (env.IsDevelopment())
            {
                return StatusCode(500, new 
                { 
                    message = ex.Message, 
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace 
                });
            }
            return StatusCode(500, new { message = "حدث خطأ أثناء التسجيل" });
        }
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    [AllowAnonymous]
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
    [AllowAnonymous]
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

    /// <summary>
    /// Logout and invalidate the current token.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]

    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var jti = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
        var expClaim = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Exp)?.Value;

        if (jti != null && expClaim != null && long.TryParse(expClaim, out var exp))
        {
            var expTime = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
            var timeRemaining = expTime - DateTime.UtcNow;
            if (timeRemaining > TimeSpan.Zero)
            {
                await _tokenBlacklistService.BlacklistTokenAsync(jti, timeRemaining);
            }
        }

        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }
}
