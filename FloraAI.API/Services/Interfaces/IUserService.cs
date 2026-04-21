namespace FloraAI.API.Services.Interfaces;

using FloraAI.API.DTOs.User;
using FloraAI.API.Models.Entities;

/// <summary>
/// Service for user authentication and account management
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Registers a new user
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(string fullName, string email, string password);

    /// <summary>
    /// Authenticates user and returns tokens
    /// </summary>
    Task<AuthResponseDto?> LoginAsync(string email, string password);

    /// <summary>
    /// Refreshes access token using a valid refresh token
    /// </summary>
    Task<AuthResponseDto?> RefreshTokenAsync(string token, string refreshToken);

    /// <summary>
    /// Retrieves user by ID
    /// </summary>
    Task<User?> GetUserByIdAsync(int userId);

    /// <summary>
    /// Checks if email already exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email);
}
