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
        return result; // Return null if it fails, don't return fallback here
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
        var model = _config["Gemini:Model"] ?? "gemini-1.5-flash";
        if (string.IsNullOrEmpty(apiKey)) { _logger.LogError("Gemini:ApiKey غير مضبوط."); return null; }

        var baseUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = jsonMode
                ? new { temperature = 0.1, maxOutputTokens = 4096, responseMimeType = "application/json" }
                : new { temperature = 0.3, maxOutputTokens = 4096, responseMimeType = "text/plain" }
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync($"{baseUrl}?key={apiKey}", content);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (jsonMode && text is not null)
        {
            text = text.Trim().TrimStart('`').TrimEnd('`');
            if (text.StartsWith("json", StringComparison.OrdinalIgnoreCase)) text = text[4..].Trim();
        }
        return text;
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
        Role: You are a warm, friendly plant doctor named "دكتور فلورا". You speak to users like a best friend who happens to be a plant expert. Your tone is casual, reassuring, and helpful.

        CRITICAL GUARDRAIL (INPUT VALIDATION):
        Before analyzing, verify that the text is a plausible plant name. If it's a human, animal, random object, or gibberish, return ONLY this JSON:
        {
          "MainCategory": "Invalid",
          "SpecificIssue": "ليس نباتاً",
          "Treatment": "لا يوجد",
          "CareAdvice": {
            "Watering": "عذراً يا صديقي، لم أتمكن من التعرف على النبتة.",
            "Light": "تأكد من التقاط صورة واضحة للأوراق.",
            "Fertilizing": "رحلة العلاج تبدأ بصورة واضحة!",
            "Soil": "من فضلك حاول مجدداً.",
            "Humidity": "أنا بانتظارك!"
          }
        }

        Task (ONLY IF INPUT IS A VALID PLANT):
        Provide a medical diagnosis report based on the plant name and disease identified by our local model.
        
        Input Data:
        - Plant Name: [{{plantName}}]
        {{contextLine}}

        STRICT RULES FOR CONTENT:
        1. MainCategory must be exactly one of: ["فطريات", "بكتيريا", "فيروسات", "حشرات", "سليم"].
        2. Treatment (العلاج): 
           - **STRICTLY PROHIBIT** commercial brand names.
           - **Narrative Style:** Write the treatment as a professional, simplified story of recovery. Describe the condition and the path to cure like an expert guiding a friend through a journey, making it feel supportive and technical yet accessible.
           - **Terminology:** You MUST use the phrase "المادة الفعالة" when describing the active ingredient. **STRICTLY BAN** the phrase "المكون النشط".
           - **Directness:** DO NOT use long conversational intros like "يا صديقي لا تقلق" or "أهلاً بك". Start directly with the narrative of the plant's condition and the expert cure steps.
           - **Actionable:** Even if no chemical cure exists, describe mechanical steps (pruning, isolation) as part of the recovery story.
           - Format: Single flowing paragraph.
        3. CareAdvice (الرعاية): Provide specific prevention steps to stop THIS EXACT disease from returning.
           - This MUST be a JSON object with exactly 5 keys: "Watering", "Light", "Fertilizing", "Soil", "Humidity".
           - Each value must be a short, actionable Arabic sentence specific to the identified plant and disease.

        Writing Style:
        - Professional, Expert Storyteller tone.
        - Mention the plant name [{{plantName}}] naturally within the narrative.
        - Language: Arabic only.
        - NO line breaks, NO bullet points, NO backticks.

        Output (Strict JSON only):
        {
          "MainCategory": "[Term]",
          "SpecificIssue": "[Arabic Condition Name]",
          "Treatment": "[Professional narrative story of the cure journey using 'المادة الفعالة']",
          "CareAdvice": {
            "Watering": "[Sentence]",
            "Light": "[Sentence]",
            "Fertilizing": "[Sentence]",
            "Soil": "[Sentence]",
            "Humidity": "[Sentence]"
          }
        }
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
              "CareAdvice": {
                "Watering": "تبدو نبتتك في أفضل حالاتها يا صديقي! استمر في نفس جدول الرعاية الرائع من سقاية وإضاءة.",
                "Light": "تأكد من توفير الإضاءة المناسبة حسب نوع النبتة.",
                "Fertilizing": "لا تنسى التسميد الدوري في فصول النمو.",
                "Soil": "حافظ على تهوية التربة وتغييرها عند الحاجة.",
                "Humidity": "تأكد فقط من فحص الأوراق أسبوعياً للاطمئنان عليها."
              }
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
              "CareAdvice": {
                "Watering": "عذراً يا صديقي، لم أتمكن من التعرف على النبتة.",
                "Light": "تأكد من التقاط صورة واضحة للأوراق.",
                "Fertilizing": "رحلة العلاج تبدأ بصورة واضحة!",
                "Soil": "من فضلك حاول مجدداً.",
                "Humidity": "أنا بانتظارك!"
              }
            }
            """;
        }

        return $$"""
        {
          "MainCategory": "{{arabicDisease}}",
          "SpecificIssue": "{{diseaseLabel}}",
          "Treatment": "يرجى استخدام مبيد يحتوي على المادة الفعالة المناسبة لعلاج {{arabicDisease}} في نبتتك واستشر المورد المحلي.",
          "CareAdvice": {
            "Watering": "حافظ على تهوية جيدة وتقليل الري مؤقتاً.",
            "Light": "تجنب أشعة الشمس المباشرة الحارقة حالياً.",
            "Fertilizing": "توقف عن التسميد حتى تتعافى نبتتك.",
            "Soil": "تأكد من جودة صرف التربة.",
            "Humidity": "قلل الرطوبة حول الأوراق المصابة."
          }
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