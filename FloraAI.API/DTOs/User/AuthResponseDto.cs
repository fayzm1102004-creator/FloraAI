namespace FloraAI.API.DTOs.User;

public class AuthResponseDto
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
    public required UserResponseDto User { get; set; }
}

public class TokenRequestDto
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
}
