using System.Text;
using System.Text.Json;
using FloraAI.API.DTOs;
using FloraAI.API.Services.Interfaces;

namespace FloraAI.API.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<GeminiService> _logger;

    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

    public GeminiService(HttpClient http, IConfiguration config, ILogger<GeminiService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<string> GenerateArabicTreatmentTextAsync(string plantName, string diseaseLabel, string? detectedCategory = null)
    {
        var result = await CallGeminiAsync(BuildTreatmentPrompt(plantName, diseaseLabel, detectedCategory), jsonMode: true);
        return result ?? BuildFallbackText(plantName, diseaseLabel);
    }

    public async Task<GeminiNewPlantResponse?> GenerateNewPlantDataAsync(string plantName)
    {
        var raw = await CallGeminiAsync(BuildNewPlantPrompt(plantName), jsonMode: true);
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
        if (string.IsNullOrEmpty(apiKey)) { _logger.LogError("Gemini:ApiKey غير مضبوط."); return null; }

        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = jsonMode
                ? new { temperature = 0.1, maxOutputTokens = 1024, responseMimeType = "application/json" }
                : new { temperature = 0.3, maxOutputTokens = 1024, responseMimeType = "text/plain" }
        };

        try
        {
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{BaseUrl}?key={apiKey}", content);
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
        string categoryInstruction = string.IsNullOrWhiteSpace(detectedCategory) 
            ? "Your task is to analyze the plant and classify its disease into ONLY one of these four categories: (فطريات, فيروس, آفات, بكتيريا)."
            : $"Our primary vision model has detected that this plant damage belongs to the category: [{detectedCategory}]. Your task is to confirm this and identify the SPECIFIC disease name within this category for the [{plantName}] plant.";

        if (detectedCategory?.ToLower() == "healthy" || detectedCategory == "سليم" || diseaseLabel.ToLower() == "healthy")
        {
             return $$"""
                You are a plant expert. The plant [{{plantName}}] has been scanned and detected as HEALTHY (سليم).
                
                Strict Constraints:
                1. Category: Must be "سليم".
                2. Diagnosis: Must be "النبات بصحة جيدة" or similar.
                3. Treatment: Must be "لا يوجد - النبات بصحة جيدة".
                4. Care: Provide specific long-term maintenance for a healthy [{{plantName}}].
                5. Language: Arabic.
                6. Format: JSON only.
                {
                  "Category": "سليم",
                  "Diagnosis": "النبات بصحة جيدة",
                  "Treatment": "لا يوجد - النبات بصحة جيدة",
                  "Care": "Step by step care instructions..."
                }
                """;
        }

        return $$"""
        You are a professional agricultural expert and plant pathologist. 
        Plant Name: [{{plantName}}]
        User Description/Label: [{{diseaseLabel}}]
        
        {{categoryInstruction}}

        Strict Constraints:

        Classification: You must use (فطريات, فيروس, آفات, بكتيريا) as the "Category".
        
        Structure: You must separate 'Treatment' (العلاج) from 'Care' (الرعاية).

        Treatment: Specific steps to cure the current disease (e.g., specific pesticides, removing infected leaves).

        Care: Long-term maintenance and prevention (e.g., watering schedule, sun exposure, soil type).

        Language: All output content must be in Arabic.

        Format: Return the response in a clean JSON format with NO markdown, NO backticks, NO additional text:
        {
          "Category": "Category Name",
          "Diagnosis": "Specific Disease Name",
          "Treatment": "Steps for treatment",
          "Care": "Steps for long-term care"
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
            "bacteria" => "بكتيريا",
            "virus" => "فيروس",
            "pests" => "آفات حشرية",
            _=> "سليم"
        };

        if (diseaseLabel.ToLower() == "healthy")
        {
            return $$"""
            {
              "Category": "سليم",
              "Diagnosis": "النبات بصحة جيدة",
              "Treatment": "لا يوجد - النبات بصحة جيدة",
              "Care": "استمر في سقاية منتظمة، توفير شمس كافية، وتهوية جيدة. افحص النبات أسبوعياً للتأكد من عدم ظهور أي علامات مرضية."
            }
            """;
        }

        return $$"""
        {
          "Category": "{{arabicDisease}}",
          "Diagnosis": "{{diseaseLabel}}",
          "Treatment": "1. عزل النبات فوراً عن باقي النباتات. 2. إزالة الأوراق المصابة بأداة معقمة. 3. تطبيق مبيد مناسب لـ{{arabicDisease}}. 4. المراقبة اليومية.",
          "Care": "قلل الري قليلاً، تجنب التسميد أثناء العلاج، تأكد من التهوية الجيدة، حافظ على رطوبة التربة المعتدلة."
        }
        """;
    }
}