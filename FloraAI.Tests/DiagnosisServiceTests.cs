using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FloraAI.API.Data;
using FloraAI.API.Models.Entities;
using FloraAI.API.Services;
using FloraAI.API.Services.Interfaces;
using FloraAI.API.DTOs.Diagnosis;

namespace FloraAI.Tests;

public class DiagnosisServiceTests
{
    private readonly DiagnosisService _service;
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IConditionService> _conditionServiceMock;
    private readonly Mock<ILogger<DiagnosisService>> _loggerMock;

    public DiagnosisServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _conditionServiceMock = new Mock<IConditionService>();
        _loggerMock = new Mock<ILogger<DiagnosisService>>();

        _service = new DiagnosisService(_conditionServiceMock.Object, _dbContext, _loggerMock.Object);
    }

    [Fact]
    public async Task ScanPlantAsync_ReturnsDiagnosisScanResponse()
    {
        // Arrange
        var plantType = "Tomato";
        var conditionName = "Blight";
        var condition = new ConditionsDictionary 
        { 
            PlantType = plantType, 
            ConditionName = conditionName,
            Treatment = "Use Fungicide",
            CareInstructions = "Water at base"
        };
        
        _conditionServiceMock.Setup(s => s.GetOrFetchConditionAsync(plantType, conditionName))
            .ReturnsAsync(condition);

        // Act
        var result = await _service.ScanPlantAsync(plantType, conditionName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(plantType, result.PlantType);
        Assert.Equal("Use Fungicide", result.Treatment);
    }

    [Fact]
    public async Task RecordScanAsync_ExistingPlant_AddsToHistory()
    {
        // Arrange
        var userPlant = new UserPlant 
        { 
            Id = 1, 
            UserId = 1, 
            Nickname = "My Tomato", 
            PlantType = "Tomato", 
            CurrentStatus = "Sick" 
        };
        _dbContext.UserPlants.Add(userPlant);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.RecordScanAsync(1, "Blight");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.UserPlantId);
        Assert.Equal("Blight", result.ConditionFound);
        
        var count = await _dbContext.ScanHistories.CountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task RecordScanAsync_NonExistingPlant_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _service.RecordScanAsync(999, "Blight"));
    }
}
