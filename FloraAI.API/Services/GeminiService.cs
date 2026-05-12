using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using FloraAI.API.DTOs;
using FloraAI.API.Services.Interfaces;

namespace FloraAI.API.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient http, IConfiguration config, ILogger<GeminiService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<string> GenerateArabicTreatmentTextAsync(string plantName, string diseaseLabel, string? detectedCategory = null)
    {
        var sanitizedPlant = SanitizeInput(plantName);
        var sanitizedDisease = SanitizeInput(diseaseLabel);
        var sanitizedCategory = detectedCategory != null ? SanitizeInput(detectedCategory) : null;

        var result = await CallGeminiAsync(BuildTreatmentPrompt(sanitizedPlant, sanitizedDisease, sanitizedCategory), jsonMode: true);
        return result ?? BuildFallbackText(sanitizedPlant, sanitizedDisease);
    }

    public async Task<GeminiNewPlantResponse?> GenerateNewPlantDataAsync(string plantName)
    {
        var sanitizedPlant = SanitizeInput(plantName);
        var raw = await CallGeminiAsync(BuildNewPlantPrompt(sanitizedPlant), jsonMode: true);
        if (raw is null) return null;
        try
        {
            return JsonSerializer.Deserialize<GeminiNewPlantResponse>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "فشل تحليل JSON لبيانات النبات '{Plant}'.", plantName);
            return null;
        }
    }

    private async Task<string?> CallGeminiAsync(string prompt, bool jsonMode)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:Model"] ?? "gemini-2.5-flash";
        if (string.IsNullOrEmpty(apiKey)) { _logger.LogError("Gemini:ApiKey غير مضبوط."); return null; }

        var baseUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = jsonMode
                ? new { temperature = 0.1, maxOutputTokens = 4096, responseMimeType = "application/json" }
                : new { temperature = 0.3, maxOutputTokens = 4096, responseMimeType = "text/plain" }
        };

        try
        {
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{baseUrl}?key={apiKey}", content);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var text = doc.RootElement
                .GetProperty("candidates")[0].GetProperty("content")
                .GetProperty("parts")[0].GetProperty("text").GetString();

            if (jsonMode && text is not null)
            {
                text = text.Trim().TrimStart('`').TrimEnd('`');
                if (text.StartsWith("json", StringComparison.OrdinalIgnoreCase)) text = text[4..].Trim();
            }
            return text;
        }
        catch (Exception ex) { _logger.LogError(ex, "فشل استدعاء Gemini API."); return null; }
    }

    private static string BuildTreatmentPrompt(string plantName, string diseaseLabel, string? detectedCategory)
    {
        if (detectedCategory?.ToLower() == "healthy" || detectedCategory == "سليم" || diseaseLabel.ToLower() == "healthy" || diseaseLabel == "سليم")
        {
             return $$"""
                Role: You are a warm, friendly plant doctor named "دكتور فلورا". You speak to users like a best friend who happens to be a plant expert.
                Task: The plant [{{plantName}}] has been scanned and detected as HEALTHY (سليم).
                
                Strict Constraints:
                1. MainCategory: Must be "سليم".
                2. SpecificIssue: Must be "النبات بصحة جيدة".
                3. Treatment: Must be "لا يحتاج علاج".
                4. CareAdvice: Write ONE short flowing paragraph (2-3 sentences) with tips to keep the plant healthy. Mention the plant name [{{plantName}}]. No newlines.
                5. Language: Arabic. Format: JSON only.
                {
                  "MainCategory": "سليم",
                  "SpecificIssue": "النبات بصحة جيدة",
                  "Treatment": "لا يحتاج علاج",
                  "CareAdvice": "[نصائح رعاية ووقاية قصيرة بأسلوب ودود]"
                }
                """;
        }

        string contextLine = !string.IsNullOrWhiteSpace(detectedCategory)
            ? $"Issue/Description: [{diseaseLabel}] (Model suspected Category: [{detectedCategory}])"
            : $"Issue/Description: [{diseaseLabel}]";

        return $$"""
        Role: You are a warm, friendly plant doctor named "دكتور فلورا". You speak to users like a best friend who happens to be a plant expert. Your tone is casual, warm, and reassuring — never robotic or formal.

        CRITICAL GUARDRAIL (INPUT VALIDATION):
        Before analyzing, verify that the text is a plausible plant name. If it's a human, animal, random object, or gibberish, return ONLY this JSON:
        {
          "MainCategory": "Invalid",
          "SpecificIssue": "ليس نباتاً",
          "Treatment": "لا يوجد",
          "CareAdvice": "عذراً يا صديقي، لم أتمكن من التعرف على أي نبتة في هذه الصورة. قد تكون الصورة غير واضحة أو تحتوي على شيء آخر. من فضلك، التقط صورة مقربة وواضحة لأوراق أو ساق النبتة لنبدأ رحلة العلاج معاً!"
        }

        Task (ONLY IF INPUT IS A VALID PLANT):
        Categorize the MainCategory into exactly ONE of these 5 Arabic terms:
        ["فطريات", "بكتيريا", "فيروسات", "حشرات", "سليم"]
        
        Plant Name: [{{plantName}}]
        {{contextLine}}

        YOU MUST RETURN 4 SEPARATE FIELDS:
        - Treatment (العلاج): خطوات العلاج الفعلية لإنقاذ النبتة من هذا المرض تحديداً. اذكر أسماء منتجات محددة وجدول زمني واضح.
        - CareAdvice (الرعاية): نصائح الرعاية والوقاية المرتبطة بنفس المرض تحديداً. اشرح كيف يمنع المستخدم هذا المرض بالذات من الرجوع مرة أخرى.

        CRITICAL: CareAdvice MUST be disease-specific, NOT generic plant care. If the disease is فطريات, the care must be about preventing fungi. If it's حشرات, the care must be about preventing pests. NEVER give generic watering/sunlight advice.

        STRICT WRITING RULES:
        1. Both Treatment and CareAdvice must be ONE single flowing paragraph each. NEVER use line breaks, bullet points, or numbered lists.
        2. Talk like you're chatting with a friend. Use words like "يا صديقي", "لا تقلق", "خليني أقولك".
        3. You MUST mention the plant name [{{plantName}}] naturally.
        4. Treatment: 2-3 sentences about the exact cure (مبيدات، عزل، تقليم، إلخ).
        5. CareAdvice: 2-3 sentences about how to prevent THIS SPECIFIC disease from returning.
        6. NO newline characters inside any field.

        Language: Arabic only.

        Output (Strict JSON, no markdown, no backticks):
        {
          "MainCategory": "[ONE of the 5 terms]",
          "SpecificIssue": "[اسم المرض بدقة]",
          "Treatment": "[فقرة واحدة عن خطوات العلاج المحددة]",
          "CareAdvice": "[فقرة واحدة عن نصائح الرعاية والوقاية]"
        }

        OUTPUT REQUIREMENT: Return ONLY the JSON object, nothing else.
        """;
    }

    private static string BuildNewPlantPrompt(string plantName) => $$"""
    You are a strict botanical expert API. Your ONLY output must be a single valid JSON object with NO markdown, NO backticks, NO explanation.

    Plant Name: {{plantName}}

    IMPORTANT: All string values MUST be written in the Arabic language (العربية). The JSON keys MUST remain in English.

    Return this exact JSON structure:
    {
      "BaseWateringDays": <integer>,
      "WateringInstructions": "<One sentence in Arabic explaining watering>",
      "SunlightRequirement": "<One sentence in Arabic explaining sunlight>",
      "FertilizingInstructions": "<One sentence in Arabic explaining fertilizer>",
      "CareTips": "<One sentence in Arabic with a care tip>",
      "ArabicName": "<The common Arabic name for the plant>"
    }
    """;

    private static string BuildFallbackText(string plantName, string diseaseLabel)
    {
        var arabicDisease = diseaseLabel.ToLower() switch
        {
            "fungi" => "فطريات",
            "rot" => "فطريات",
            "bacteria" => "بكتيريا",
            "virus" => "فيروسات",
            "pests" => "حشرات",
            "healthy" => "سليم",
            "سليم" => "سليم",
            _=> "غير معروف"
        };

        if (arabicDisease == "سليم")
        {
            return $$"""
            {
              "MainCategory": "سليم",
              "SpecificIssue": "النبات بصحة جيدة",
              "Treatment": "لا يحتاج علاج",
              "CareAdvice": "تبدو نبتتك في أفضل حالاتها يا صديقي! استمر في نفس جدول الرعاية الرائع من سقاية وإضاءة، وتأكد فقط من فحص الأوراق أسبوعياً للاطمئنان عليها."
            }
            """;
        }

        if (arabicDisease == "غير معروف")
        {
            return $$"""
            {
              "MainCategory": "Invalid",
              "SpecificIssue": "غير معروف",
              "Treatment": "لا يوجد",
              "CareAdvice": "عذراً يا صديقي، لم أتمكن من التعرف على أي نبتة في هذه الصورة. قد تكون الصورة غير واضحة أو تحتوي على شيء آخر. من فضلك، التقط صورة مقربة وواضحة لأوراق أو ساق النبتة لنبدأ رحلة العلاج معاً!"
            }
            """;
        }

        return $$"""
        {
          "MainCategory": "{{arabicDisease}}",
          "SpecificIssue": "{{diseaseLabel}}",
          "Treatment": "يرجى استشارة متخصص لعلاج {{arabicDisease}} في نبتتك.",
          "CareAdvice": "حافظ على تهوية جيدة وتقليل الري مؤقتاً حتى تتعافى نبتتك."
        }
        """;
    }

    private static string SanitizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        
        if (input.Length > 100)
        {
            throw new ValidationException("Input exceeds maximum length of 100 characters.");
        }

        // Strip JSON control characters: { } " \ ` [ ]
        var sanitized = Regex.Replace(input, @"[\{\}\""\\`\[\]]", "");

        // Allow only Arabic letters, English letters, spaces, numbers, dots, dashes
        if (!Regex.IsMatch(sanitized, @"^[\p{IsArabic}a-zA-Z0-9\s\.\-]+$"))
        {
            throw new ValidationException("Input contains invalid characters.");
        }

        return sanitized;
    }
}