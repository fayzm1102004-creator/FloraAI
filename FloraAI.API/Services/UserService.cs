namespace FloraAI.API.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FloraAI.API.Data;
using FloraAI.API.DTOs.User;
using FloraAI.API.Models.Entities;
using FloraAI.API.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Implementation of UserService - handles authentication and account management
/// </summary>
public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;

    public UserService(
        ApplicationDbContext dbContext,
        ILogger<UserService> logger,
        IMapper mapper,
        ITokenService tokenService,
        IConfiguration config)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
        _tokenService = tokenService;
        _config = config;
    }

    /// <summary>
    /// Registers a new user with email and password
    /// </summary>
    public async Task<AuthResponseDto> RegisterAsync(string fullName, string email, string password)
    {
        try
        {
            if (await EmailExistsAsync(email))
            {
                throw new InvalidOperationException("البريد الإلكتروني مسجل بالفعل");
            }

            var passwordHash = HashPassword(password);

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = passwordHash,
                Role = Models.Enums.UserRole.User // Default role
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return await GenerateAuthResponse(user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error registering user: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Authenticates user and returns tokens
    /// </summary>
    public async Task<AuthResponseDto?> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning($"Failed login attempt for: {email}");
                return null;
            }

            return await GenerateAuthResponse(user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during login: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Refreshes access token using a valid refresh token
    /// </summary>
    public async Task<AuthResponseDto?> RefreshTokenAsync(string token, string refreshToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(token);
        var userEmail = principal.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;

        if (userEmail == null) return null;

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            return null;
        }

        return await GenerateAuthResponse(user);
    }

    private async Task<AuthResponseDto> GenerateAuthResponse(User user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(_config["Jwt:RefreshTokenValidityInDays"]!));

        await _dbContext.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:TokenValidityInMinutes"]!)),
            User = _mapper.Map<UserResponseDto>(user)
        };
    }

    /// <summary>
    /// Retrieves a user by ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _dbContext.Users.FindAsync(userId);
    }

    /// <summary>
    /// Checks if email already exists in the system
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    /// <summary>
    /// Hashes a password using SHA256
    /// </summary>
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }
    }

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    private bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(hash);
    }
}
