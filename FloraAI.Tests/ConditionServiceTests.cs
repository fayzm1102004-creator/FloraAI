using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using System.Linq;

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
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<IConfiguration> _configMock;

    public ConditionServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        
        _loggerMock = new Mock<ILogger<ConditionService>>();
        _geminiMock = new Mock<IGeminiService>();
        _cacheMock = new Mock<IDistributedCache>();
        _configMock = new Mock<IConfiguration>();

        _configMock.Setup(c => c["Redis:DefaultExpirationInMinutes"]).Returns("30");

        _service = new ConditionService(_dbContext, _geminiMock.Object, _loggerMock.Object, _cacheMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task GetOrFetchConditionAsync_CacheHit_ReturnsCachedValue()
    {
        var plant = "Rose";
        var condition = "Black Spot";
        var cacheKey = $"condition_{plant.ToLower()}_{condition.ToLower().Replace(" ", "_")}";
        var cachedObj = new ConditionsDictionary { PlantType = plant, ConditionName = condition, Treatment = "Cached Treatment" };
        var json = JsonSerializer.Serialize(cachedObj);
        
        _cacheMock.Setup(c => c.GetAsync(cacheKey, default)).ReturnsAsync(Encoding.UTF8.GetBytes(json));

        var result = await _service.GetOrFetchConditionAsync(plant, condition);

        Assert.Equal("Cached Treatment", result.Treatment);
        _geminiMock.Verify(g => g.GenerateArabicTreatmentTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetOrFetchConditionAsync_CacheMiss_SetsCache()
    {
        var plant = "Rose";
        var condition = "healthy";
        var cacheKey = $"condition_{plant.ToLower()}_{condition.ToLower()}";
        _cacheMock.Setup(c => c.GetAsync(cacheKey, default)).ReturnsAsync((byte[]?)null);

        await _service.GetOrFetchConditionAsync(plant, condition);

        _cacheMock.Verify(c => c.SetAsync(cacheKey, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
    }

    [Fact]
    public async Task GetOrFetchConditionAsync_RedisError_FallbackToDb()
    {
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ThrowsAsync(new Exception("Redis Error"));
        _dbContext.ConditionsDictionary.Add(new ConditionsDictionary { PlantType = "Tomato", ConditionName = "Blight", Treatment = "DB" });
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetOrFetchConditionAsync("Tomato", "Blight");

        Assert.Equal("DB", result.Treatment);
    }

    [Fact]
    public async Task GetAllPlantsAsync_ReturnsPagedResponse()
    {
        for(int i=0; i<12; i++)
            _dbContext.PlantLookups.Add(new PlantLookup { Id = i+100, CommonName = $"P{i}" });
        await _dbContext.SaveChangesAsync();
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync((byte[]?)null);

        var result = await _service.GetAllPlantsAsync(1, 10);

        Assert.Equal(10, result.Data.Count());
        Assert.Equal(12, result.TotalRecords);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task SearchPlantsAsync_ReturnsPagedMatches()
    {
        for(int i=0; i<5; i++)
            _dbContext.PlantLookups.Add(new PlantLookup { Id = i+200, CommonName = $"Cactus{i}" });
        await _dbContext.SaveChangesAsync();
        
        var result = await _service.SearchPlantsAsync("Cactus", 1, 2);

        Assert.Equal(2, result.Data.Count());
        Assert.Equal(5, result.TotalRecords);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task GetAllConditionsAsync_ReturnsAll()
    {
        _dbContext.ConditionsDictionary.Add(new ConditionsDictionary { PlantType = "P", ConditionName = "C", Treatment = "T" });
        await _dbContext.SaveChangesAsync();
        var result = await _service.GetAllConditionsAsync();
        Assert.Single(result);
    }

    [Fact]
    public async Task ForceRefreshConditionAsync_CallsGeminiAndUpdate()
    {
        _geminiMock.Setup(g => g.GenerateArabicTreatmentTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("{\"Treatment\":\"New\"}");
        var result = await _service.ForceRefreshConditionAsync("Rose", "Fungi");
        Assert.Equal("New", result.Treatment);
    }
}
