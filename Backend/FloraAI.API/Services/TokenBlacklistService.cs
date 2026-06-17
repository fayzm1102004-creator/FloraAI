using FloraAI.API.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace FloraAI.API.Services;

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IDistributedCache _cache;

    public TokenBlacklistService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task BlacklistTokenAsync(string jti, TimeSpan expiry)
    {
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry };
        await _cache.SetStringAsync($"blacklist:{jti}", "true", options);
    }

    public async Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        var value = await _cache.GetStringAsync($"blacklist:{jti}");
        return value != null;
    }
}
