using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FloraAI.API.Controllers;
using FloraAI.API.Services.Interfaces;
using FloraAI.API.DTOs.UserPlant;
using FloraAI.API.Models.Entities;

namespace FloraAI.Tests;

public class UserPlantsControllerTests
{
    private readonly UserPlantsController _controller;
    private readonly Mock<IUserPlantService> _serviceMock;
    private readonly Mock<ILogger<UserPlantsController>> _loggerMock;

    public UserPlantsControllerTests()
    {
        _serviceMock = new Mock<IUserPlantService>();
        _loggerMock = new Mock<ILogger<UserPlantsController>>();
        _controller = new UserPlantsController(_serviceMock.Object, _loggerMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));
        _controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetUserPlants_ValidId_ReturnsOk()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetUserPlantsAsync(1))
            .ReturnsAsync(new List<UserPlantResponseDto>());

        // Act
        var result = await _controller.GetUserPlants(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SavePlant_ValidDto_ReturnsCreated()
    {
        // Arrange
        var dto = new SaveUserPlantDto 
        { 
            UserId = 1,
            Nickname = "N", 
            PlantType = "T", 
            CurrentStatus = "S",
            SavedCareInstructions = "Care"
        };
        _serviceMock.Setup(s => s.SaveUserPlantAsync(It.IsAny<int>(), It.IsAny<SaveUserPlantDto>()))
            .ReturnsAsync(new UserPlantResponseDto { Id = 1, UserId = 1, Nickname = "N", PlantType = "T", CurrentStatus = "S" });

        // Act
        var result = await _controller.SavePlant(dto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdAtActionResult.StatusCode);
    }

    [Fact]
    public async Task SavePlant_InvalidUserId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SavePlant(new SaveUserPlantDto 
        { 
            UserId = 0, 
            Nickname = "N", 
            PlantType = "T", 
            CurrentStatus = "S", 
            SavedCareInstructions = "C" 
        });

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetUserPlants_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetUserPlantsAsync(99))
            .ReturnsAsync((List<UserPlantResponseDto>?)null);

        // Act
        var result = await _controller.GetUserPlants(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePlantStatus_Exception_Returns500()
    {
        // Arrange
        _serviceMock.Setup(s => s.UpdatePlantStatusAsync(It.IsAny<int>(), It.IsAny<string>())).ThrowsAsync(new Exception("Fail"));

        // Act
        var result = await _controller.UpdatePlantStatus(1, new Dictionary<string, string> { { "status", "Dead" } });

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
