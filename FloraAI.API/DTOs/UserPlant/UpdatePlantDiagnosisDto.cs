namespace FloraAI.API.DTOs.UserPlant;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// DTO for updating the saved diagnosis/treatment for an existing plant
/// </summary>
public class UpdatePlantDiagnosisDto
{
    [Required]
    public int PlantId { get; set; }

    [Required]
    public string Treatment { get; set; } = string.Empty;

    [Required]
    public FloraAI.API.DTOs.Diagnosis.CareAdviceDto CareAdvice { get; set; } = new();
}
