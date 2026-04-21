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
using FloraAI.API.DTOs.Common;
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
        var pagedResponse = new PagedResponse<PlantLookupDto>(
            new List<PlantLookupDto> { new PlantLookupDto { Id = 1, CommonName = "Tomato" } },
            1, 10, 1
        );
        _serviceMock.Setup(s => s.GetAllPlantsAsync(1, 10)).ReturnsAsync(pagedResponse);

        // Act
        var result = await _controller.GetAllPlants(1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<PagedResponse<PlantLookupDto>>(okResult.Value);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task GetAllPlants_Exception_Returns500()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllPlantsAsync(1, 10)).ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _controller.GetAllPlants(1, 10);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task SearchPlants_ValidQuery_ReturnsOk()
    {
        // Arrange
        var query = "Tom";
        var pagedResponse = new PagedResponse<PlantLookupDto>(
            new List<PlantLookupDto> { new PlantLookupDto { CommonName = "Tomato" } },
            1, 10, 1
        );
        _serviceMock.Setup(s => s.SearchPlantsAsync(query, 1, 10)).ReturnsAsync(pagedResponse);

        // Act
        var result = await _controller.SearchPlants(query, 1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<PagedResponse<PlantLookupDto>>(okResult.Value);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task SearchPlants_EmptyQuery_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SearchPlants("", 1, 10);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SearchPlants_Exception_Returns500()
    {
        // Arrange
        _serviceMock.Setup(s => s.SearchPlantsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception("Search Failed"));

        // Act
        var result = await _controller.SearchPlants("Rose", 1, 10);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
