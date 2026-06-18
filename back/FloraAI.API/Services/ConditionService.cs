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
        
        // TEMPORARILY DISABLED CACHE FOR TESTING - FORCING FRESH DATA
        // var cachedData = await TryGetCacheAsync<ConditionsDictionary>(cacheKey);
        // if (cachedData != null) return cachedData;

        var existingCondition = await _dbContext.ConditionsDictionary
            .FirstOrDefaultAsync(c =>
                c.PlantType.ToLower() == plantType.ToLower() &&
                c.ConditionName.ToLower() == conditionName.ToLower());

        var normalizedCondition = conditionName?.Trim().ToLower() ?? string.Empty;
        var isHealthyRequest = normalizedCondition == "healthy" || normalizedCondition == "سليم" || detectedCategory?.ToLower() == "healthy" || detectedCategory == "سليم";

        ConditionsDictionary result;

        if (isHealthyRequest)
        {
            if (existingCondition != null)
            {
                existingCondition.Treatment = null;
                existingCondition.WateringAdvice = "استمر في سقاية منتظمة حسب احتياج نوع النبتة.";
                existingCondition.LightAdvice = "توفير شمس كافية أو إضاءة مناسبة لمكان النبتة.";
                existingCondition.FertilizingAdvice = "التسميد المتوازن خلال فصول النمو فقط.";
                existingCondition.SoilAdvice = "تهوية التربة والتأكد من جودة الصرف.";
                existingCondition.HumidityAdvice = "افحص النبات أسبوعياً للتأكد من عدم ظهور أي علامات مرضية.";
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
                    WateringAdvice = "استمر في سقاية منتظمة حسب احتياج نوع النبتة.",
                    LightAdvice = "توفير شمس كافية أو إضاءة مناسبة لمكان النبتة.",
                    FertilizingAdvice = "التسميد المتوازن خلال فصول النمو فقط.",
                    SoilAdvice = "تهوية التربة والتأكد من جودة الصرف.",
                    HumidityAdvice = "افحص النبات أسبوعياً للتأكد من عدم ظهور أي علامات مرضية.",
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

        // TEMPORARILY DISABLED CACHE FOR TESTING
        // await TrySetCacheAsync(cacheKey, result);
        return result;
    }

    public async Task<ConditionsDictionary> ForceRefreshConditionAsync(string plantType, string conditionName, string? detectedCategory = null)
    {
        try
        {
            var jsonResponse = await _geminiService.GenerateArabicTreatmentTextAsync(plantType, conditionName, detectedCategory);
            
            if (string.IsNullOrEmpty(jsonResponse))
            {
                _logger.LogWarning("Gemini returned null. Returning in-memory fallback for {Plant}/{Condition}", plantType, conditionName);
                
                // Return fallback immediately and EXIT the method
                return new ConditionsDictionary
                {
                    Id = 0,
                    PlantType = plantType,
                    ConditionName = conditionName,
                    Treatment = "لا يوجد",
                    WateringAdvice = "عذراً يا صديقي، لم أتمكن من التعرف على النبتة حالياً.",
                    LightAdvice = "تأكد من التقاط صورة واضحة للأوراق.",
                    FertilizingAdvice = "رحلة العلاج تبدأ بصورة واضحة!",
                    SoilAdvice = "من فضلك حاول مجدداً.",
                    HumidityAdvice = "أنا بانتظارك!",
                    LastUpdated = DateTime.UtcNow
                };
            }

            string treatment = "غير متوفر";
            string wateringAdvice = "غير متوفر";
            string lightAdvice = "غير متوفر";
            string fertilizingAdvice = "غير متوفر";
            string soilAdvice = "غير متوفر";
            string humidityAdvice = "غير متوفر";

            try
            {
                var cleanJson = jsonResponse.Trim();
                if (cleanJson.StartsWith("```"))
                {
                    cleanJson = cleanJson.Replace("```json", "").Replace("```", "").Trim();
                }

                using var doc = JsonDocument.Parse(cleanJson);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("Treatment", out var treatProp))
                    treatment = treatProp.GetString() ?? treatment;

                if (root.TryGetProperty("CareAdvice", out var careObj) && careObj.ValueKind == JsonValueKind.Object)
                {
                    if (careObj.TryGetProperty("Watering", out var w)) wateringAdvice = w.GetString() ?? wateringAdvice;
                    if (careObj.TryGetProperty("Light", out var l)) lightAdvice = l.GetString() ?? lightAdvice;
                    if (careObj.TryGetProperty("Fertilizing", out var f)) fertilizingAdvice = f.GetString() ?? fertilizingAdvice;
                    if (careObj.TryGetProperty("Soil", out var s)) soilAdvice = s.GetString() ?? soilAdvice;
                    if (careObj.TryGetProperty("Humidity", out var h)) humidityAdvice = h.GetString() ?? humidityAdvice;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse Gemini JSON response. Raw: {Response}", jsonResponse);
                treatment = jsonResponse;
            }

            treatment = treatment.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ").Trim();
            wateringAdvice = wateringAdvice.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ").Trim();
            lightAdvice = lightAdvice.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ").Trim();
            fertilizingAdvice = fertilizingAdvice.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ").Trim();
            soilAdvice = soilAdvice.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ").Trim();
            humidityAdvice = humidityAdvice.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ").Trim();

            // Find existing to update, or create new
            var existing = await _dbContext.ConditionsDictionary
                .FirstOrDefaultAsync(c => c.PlantType.ToLower() == plantType.ToLower() && c.ConditionName.ToLower() == conditionName.ToLower());

            if (existing != null)
            {
                existing.Treatment = treatment;
                existing.WateringAdvice = wateringAdvice;
                existing.LightAdvice = lightAdvice;
                existing.FertilizingAdvice = fertilizingAdvice;
                existing.SoilAdvice = soilAdvice;
                existing.HumidityAdvice = humidityAdvice;
                existing.LastUpdated = DateTime.UtcNow;
                
                // EF will track changes automatically
            }
            else
            {
                existing = new ConditionsDictionary
                {
                    PlantType = plantType,
                    ConditionName = conditionName,
                    Treatment = treatment,
                    WateringAdvice = wateringAdvice,
                    LightAdvice = lightAdvice,
                    FertilizingAdvice = fertilizingAdvice,
                    SoilAdvice = soilAdvice,
                    HumidityAdvice = humidityAdvice,
                    LastUpdated = DateTime.UtcNow
                };
                _dbContext.ConditionsDictionary.Add(existing);
            }

            await _dbContext.SaveChangesAsync();
            await TrySetCacheAsync($"condition_{plantType.ToLower().Replace(" ", "_")}_{conditionName.ToLower().Replace(" ", "_")}", existing);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ForceRefreshConditionAsync for {Plant}/{Condition}", plantType, conditionName);
            
            // Debugging Fallback: Returns the actual error message to the client
            return new ConditionsDictionary
            {
                Id = 0,
                PlantType = plantType,
                ConditionName = conditionName,
                Treatment = $"Error: {ex.Message} || Inner: {ex.InnerException?.Message} || Trace: {ex.StackTrace?.Substring(0, Math.Min(ex.StackTrace.Length, 150))}",
                WateringAdvice = "تتبع الخطأ أعلاه لمعرفة سبب فشل السيرفر في جلب بيانات Gemini أو الاتصال بالداتابيز.",
                LightAdvice = "تأكد من إعدادات الـ API Key والـ Connection String.",
                FertilizingAdvice = "فشل النظام في معالجة الطلب.",
                SoilAdvice = "تحقق من سجلات الخادم (Logs).",
                HumidityAdvice = "محاولة استعادة النظام...",
                LastUpdated = DateTime.UtcNow
            };
        }
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
