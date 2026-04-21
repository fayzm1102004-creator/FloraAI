using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FloraAI.API.Controllers;
using FloraAI.API.Services.Interfaces;
using FloraAI.API.DTOs.Diagnosis;
using FloraAI.API.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace FloraAI.Tests;

public class DiagnosisControllerTests
{
    private readonly DiagnosisController _controller;
    private readonly Mock<IDiagnosisService> _diagnosisServiceMock;
    private readonly Mock<IConditionService> _conditionServiceMock;
    private readonly Mock<ILogger<DiagnosisController>> _loggerMock;
    private readonly AutoMapper.IMapper _mapper;

    public DiagnosisControllerTests()
    {
        _diagnosisServiceMock = new Mock<IDiagnosisService>();
        _conditionServiceMock = new Mock<IConditionService>();
        _loggerMock = new Mock<ILogger<DiagnosisController>>();

        var config = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<FloraAI.API.Mappings.MappingProfile>(), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _controller = new DiagnosisController(_diagnosisServiceMock.Object, _conditionServiceMock.Object, _loggerMock.Object, _mapper);
    }

    [Fact]
    public async Task Scan_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new DiagnosisScanRequestDto { PlantType = "Tomato", ConditionName = "Blight" };
        var condition = new ConditionsDictionary { PlantType = "Tomato", ConditionName = "Blight", CareInstructions = "Test" };
        
        _conditionServiceMock.Setup(s => s.GetOrFetchConditionAsync("Tomato", "Blight", null))
            .ReturnsAsync(condition);

        // Act
        var result = await _controller.Scan(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DiagnosisScanResponseDto>(okResult.Value);
        Assert.Equal("Tomato", response.PlantType);
    }

    [Fact]
    public async Task Scan_WithDetectedCategory_ReturnsOk()
    {
        // Arrange
        var request = new DiagnosisScanRequestDto { PlantType = "Tomato", ConditionName = "Spots", DetectedCategory = "فطريات" };
        var condition = new ConditionsDictionary { PlantType = "Tomato", ConditionName = "Spots", CareInstructions = "Fungi Care" };
        
        _conditionServiceMock.Setup(s => s.GetOrFetchConditionAsync("Tomato", "Spots", "فطريات"))
            .ReturnsAsync(condition);

        // Act
        var result = await _controller.Scan(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _conditionServiceMock.Verify(s => s.GetOrFetchConditionAsync("Tomato", "Spots", "فطريات"), Times.Once);
    }

    [Fact]
    public async Task Scan_EmptyPlantType_ReturnsBadRequest()
    {
        // Arrange
        var request = new DiagnosisScanRequestDto { PlantType = "", ConditionName = "Blight" };

        // Act
        var result = await _controller.Scan(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Scan_ServiceReturnsNull_ReturnsInternalError()
    {
        // Arrange
        var request = new DiagnosisScanRequestDto { PlantType = "Tomato", ConditionName = "Blight" };
        _conditionServiceMock.Setup(s => s.GetOrFetchConditionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((ConditionsDictionary?)null);

        // Act
        var result = await _controller.Scan(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
