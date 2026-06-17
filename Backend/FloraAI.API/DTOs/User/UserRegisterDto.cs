namespace FloraAI.API.DTOs.User;

public class UserRegisterDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}
