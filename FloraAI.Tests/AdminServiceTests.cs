using FloraAI.API.Data;
using FloraAI.API.Models.Entities;
using FloraAI.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System;
using System.Linq;

namespace FloraAI.Tests;

public class AdminServiceTests
{
    private readonly AdminService _service;
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<AdminService>> _loggerMock;

    public AdminServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<AdminService>>();
        _service = new AdminService(_dbContext, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_CalculatesCorrectAggregates()
    {
        // Arrange
        _dbContext.Users.Add(new User { FullName = "U1", Email = "e1@t.com", PasswordHash = "h" });
        
        var c1 = new ConditionsDictionary { PlantType = "Rose", ConditionName = "Fungi" };
        var c2 = new ConditionsDictionary { PlantType = "Tomato", ConditionName = "Pests" };
        _dbContext.ConditionsDictionary.AddRange(c1, c2);
        await _dbContext.SaveChangesAsync();

        _dbContext.ScanHistories.Add(new ScanHistory { ConditionsDictionaryId = c1.Id, ScanDate = DateTime.UtcNow });
        _dbContext.ScanHistories.Add(new ScanHistory { ConditionsDictionaryId = c1.Id, ScanDate = DateTime.UtcNow });
        _dbContext.ScanHistories.Add(new ScanHistory { ConditionsDictionaryId = c2.Id, ScanDate = DateTime.UtcNow });
        await _dbContext.SaveChangesAsync();

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync((byte[]?)null);

        // Act
        var result = await _service.GetDashboardStatsAsync();

        // Assert
        Assert.Equal(1, result.TotalUsers);
        Assert.Equal(3, result.TotalScans);
        Assert.Contains(result.TopPlants, p => p.PlantName == "Rose" && p.Count == 2);
        Assert.Contains(result.CategoryDistribution, c => c.Category == "Fungi" && c.Count == 2);
    }
}
