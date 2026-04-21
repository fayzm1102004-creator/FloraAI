using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FloraAI.API.Controllers;
using FloraAI.API.Services.Interfaces;
using FloraAI.API.DTOs.Conditions;
using FloraAI.API.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace FloraAI.Tests;

public class ConditionsControllerTests
{
    private readonly ConditionsController _controller;
    private readonly Mock<IConditionService> _serviceMock;
    private readonly Mock<ILogger<ConditionsController>> _loggerMock;
    private readonly AutoMapper.IMapper _mapper;

    public ConditionsControllerTests()
    {
        _serviceMock = new Mock<IConditionService>();
        _loggerMock = new Mock<ILogger<ConditionsController>>();

        var config = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<FloraAI.API.Mappings.MappingProfile>(), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _controller = new ConditionsController(_serviceMock.Object, _loggerMock.Object, _mapper);
    }

    #region GetPlantConditions Tests

    [Fact]
    public async Task GetPlantConditions_ValidPlantType_ReturnsOk()
    {
        // Arrange
        var plantType = "Tomato";
        var conditions = new List<ConditionsDictionary>
        {
            new ConditionsDictionary { Id = 1, PlantType = plantType, ConditionName = "Blight", CareInstructions = "Test" }
        };
        _serviceMock.Setup(s => s.GetConditionsByPlantTypeAsync(plantType))
            .ReturnsAsync(conditions);

        // Act
        var result = await _controller.GetPlantConditions(plantType);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<ConditionResponseDto>>(okResult.Value);
        Assert.Single(response);
    }

    [Fact]
    public async Task GetPlantConditions_EmptyPlantType_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetPlantConditions("");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetPlantConditions_NoConditionsFound_ReturnsOkEmptyList()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetConditionsByPlantTypeAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<ConditionsDictionary>());

        // Act
        var result = await _controller.GetPlantConditions("Unknown");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<ConditionResponseDto>>(okResult.Value);
        Assert.Empty(response);
    }

    [Fact]
    public async Task GetPlantConditions_ExceptionThrown_Returns500()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetConditionsByPlantTypeAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetPlantConditions("Tomato");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    #endregion

    #region GetCondition Tests

    [Fact]
    public async Task GetCondition_ExistingCondition_ReturnsOk()
    {
        // Arrange
        var plantType = "Tomato";
        var conditionName = "Blight";
        var condition = new ConditionsDictionary { Id = 1, PlantType = plantType, ConditionName = conditionName, CareInstructions = "Test" };
        
        _serviceMock.Setup(s => s.GetConditionAsync(plantType, conditionName))
            .ReturnsAsync(condition);

        // Act
        var result = await _controller.GetCondition(plantType, conditionName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ConditionResponseDto>(okResult.Value);
        Assert.Equal(conditionName, response.ConditionName);
    }

    [Fact]
    public async Task GetCondition_NonExistent_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetConditionAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((ConditionsDictionary)null);

        // Act
        var result = await _controller.GetCondition("Tomato", "GhostCondition");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetCondition_MissingInputs_ReturnsBadRequest()
    {
        // Act & Assert
        Assert.IsType<BadRequestObjectResult>(await _controller.GetCondition("", "Blight"));
        Assert.IsType<BadRequestObjectResult>(await _controller.GetCondition("Tomato", ""));
    }

    #endregion

    #region GetAllConditions Tests

    [Fact]
    public async Task GetAllConditions_ReturnsAllData()
    {
        // Arrange
        var conditions = new List<ConditionsDictionary>
        {
            new ConditionsDictionary { Id = 1, PlantType = "A", ConditionName = "C1", CareInstructions = "T" },
            new ConditionsDictionary { Id = 2, PlantType = "B", ConditionName = "C2", CareInstructions = "T" }
        };
        _serviceMock.Setup(s => s.GetAllConditionsAsync())
            .ReturnsAsync(conditions);

        // Act
        var result = await _controller.GetAllConditions();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<ConditionResponseDto>>(okResult.Value);
        Assert.Equal(2, response.Count());
    }

    [Fact]
    public async Task GetAllConditions_ServiceReturnsNull_ReturnsEmptyList()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllConditionsAsync())
            .ReturnsAsync((List<ConditionsDictionary>)null);

        // Act
        var result = await _controller.GetAllConditions();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<ConditionResponseDto>>(okResult.Value);
        Assert.Empty(response);
    }

    #endregion
}
