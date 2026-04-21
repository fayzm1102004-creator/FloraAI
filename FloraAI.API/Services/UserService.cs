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

    public UserService(
        ApplicationDbContext dbContext,
        ILogger<UserService> logger,
        IMapper mapper)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Registers a new user with email and password
    /// </summary>
    public async Task<UserResponseDto> RegisterAsync(string fullName, string email, string password)
    {
        try
        {
            // Check if email already exists
            if (await EmailExistsAsync(email))
            {
                throw new InvalidOperationException("البريد الإلكتروني مسجل بالفعل");
            }

            // Hash password
            var passwordHash = HashPassword(password);

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = passwordHash
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"User registered: {user.Email}");

            return _mapper.Map<UserResponseDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error registering user: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Authenticates user with email and password
    /// </summary>
    public async Task<UserResponseDto?> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                _logger.LogWarning($"Login attempt for non-existent email: {email}");
                return null;
            }

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning($"Failed login attempt for: {email}");
                return null;
            }

            _logger.LogInformation($"User logged in: {user.Email}");

            return _mapper.Map<UserResponseDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during login: {ex.Message}");
            throw;
        }
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
    /// Hashes a password using PBKDF2
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
