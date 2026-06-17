namespace FloraAI.API.DTOs;

public class GeminiNewPlantResponse
{
    public int BaseWateringDays { get; set; }
    public string? WateringInstructions { get; set; }
    public string? SunlightRequirement { get; set; }
    public string? FertilizingInstructions { get; set; }
    public string? CareTips { get; set; }
    public string? ArabicName { get; set; }
}
