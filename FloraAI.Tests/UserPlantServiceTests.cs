using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FloraAI.API.Data;
using FloraAI.API.Models.Entities;
using FloraAI.API.Services;
using FloraAI.API.DTOs.UserPlant;

namespace FloraAI.Tests;

public class UserPlantServiceTests
{
    private readonly UserPlantService _service;
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<UserPlantService>> _loggerMock;

    public UserPlantServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        // Seed user
        _dbContext.Users.Add(new User { Id = 1, FullName = "Test User", Email = "test@example.com", PasswordHash = "hash" });
        _dbContext.SaveChanges();

        _loggerMock = new Mock<ILogger<UserPlantService>>();
        
        var config = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<FloraAI.API.Mappings.MappingProfile>(), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        var mapper = config.CreateMapper();

        _service = new UserPlantService(_dbContext, _loggerMock.Object, mapper);
    }

    [Fact]
    public async Task SaveUserPlantAsync_ValidUser_SavesSuccessfully()
    {
        // Arrange
        var dto = new SaveUserPlantDto 
        { 
            Nickname = "Greenie", 
            PlantType = "Fern", 
            CurrentStatus = "Healthy",
            SavedCareInstructions = "Water daily"
        };

        // Act
        var result = await _service.SaveUserPlantAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Greenie", result.Nickname);
        var plantsInDb = await _dbContext.UserPlants.CountAsync();
        Assert.Equal(1, plantsInDb);
    }

    [Fact]
    public async Task SaveUserPlantAsync_InvalidUser_ThrowsException()
    {
        // Arrange
        var dto = new SaveUserPlantDto 
        { 
            Nickname = "Ghost", 
            PlantType = "Air", 
            CurrentStatus = "None",
            SavedCareInstructions = "None"
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _service.SaveUserPlantAsync(999, dto));
    }

    [Fact]
    public async Task GetUserPlantsAsync_ReturnsList()
    {
        // Arrange
        _dbContext.UserPlants.Add(new UserPlant { UserId = 1, Nickname = "P1", PlantType = "T", CurrentStatus = "S" });
        _dbContext.UserPlants.Add(new UserPlant { UserId = 1, Nickname = "P2", PlantType = "T", CurrentStatus = "S" });
        await _dbContext.SaveChangesAsync();

        // Act
        var results = await _service.GetUserPlantsAsync(1);

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetUserPlantByIdAsync_Existing_ReturnsPlant()
    {
        // Arrange
        var p = new UserPlant { Id = 10, UserId = 1, Nickname = "FindMe", PlantType = "T", CurrentStatus = "S" };
        _dbContext.UserPlants.Add(p);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetUserPlantByIdAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("FindMe", result.Nickname);
    }

    [Fact]
    public async Task UpdatePlantStatusAsync_Existing_UpdatesStatus()
    {
        // Arrange
        var p = new UserPlant { Id = 5, UserId = 1, Nickname = "Upd", PlantType = "T", CurrentStatus = "Old" };
        _dbContext.UserPlants.Add(p);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.UpdatePlantStatusAsync(5, "New");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New", result.CurrentStatus);
        var dbPlant = await _dbContext.UserPlants.FindAsync(5);
        Assert.Equal("New", dbPlant.CurrentStatus);
    }

    [Fact]
    public async Task DeleteUserPlantAsync_Existing_ReturnsTrue()
    {
        // Arrange
        var p = new UserPlant { Id = 20, UserId = 1, Nickname = "Bye", PlantType = "T", CurrentStatus = "S" };
        _dbContext.UserPlants.Add(p);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.DeleteUserPlantAsync(20);

        // Assert
        Assert.True(result);
        var dbPlant = await _dbContext.UserPlants.FindAsync(20);
        Assert.Null(dbPlant);
    }
}
