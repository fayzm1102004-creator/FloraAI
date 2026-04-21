using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FloraAI.API.Controllers;
using FloraAI.API.Services.Interfaces;
using FloraAI.API.DTOs.PlantLookup;
using Microsoft.AspNetCore.Http;

namespace FloraAI.Tests;

public class PlantLookupControllerTests
{
    private readonly PlantLookupController _controller;
    private readonly Mock<IConditionService> _serviceMock;
    private readonly Mock<ILogger<PlantLookupController>> _loggerMock;

    public PlantLookupControllerTests()
    {
        _serviceMock = new Mock<IConditionService>();
        _loggerMock = new Mock<ILogger<PlantLookupController>>();
        _controller = new PlantLookupController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllPlants_ReturnList_ReturnsOk()
    {
        // Arrange
        var plants = new List<PlantLookupDto> 
        { 
            new PlantLookupDto { Id = 1, CommonName = "Tomato" } 
        };
        _serviceMock.Setup(s => s.GetAllPlantsAsync()).ReturnsAsync(plants);

        // Act
        var result = await _controller.GetAllPlants();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<List<PlantLookupDto>>(okResult.Value);
        Assert.Single(response);
    }

    [Fact]
    public async Task GetAllPlants_Exception_Returns500()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllPlantsAsync()).ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _controller.GetAllPlants();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task SearchPlants_ValidQuery_ReturnsOk()
    {
        // Arrange
        var query = "Tom";
        var plants = new List<PlantLookupDto> { new PlantLookupDto { CommonName = "Tomato" } };
        _serviceMock.Setup(s => s.SearchPlantsAsync(query)).ReturnsAsync(plants);

        // Act
        var result = await _controller.SearchPlants(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<List<PlantLookupDto>>(okResult.Value);
        Assert.Single(response);
    }

    [Fact]
    public async Task SearchPlants_EmptyQuery_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SearchPlants("");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SearchPlants_Exception_Returns500()
    {
        // Arrange
        _serviceMock.Setup(s => s.SearchPlantsAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Search Failed"));

        // Act
        var result = await _controller.SearchPlants("Rose");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
