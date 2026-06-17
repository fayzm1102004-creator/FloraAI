using FloraAI.API.DTOs;

namespace FloraAI.API.Services.Interfaces;

public interface IGeminiService
{
    Task<string> GenerateArabicTreatmentTextAsync(string plantName, string diseaseLabel, string? detectedCategory = null);
    Task<GeminiNewPlantResponse?> GenerateNewPlantDataAsync(string plantName);
}
