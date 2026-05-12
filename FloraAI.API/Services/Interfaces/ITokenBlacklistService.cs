namespace FloraAI.API.Services.Interfaces;

public interface ITokenBlacklistService
{
    Task BlacklistTokenAsync(string jti, TimeSpan expiry);
    Task<bool> IsTokenBlacklistedAsync(string jti);
}
