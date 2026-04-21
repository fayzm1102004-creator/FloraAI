namespace FloraAI.API.DTOs.User;

public class UserResponseDto
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
}
