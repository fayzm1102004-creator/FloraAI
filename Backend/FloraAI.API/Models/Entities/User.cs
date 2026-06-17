namespace FloraAI.API.Models.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Security & Auth
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public Enums.UserRole Role { get; set; } = Enums.UserRole.User;

    // Navigation Properties
    public ICollection<UserPlant> UserPlants { get; set; } = new List<UserPlant>();
}
