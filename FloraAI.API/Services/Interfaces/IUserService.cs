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
    Task<UserResponseDto> RegisterAsync(string fullName, string email, string password);

    /// <summary>
    /// Authenticates user and returns user info
    /// </summary>
    Task<UserResponseDto?> LoginAsync(string email, string password);

    /// <summary>
    /// Retrieves user by ID
    /// </summary>
    Task<User?> GetUserByIdAsync(int userId);

    /// <summary>
    /// Checks if email already exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email);
}
