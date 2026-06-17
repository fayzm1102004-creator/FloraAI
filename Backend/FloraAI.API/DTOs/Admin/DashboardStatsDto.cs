using System.Collections.Generic;

namespace FloraAI.API.DTOs.Admin;

public class DashboardStatsDto
{
    public int TotalScans { get; set; }
    public int TotalUsers { get; set; }
    public List<TopPlantDto> TopPlants { get; set; } = new();
    public List<CategoryStatDto> CategoryDistribution { get; set; } = new();
}

public class TopPlantDto
{
    public string PlantName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class CategoryStatDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}
