namespace FloraAI.API.Services;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using FloraAI.API.Data;
using FloraAI.API.Models.Entities;
using FloraAI.API.Services.Interfaces;

public class ConditionService : IConditionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IGeminiService _geminiService;
    private readonly ILogger<ConditionService> _logger;

    public ConditionService(
        ApplicationDbContext dbContext,
        IGeminiService geminiService,
        ILogger<ConditionService> logger)
    {
        _dbContext = dbContext;
        _geminiService = geminiService;
        _logger = logger;
    }

    public async Task<ConditionsDictionary> GetOrFetchConditionAsync(string plantType, string conditionName, string? detectedCategory = null)
    {
        // البحث في قاعدة البيانات أولاً
        var existingCondition = await _dbContext.ConditionsDictionary
            .FirstOrDefaultAsync(c =>
                c.PlantType.ToLower() == plantType.ToLower() &&
                c.ConditionName.ToLower() == conditionName.ToLower());

        // Normalize and treat English "healthy" as Arabic "سليم"
        var normalizedCondition = conditionName?.Trim().ToLower() ?? string.Empty;
        var isHealthyRequest = normalizedCondition == "healthy" || normalizedCondition == "سليم" || detectedCategory?.ToLower() == "healthy" || detectedCategory == "سليم";

        if (isHealthyRequest)
        {
            var care = "استمر في سقاية منتظمة، توفير شمس كافية، وتهوية جيدة. افحص النبات أسبوعياً للتأكد من عدم ظهور أي علامات مرضية.";

            if (existingCondition != null)
            {
                existingCondition.Treatment = null;
                existingCondition.CareInstructions = care;
                existingCondition.LastUpdated = DateTime.UtcNow;
                try { await _dbContext.SaveChangesAsync(); } catch { }
                _logger.LogInformation($"إرجاع حالة سليم من الذاكرة المخبأة: {plantType}/{conditionName}");
                return existingCondition;
            }

            var healthyCondition = new ConditionsDictionary
            {
                PlantType = plantType,
                ConditionName = conditionName,
                Treatment = null,
                CareInstructions = care,
                LastUpdated = DateTime.UtcNow
            };

            _dbContext.ConditionsDictionary.Add(healthyCondition);
            try { await _dbContext.SaveChangesAsync(); } catch { }
            _logger.LogInformation($"تم إنشاء حالة سليم وتخزينها: {plantType}/{conditionName}");
            return healthyCondition;
        }

        // If the requested conditionName is exactly one of the canonical categories,
        // return a deterministic default treatment+care immediately (avoid calling Gemini).
        var canonical = normalizedCondition;
        var canonicalDefaults = new Dictionary<string, (string treatment, string care)>()
        {
            ["افات"] = ("1. عزل النبات المصاب فوراً. 2. رش مبيد حشري مناسب وفق التعليمات. 3. إزالة الحشرات والبيوض يدوياً.", "قلل الري قليلاً، راقب الحشرات، وحافظ على التهوية.") ,
            ["افات"] = ("1. عزل النبات المصاب فوراً. 2. رش مبيد حشري مناسب وفق التعليمات. 3. إزالة الحشرات والبيوض يدوياً.", "قلل الري قليلاً، راقب الحشرات، وحافظ على التهوية.") ,
            ["افات"] = ("1. عزل النبات المصاب فوراً. 2. رش مبيد حشري مناسب وفق التعليمات. 3. إزالة الحشرات والبيوض يدوياً.", "قلل الري قليلاً، راقب الحشرات، وحافظ على التهوية.") ,
            ["فطريات"] = ("1. إزالة الأنسجة المصابة. 2. تطبيق مبيد فطري مناسب مثل النحاس أو الكبريت. 3. تحسين التهوية وتقليل الرطوبة.", "تجنب الرطوبة الزائدة وتوفير تهوية جيدة.") ,
            ["فيروس"] = ("لا يوجد علاج دوائي فعال للفيروسات؛ قم بإزالة النباتات المصابة ومنع الانتقال عن طريق التعقيم.", "استخدم نباتات مقاومة وراقب الحشرات الناقلة.") ,
            ["بكتيريا"] = ("1. إزالة الأنسجة المصابة وتطهير الأدوات. 2. تحسين الصرف وتجنب الإفراط في الري.", "تحسين التهوية وتجنب الري الزائد.")
        };

        // match by contains to tolerate small variations
        (string treatment, string care)? matchedDef = null;
        foreach (var kv in canonicalDefaults)
        {
            if (canonical.Contains(kv.Key)) { matchedDef = kv.Value; break; }
        }

        if (matchedDef.HasValue)
        {
            var def = matchedDef.Value;
            if (existingCondition != null)
            {
                existingCondition.Treatment = def.treatment;
                existingCondition.CareInstructions = def.care;
                existingCondition.LastUpdated = DateTime.UtcNow;
                try { await _dbContext.SaveChangesAsync(); } catch { }
                _logger.LogInformation($"إرجاع افتراضي لمجموعة canonical: {plantType}/{conditionName}");
                return existingCondition;
            }

            var newCond = new ConditionsDictionary
            {
                PlantType = plantType,
                ConditionName = conditionName,
                Treatment = def.treatment,
                CareInstructions = def.care,
                LastUpdated = DateTime.UtcNow
            };
            _dbContext.ConditionsDictionary.Add(newCond);
            try { await _dbContext.SaveChangesAsync(); } catch { }
            _logger.LogInformation($"تم إنشاء سجل افتراضي لفئة canonical: {plantType}/{conditionName}");
            return newCond;
        }

        if (existingCondition != null && !string.IsNullOrEmpty(existingCondition.Treatment))
        {
            _logger.LogInformation($"تم العثور على الحالة في الذاكرة المخبئة: {plantType}/{conditionName}");
            return existingCondition;
        }

        // لم يتم العثور - استدعاء Gemini API
        _logger.LogInformation($"الحالة غير موجودة في الذاكرة المخبئة، استدعاء Gemini API: {plantType}/{conditionName}");

        try
        {
            var jsonResponse = await _geminiService.GenerateArabicTreatmentTextAsync(plantType, conditionName, detectedCategory);

            // Parse JSON response to extract Treatment and Care
            string treatment = "غير متوفر";
            string careInstructions = "غير متوفر";
            string? mappedCategoryOutside = null;

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                try
                {
                    using var doc = JsonDocument.Parse(jsonResponse);
                    var root = doc.RootElement;
                    // Normalize Category to one of the accepted five types
                    string? mappedCategory = null;
                    if (root.TryGetProperty("Category", out var categoryProp))
                    {
                        var catRaw = categoryProp.GetString()?.Trim().ToLower() ?? string.Empty;
                        mappedCategory = catRaw switch
                        {
                            "fungi" or "فطريات" or "فطري" => "فطريات",
                            "bacteria" or "بacteria" or "بكتيريا" => "بكتيريا",
                            "virus" or "فيرس" or "فيروس" => "فيروس",
                            "pests" or "افات" or "آفات" or "آفات حشرية" => "آفات",
                            "healthy" or "سليم" => "سليم",
                            _ => null
                        };
                    }

                    mappedCategoryOutside = mappedCategory;

                    if (mappedCategory == "سليم")
                    {
                        // Healthy: no treatment, only care
                        treatment = null;
                        if (root.TryGetProperty("Care", out var careProp))
                            careInstructions = careProp.GetString() ?? careInstructions;
                    }
                    
                    // If no treatment returned for a non-healthy category, provide category-specific defaults
                    if (!string.IsNullOrEmpty(mappedCategory) && mappedCategory != "سليم")
                    {
                        var isGeneric = string.IsNullOrEmpty(treatment) || treatment == "غير متوفر";
                        if (isGeneric)
                        {
                            treatment = mappedCategory switch
                            {
                                "آفات" or "آفات" or "افات" => "1. عزل النبات المصاب فوراً. 2. استخدم مبيد حشري مناسب للحشرة المستهدفة (رش حسب التعليمات). 3. إزالة الحشرات والبيوض يدوياً ومراقبة يومية.",
                                "فطريات" => "1. إزالة الأوراق والسيقان المصابة. 2. تطبيق مبيد فطري واسع الطيف مثل مبيدات تحتوي على النحاس أو الكبريت حسب شدة الإصابة. 3. تحسين التهوية وتقليل الرطوبة.",
                                "فيروس" => "لا يوجد علاج دوائي فعال للفيروسات؛ 1. إزالة النباتات المصابة ومنع انتقالها عن طريق التعقيم. 2. استخدام نباتات مقاومة وراقب الحشرات الناقلة.",
                                "بكتيريا" => "1. إزالة الأنسجة المصابة وتطهير الأدوات. 2. تحسين الصرف والتهوية وتجنب الإفراط في الري. 3. استخدام مبيدات أو مطهرات مخصصة إن لزم.",
                                _ => treatment
                            };
                        }
                    }
                    else
                    {
                        if (root.TryGetProperty("Treatment", out var treatmentProp))
                            treatment = treatmentProp.GetString() ?? treatment;

                        if (root.TryGetProperty("Care", out var careProp))
                            careInstructions = careProp.GetString() ?? careInstructions;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"فشل تحليل JSON response: {ex.Message}. استخدام النص الكامل كـ treatment.");
                    treatment = jsonResponse;
                }
            }

            if (existingCondition != null)
            {
                // تحديث السجل الموجود
                existingCondition.Treatment = treatment;
                existingCondition.CareInstructions = careInstructions;
                existingCondition.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                // إنشاء سجل جديد
                existingCondition = new ConditionsDictionary
                {
                    PlantType = plantType,
                    ConditionName = conditionName,
                    Treatment = treatment,
                    CareInstructions = careInstructions,
                    LastUpdated = DateTime.UtcNow
                };
                _dbContext.ConditionsDictionary.Add(existingCondition);
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"تم حفظ الحالة في الذاكرة المخبئة: {plantType}/{conditionName}");

            return existingCondition;
        }
        catch (Exception ex)
        {
            _logger.LogError($"خطأ في إنشاء الحالة عبر Gemini: {ex.Message}");
            
            // الرجوع للبيانات الموجودة إذا توفرت
            if (existingCondition != null)
            {
                _logger.LogInformation($"إرجاع الحالة الموجودة بقيم افتراضية: {plantType}/{conditionName}");
                return existingCondition;
            }

            // إرجاع بيانات محاكاة
            _logger.LogInformation($"إرجاع حالة محاكاة لـ: {plantType}/{conditionName}");
            var mockCondition = new ConditionsDictionary
            {
                PlantType = plantType,
                ConditionName = conditionName,
                Treatment = "راقب النبات عن كثب وتأكد من الري والشمس المناسبة. اتصل بأخصائي النباتات إذا استمرت الحالة.",
                CareInstructions = "1. سقاية بانتظام وإبقاء التربة رطبة\n2. توفير شمس كافية\n3. إزالة الأوراق المصابة\n4. تأكد من تدوير الهواء الجيد\n5. ضع زيت النيم عند الحاجة",
                LastUpdated = DateTime.UtcNow
            };
            
            try
            {
                _dbContext.ConditionsDictionary.Add(mockCondition);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                _logger.LogError($"لا يمكن حفظ الحالة المحاكاة: {dbEx.Message}");
            }

            return mockCondition;
        }
    }

    /// <summary>
    /// Searches for a condition without creating if not found
    /// </summary>
    public async Task<ConditionsDictionary?> FindConditionAsync(string plantType, string conditionName)
    {
        return await _dbContext.ConditionsDictionary
            .FirstOrDefaultAsync(c =>
                c.PlantType.ToLower() == plantType.ToLower() &&
                c.ConditionName.ToLower() == conditionName.ToLower());
    }

    /// <summary>
    /// Gets all conditions (for complete sync)
    /// </summary>
    public async Task<List<ConditionsDictionary>> GetAllConditionsAsync()
    {
        return await _dbContext.ConditionsDictionary
            .OrderByDescending(c => c.LastUpdated)
            .ToListAsync();
    }

    /// <summary>
    /// Gets conditions updated since a specific date (for incremental sync)
    /// </summary>
    public async Task<List<ConditionsDictionary>> GetConditionsSinceAsync(DateTime lastSyncDate)
    {
        return await _dbContext.ConditionsDictionary
            .Where(c => c.LastUpdated >= lastSyncDate)
            .OrderByDescending(c => c.LastUpdated)
            .ToListAsync();
    }

    /// <summary>
    /// Get conditions by plant type
    /// </summary>
    public async Task<List<ConditionsDictionary>> GetConditionsByPlantTypeAsync(string plantType)
    {
        return await _dbContext.ConditionsDictionary
            .Where(c => c.PlantType.ToLower() == plantType.ToLower())
            .OrderByDescending(c => c.LastUpdated)
            .ToListAsync();
    }

    /// <summary>
    /// Get a specific condition by plant type and name
    /// </summary>
    public async Task<ConditionsDictionary?> GetConditionAsync(string plantType, string conditionName)
    {
        return await _dbContext.ConditionsDictionary
            .FirstOrDefaultAsync(c =>
                c.PlantType.ToLower() == plantType.ToLower() &&
                c.ConditionName.ToLower() == conditionName.ToLower());
    }

    public async Task<ConditionsDictionary> ForceRefreshConditionAsync(string plantType, string conditionName, string? detectedCategory = null)
    {
        // Always regenerate and overwrite cached record
        _logger.LogInformation($"Force refreshing condition: {plantType}/{conditionName}");

        // Call Gemini or apply defaults (reuse existing parsing logic)
        var jsonResponse = await _geminiService.GenerateArabicTreatmentTextAsync(plantType, conditionName, detectedCategory);

        string treatment = "غير متوفر";
        string careInstructions = "غير متوفر";

        if (!string.IsNullOrEmpty(jsonResponse))
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                string? mappedCategory = null;
                if (root.TryGetProperty("Category", out var categoryProp))
                {
                    var catRaw = categoryProp.GetString()?.Trim().ToLower() ?? string.Empty;
                    mappedCategory = catRaw switch
                    {
                        "fungi" or "فطريات" or "فطري" => "فطريات",
                        "bacteria" or "بacteria" or "بكتيريا" => "بكتيريا",
                        "virus" or "فيرس" or "فيروس" => "فيروس",
                        "pests" or "افات" or "آفات" or "آفات حشرية" => "آفات",
                        "healthy" or "سليم" => "سليم",
                        _ => null
                    };
                }

                if (mappedCategory == "سليم")
                {
                    treatment = null;
                    if (root.TryGetProperty("Care", out var careProp))
                        careInstructions = careProp.GetString() ?? careInstructions;
                }
                else
                {
                    if (root.TryGetProperty("Treatment", out var treatmentProp))
                        treatment = treatmentProp.GetString() ?? treatment;

                    if (root.TryGetProperty("Care", out var careProp))
                        careInstructions = careProp.GetString() ?? careInstructions;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"فشل تحليل JSON response during ForceRefresh: {ex.Message}. Using full text as treatment.");
                treatment = jsonResponse;
            }
        }

        // category-specific defaults (if still generic)
        var lower = conditionName?.Trim().ToLower() ?? string.Empty;
        if (string.IsNullOrEmpty(treatment) || treatment == "غير متوفر")
        {
            if (lower.Contains("فات") || lower.Contains("افات") || lower.Contains("آفات"))
                treatment = "1. عزل النبات المصاب فوراً. 2. رش مبيد حشري مناسب وفق التعليمات. 3. إزالة الحشرات والبيوض يدوياً.";
            else if (lower.Contains("فطر") || lower.Contains("فطري") || lower.Contains("فطريات"))
                treatment = "1. إزالة الأنسجة المصابة. 2. تطبيق مبيد فطري مناسب مثل النحاس أو الكبريت. 3. تحسين التهوية وتقليل الرطوبة.";
            else if (lower.Contains("فير") || lower.Contains("فيروس") || lower.Contains("فيرس"))
                treatment = "لا يوجد علاج دوائي فعال للفيروسات؛ قم بإزالة النباتات المصابة ومنع الانتقال عن طريق التعقيم.";
            else if (lower.Contains("بكت") || lower.Contains("بكتيريا"))
                treatment = "1. إزالة الأنسجة المصابة وتطهير الأدوات. 2. تحسين الصرف وتجنب الإفراط في الري.";
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
        return existing;
    }

    /// <summary>
    /// Get all unique plants from lookup
    /// </summary>
    public async Task<List<FloraAI.API.DTOs.PlantLookup.PlantLookupDto>> GetAllPlantsAsync()
    {
        var plants = await _dbContext.PlantLookups
            .OrderBy(p => p.CommonName)
            .ToListAsync();

        return plants.Select(p => new FloraAI.API.DTOs.PlantLookup.PlantLookupDto
        {
            Id = p.Id,
            CommonName = p.CommonName,
            DefaultImage = p.DefaultImage
        }).ToList();
    }

    /// <summary>
    /// Search plants by name
    /// </summary>
    public async Task<List<FloraAI.API.DTOs.PlantLookup.PlantLookupDto>> SearchPlantsAsync(string query)
    {
        var searchTerm = query.ToLower();
        var plants = await _dbContext.PlantLookups
            .Where(p => p.CommonName.ToLower().Contains(searchTerm))
            .OrderBy(p => p.CommonName)
            .ToListAsync();

        return plants.Select(p => new FloraAI.API.DTOs.PlantLookup.PlantLookupDto
        {
            Id = p.Id,
            CommonName = p.CommonName,
            DefaultImage = p.DefaultImage
        }).ToList();
    }
}
