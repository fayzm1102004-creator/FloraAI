namespace FloraAI.API.Services;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using FloraAI.API.Data;
using FloraAI.API.Models.Entities;
using FloraAI.API.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using FloraAI.API.DTOs.Common;
using FloraAI.API.DTOs.PlantLookup;

public class ConditionService : IConditionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IGeminiService _geminiService;
    private readonly ILogger<ConditionService> _logger;
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _config;

    public ConditionService(
        ApplicationDbContext dbContext,
        IGeminiService geminiService,
        ILogger<ConditionService> logger,
        IDistributedCache cache,
        IConfiguration config)
    {
        _dbContext = dbContext;
        _geminiService = geminiService;
        _logger = logger;
        _cache = cache;
        _config = config;
    }

    public async Task<ConditionsDictionary> GetOrFetchConditionAsync(string plantType, string conditionName, string? detectedCategory = null)
    {
        var cacheKey = $"condition_{plantType.ToLower().Replace(" ", "_")}_{conditionName.ToLower().Replace(" ", "_")}";
        
        var cachedData = await TryGetCacheAsync<ConditionsDictionary>(cacheKey);
        if (cachedData != null) return cachedData;

        var existingCondition = await _dbContext.ConditionsDictionary
            .FirstOrDefaultAsync(c =>
                c.PlantType.ToLower() == plantType.ToLower() &&
                c.ConditionName.ToLower() == conditionName.ToLower());

        var normalizedCondition = conditionName?.Trim().ToLower() ?? string.Empty;
        var isHealthyRequest = normalizedCondition == "healthy" || normalizedCondition == "سليم" || detectedCategory?.ToLower() == "healthy" || detectedCategory == "سليم";

        ConditionsDictionary result;

        if (isHealthyRequest)
        {
            var care = "استمر في سقاية منتظمة، توفير شمس كافية، وتهوية جيدة. افحص النبات أسبوعياً للتأكد من عدم ظهور أي علامات مرضية.";
            if (existingCondition != null)
            {
                existingCondition.Treatment = null;
                existingCondition.CareInstructions = care;
                existingCondition.LastUpdated = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                result = existingCondition;
            }
            else
            {
                result = new ConditionsDictionary
                {
                    PlantType = plantType,
                    ConditionName = conditionName,
                    Treatment = null,
                    CareInstructions = care,
                    LastUpdated = DateTime.UtcNow
                };
                _dbContext.ConditionsDictionary.Add(result);
                await _dbContext.SaveChangesAsync();
            }
        }
        else if (existingCondition != null && !string.IsNullOrEmpty(existingCondition.Treatment))
        {
            result = existingCondition;
        }
        else
        {
            result = await ForceRefreshConditionAsync(plantType, conditionName, detectedCategory);
        }

        await TrySetCacheAsync(cacheKey, result);
        return result;
    }

    public async Task<ConditionsDictionary> ForceRefreshConditionAsync(string plantType, string conditionName, string? detectedCategory = null)
    {
        try
        {
            var jsonResponse = await _geminiService.GenerateArabicTreatmentTextAsync(plantType, conditionName, detectedCategory);
            
            string treatment = "غير متوفر";
            string careInstructions = "غير متوفر";

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                try
                {
                    using var doc = JsonDocument.Parse(jsonResponse);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("Treatment", out var tProp)) treatment = tProp.GetString() ?? treatment;
                    if (root.TryGetProperty("Care", out var cProp)) careInstructions = cProp.GetString() ?? careInstructions;
                }
                catch { treatment = jsonResponse; }
            }

            var existing = await _dbContext.ConditionsDictionary
                .FirstOrDefaultAsync(c => c.PlantType.ToLower() == plantType.ToLower() && c.ConditionName.ToLower() == conditionName.ToLower());

            if (existing != null)
            {
                existing.Treatment = treatment;
                existing.CareInstructions = careInstructions;
                existing.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                existing = new ConditionsDictionary
                {
                    PlantType = plantType,
                    ConditionName = conditionName,
                    Treatment = treatment,
                    CareInstructions = careInstructions,
                    LastUpdated = DateTime.UtcNow
                };
                _dbContext.ConditionsDictionary.Add(existing);
            }

            await _dbContext.SaveChangesAsync();
            await TrySetCacheAsync($"condition_{plantType.ToLower().Replace(" ", "_")}_{conditionName.ToLower().Replace(" ", "_")}", existing);
            return existing;
        }
        catch { throw; }
    }

    public async Task<PagedResponse<PlantLookupDto>> GetAllPlantsAsync(int pageNumber = 1, int pageSize = 10)
    {
        var cacheKey = $"all_plants_p{pageNumber}_s{pageSize}";
        var cached = await TryGetCacheAsync<PagedResponse<PlantLookupDto>>(cacheKey);
        if (cached != null) return cached;

        var query = _dbContext.PlantLookups.AsNoTracking();
        var totalRecords = await query.CountAsync();
        
        var plants = await query
            .OrderBy(p => p.CommonName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PlantLookupDto
            {
                Id = p.Id,
                CommonName = p.CommonName,
                DefaultImage = p.DefaultImage
            })
            .ToListAsync();

        var response = new PagedResponse<PlantLookupDto>(plants, pageNumber, pageSize, totalRecords);
        await TrySetCacheAsync(cacheKey, response);
        return response;
    }

    public async Task<PagedResponse<PlantLookupDto>> SearchPlantsAsync(string query, int pageNumber = 1, int pageSize = 10)
    {
        var searchTerm = query.ToLower();
        var baseQuery = _dbContext.PlantLookups
            .AsNoTracking()
            .Where(p => p.CommonName.ToLower().Contains(searchTerm));

        var totalRecords = await baseQuery.CountAsync();
        
        var plants = await baseQuery
            .OrderBy(p => p.CommonName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PlantLookupDto
            {
                Id = p.Id,
                CommonName = p.CommonName,
                DefaultImage = p.DefaultImage
            })
            .ToListAsync();

        return new PagedResponse<PlantLookupDto>(plants, pageNumber, pageSize, totalRecords);
    }

    #region Cache Helpers
    private async Task<T?> TryGetCacheAsync<T>(string key) where T : class
    {
        try
        {
            var jsonData = await _cache.GetStringAsync(key);
            return jsonData == null ? null : JsonSerializer.Deserialize<T>(jsonData);
        }
        catch { return null; }
    }

    private async Task TrySetCacheAsync<T>(string key, T data) where T : class
    {
        try
        {
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(double.Parse(_config["Redis:DefaultExpirationInMinutes"] ?? "30")) };
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(data), options);
        }
        catch { }
    }
    #endregion

    public async Task<ConditionsDictionary?> FindConditionAsync(string plantType, string conditionName) => await GetConditionAsync(plantType, conditionName);
    public async Task<List<ConditionsDictionary>> GetAllConditionsAsync() => await _dbContext.ConditionsDictionary.AsNoTracking().OrderByDescending(c => c.LastUpdated).ToListAsync();
    public async Task<List<ConditionsDictionary>> GetConditionsSinceAsync(DateTime lastSyncDate) => await _dbContext.ConditionsDictionary.AsNoTracking().Where(c => c.LastUpdated >= lastSyncDate).ToListAsync();
    public async Task<List<ConditionsDictionary>> GetConditionsByPlantTypeAsync(string plantType) => await _dbContext.ConditionsDictionary.AsNoTracking().Where(c => c.PlantType.ToLower() == plantType.ToLower()).ToListAsync();
    public async Task<ConditionsDictionary?> GetConditionAsync(string plantType, string conditionName) => 
        await _dbContext.ConditionsDictionary.AsNoTracking().FirstOrDefaultAsync(c => c.PlantType.ToLower() == plantType.ToLower() && c.ConditionName.ToLower() == conditionName.ToLower());
}
