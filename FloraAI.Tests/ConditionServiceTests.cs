using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

using FloraAI.API.Data;
using FloraAI.API.Models.Entities;
using FloraAI.API.Services;
using FloraAI.API.Services.Interfaces;

namespace FloraAI.Tests;

public class ConditionServiceTests
{
    private readonly ConditionService _service;
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<ConditionService>> _loggerMock;
    private readonly Mock<IGeminiService> _geminiMock;

    public ConditionServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "ConditionDb")
            .Options;
        _dbContext = new ApplicationDbContext(options);
        // Seed data
        _dbContext.ConditionsDictionary.AddRange(
            new ConditionsDictionary { PlantType = "Plant", ConditionName = "Fungi", Treatment = null, CareInstructions = null },
            new ConditionsDictionary { PlantType = "Plant", ConditionName = "Bacteria", Treatment = null, CareInstructions = null }
        );
        _dbContext.SaveChanges();
        _loggerMock = new Mock<ILogger<ConditionService>>();
        _geminiMock = new Mock<IGeminiService>();
        _geminiMock.Setup(g => g.GenerateArabicTreatmentTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("{}");
        _service = new ConditionService(_dbContext, _geminiMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllConditionsAsync_ReturnsAll()
    {
        var result = await _service.GetAllConditionsAsync();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetConditionByLabelAsync_ExistingLabel_ReturnsCondition()
    {
        var result = await _service.GetConditionAsync("Plant", "Fungi");
        Assert.NotNull(result);
        Assert.Equal("Fungi", result.ConditionName);
    }

// Additional tests for ConditionService covering healthy, canonical, and Gemini paths
    [Fact]
    public async Task GetOrFetchConditionAsync_Healthy_ReturnsCareOnly()
    {
        var result = await _service.GetOrFetchConditionAsync("Plant", "healthy");
        Assert.NotNull(result);
        Assert.Null(result.Treatment);
        Assert.NotNull(result.CareInstructions);
    }

    [Fact]
    public async Task GetOrFetchConditionAsync_Canonical_ReturnsDefaultTreatment()
    {
        var result = await _service.GetOrFetchConditionAsync("Plant", "افات");
        Assert.NotNull(result);
        Assert.Contains("مبيد", result.Treatment);
    }

    [Fact]
    public async Task GetOrFetchConditionAsync_GeminiResponse_ParsesJson()
    {
        _geminiMock.Setup(g => g.GenerateArabicTreatmentTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("{\"Treatment\":\"علاج تجريبي\",\"Care\":\"رعاية تجريبية\"}");
        var result = await _service.GetOrFetchConditionAsync("Plant", "unknownCondition");
        Assert.NotNull(result);
        Assert.Equal("علاج تجريبي", result.Treatment);
        Assert.Equal("رعاية تجريبية", result.CareInstructions);
    }

    [Fact]
    public async Task GetAllPlantsAsync_ReturnsUniquePlants()
    {
        // Arrange
        _dbContext.PlantLookups.Add(new PlantLookup { Id = 10, CommonName = "Rose" });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllPlantsAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, p => p.CommonName == "Rose");
    }

    [Fact]
    public async Task SearchPlantsAsync_ValidQuery_ReturnsMatches()
    {
        // Arrange
        _dbContext.PlantLookups.Add(new PlantLookup { Id = 11, CommonName = "Cactus" });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.SearchPlantsAsync("Cactus");

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ForceRefreshConditionAsync_CallsGeminiAndUpdate()
    {
        // Arrange
        _geminiMock.Setup(g => g.GenerateArabicTreatmentTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("{\"Treatment\":\"New Treatment\",\"Care\":\"New Care\"}");

        // Act
        var result = await _service.ForceRefreshConditionAsync("Plant", "Fungi");

        // Assert
        Assert.Equal("New Treatment", result.Treatment);
        var updated = await _dbContext.ConditionsDictionary.FirstOrDefaultAsync(c => c.ConditionName == "Fungi");
        Assert.Equal("New Treatment", updated?.Treatment);
    }

    [Fact]
    public async Task GetOrFetchConditionAsync_MalformedJson_UsesResponseAsTreatment()
    {
        // Arrange
        _geminiMock.Setup(g => g.GenerateArabicTreatmentTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("Not a JSON string");

        // Act
        var result = await _service.GetOrFetchConditionAsync("Rose", "Black Spot");

        // Assert
        Assert.Equal("Not a JSON string", result.Treatment); // Should fall into catch and use raw text
    }

    [Fact]
    public async Task GetOrFetchConditionAsync_GeminiThrows_ReturnsMockData()
    {
        // Arrange
        _geminiMock.Setup(g => g.GenerateArabicTreatmentTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new System.Exception("API Down"));

        // Act
        var result = await _service.GetOrFetchConditionAsync("Rose", "Unknown");

        // Assert
        Assert.Contains("راقب النبات", result.Treatment); // Returns mock condition from catch block
    }

    [Fact]
    public async Task GetOrFetchConditionAsync_CategorySwitch_CoversAllBranches()
    {
        // Test "فطريات"
        _geminiMock.Setup(g => g.GenerateArabicTreatmentTextAsync(It.IsAny<string>(), It.IsAny<string>(), null))
            .ReturnsAsync("{\"Category\":\"فطريات\", \"Treatment\":\"\", \"Care\":\"Care\"}");
        var result = await _service.GetOrFetchConditionAsync("Rose", "FungiTest");
        Assert.Contains("مبيد فطري", result.Treatment);

        // Test "آفات"
        _geminiMock.Setup(g => g.GenerateArabicTreatmentTextAsync(It.IsAny<string>(), It.IsAny<string>(), null))
            .ReturnsAsync("{\"Category\":\"آفات\", \"Treatment\":\"\", \"Care\":\"Care\"}");
        result = await _service.GetOrFetchConditionAsync("Rose", "PestTest");
        Assert.Contains("مبيد حشري", result.Treatment);
    }

    [Fact]
    public async Task GetOrFetchConditionAsync_WithDetectedCategory_PassesToGemini()
    {
        // Arrange
        var plant = "Tomato";
        var condition = "Spots";
        var category = "فطريات";
        _geminiMock.Setup(g => g.GenerateArabicTreatmentTextAsync(plant, condition, category))
            .ReturnsAsync("{\"Category\":\"فطريات\", \"Diagnosis\":\"Late Blight\", \"Treatment\":\"Treat\", \"Care\":\"Care\"}");

        // Act
        var result = await _service.GetOrFetchConditionAsync(plant, condition, category);

        // Assert
        Assert.Equal(condition, result.ConditionName); 
        _geminiMock.Verify(g => g.GenerateArabicTreatmentTextAsync(plant, condition, category), Times.Once);
    }
}
