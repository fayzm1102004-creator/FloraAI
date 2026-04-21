namespace FloraAI.API.DTOs.Conditions;

public class ConditionResponseDto
{
    public int Id { get; set; }
    public required string PlantType { get; set; }
    public required string ConditionName { get; set; }
    public string? Treatment { get; set; }
    public required string CareInstructions { get; set; }
    public DateTime LastUpdated { get; set; }
}
