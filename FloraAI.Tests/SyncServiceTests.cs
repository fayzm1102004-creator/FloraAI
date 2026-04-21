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
using FloraAI.API.DTOs.Sync;
using FloraAI.API.DTOs.Diagnosis;

namespace FloraAI.Tests;

public class SyncServiceTests
{
    private readonly SyncService _service;
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IConditionService> _conditionServiceMock;
    private readonly Mock<IDiagnosisService> _diagnosisServiceMock;
    private readonly Mock<ILogger<SyncService>> _loggerMock;

    public SyncServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _conditionServiceMock = new Mock<IConditionService>();
        _diagnosisServiceMock = new Mock<IDiagnosisService>();
        _loggerMock = new Mock<ILogger<SyncService>>();
        
        var config = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<FloraAI.API.Mappings.MappingProfile>(), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        var mapper = config.CreateMapper();

        _service = new SyncService(
            _conditionServiceMock.Object, 
            _diagnosisServiceMock.Object, 
            _dbContext, 
            _loggerMock.Object,
            mapper);
    }

    [Fact]
    public async Task PullConditionsAsync_ReturnsUpdatedConditions()
    {
        // Arrange
        var lastSync = DateTime.UtcNow.AddDays(-1);
        var conditions = new List<ConditionsDictionary>
        {
            new ConditionsDictionary { Id = 1, PlantType = "T", ConditionName = "C", LastUpdated = DateTime.UtcNow }
        };

        _conditionServiceMock.Setup(s => s.GetConditionsSinceAsync(lastSync))
            .ReturnsAsync(conditions);

        // Act
        var result = await _service.PullConditionsAsync(lastSync);

        // Assert
        Assert.Single(result.NewConditions);
        Assert.Equal(1, result.NewConditions[0].Id);
    }

    [Fact]
    public async Task PushPendingScansAsync_ProcessesScans()
    {
        // Arrange
        var pendingScans = new List<DiagnosisScanRequestDto>
        {
            new DiagnosisScanRequestDto { PlantType = "Tomato", ConditionName = "Blight" }
        };

        _diagnosisServiceMock.Setup(s => s.ScanPlantAsync("Tomato", "Blight"))
            .ReturnsAsync(new DiagnosisScanResponseDto 
            { 
                PlantType = "Tomato", 
                ConditionName = "Blight", 
                Treatment = "T", 
                CareInstructions = "C",
                ScannedAt = DateTime.UtcNow
            });

        // Act
        var result = await _service.PushPendingScansAsync(pendingScans);

        // Assert
        Assert.Equal(1, result.ProcessedCount);
        Assert.Single(result.DiagnosisResults);
        Assert.Equal("Tomato", result.DiagnosisResults[0].PlantType);
    }

    [Fact]
    public async Task GetSyncStatusAsync_ReturnsStats()
    {
        // Arrange
        _dbContext.ConditionsDictionary.Add(new ConditionsDictionary { Id = 1, PlantType = "T", ConditionName = "C1", Treatment = "T" });
        _dbContext.ConditionsDictionary.Add(new ConditionsDictionary { Id = 2, PlantType = "T", ConditionName = "C2", Treatment = null });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetSyncStatusAsync();

        // Assert
        Assert.NotNull(result);
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.Equal(2, doc.RootElement.GetProperty("TotalConditionsInDatabase").GetInt32());
        Assert.Equal(1, doc.RootElement.GetProperty("CachedConditions").GetInt32());
    }
}
