using FloraAI.API.Data;
using FloraAI.API.DTOs.Admin;
using FloraAI.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FloraAI.API.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AdminService> _logger;
    private const string StatsCacheKey = "admin_dashboard_stats";

    public AdminService(ApplicationDbContext dbContext, IDistributedCache cache, ILogger<AdminService> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var cachedStats = await _cache.GetStringAsync(StatsCacheKey);
        if (!string.IsNullOrEmpty(cachedStats))
        {
            return JsonSerializer.Deserialize<DashboardStatsDto>(cachedStats)!;
        }

        var totalUsers = await _dbContext.Users.CountAsync();
        var scanHistoryQuery = _dbContext.ScanHistories.AsNoTracking();
        var totalScans = await scanHistoryQuery.CountAsync();

        // Top 5 Plants (Join with ConditionsDictionary to get PlantType)
        var topPlants = await scanHistoryQuery
            .GroupBy(s => s.ConditionsDictionary!.PlantType)
            .Select(g => new TopPlantDto
            {
                PlantName = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        // Category Distribution (Group by ConditionName as a proxy for category)
        var categoryDist = await scanHistoryQuery
            .GroupBy(s => s.ConditionsDictionary!.ConditionName)
            .Select(g => new CategoryStatDto
            {
                Category = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var stats = new DashboardStatsDto
        {
            TotalUsers = totalUsers,
            TotalScans = totalScans,
            TopPlants = topPlants,
            CategoryDistribution = categoryDist
        };

        var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };
        await _cache.SetStringAsync(StatsCacheKey, JsonSerializer.Serialize(stats), cacheOptions);

        return stats;
    }
}
