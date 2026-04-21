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

namespace FloraAI.Tests;

public class AdminControllerTests
{
    private readonly AdminController _controller;
    private readonly Mock<IConditionService> _serviceMock;
    private readonly Mock<ILogger<AdminController>> _loggerMock;
    private readonly AutoMapper.IMapper _mapper;

    public AdminControllerTests()
    {
        _serviceMock = new Mock<IConditionService>();
        _loggerMock = new Mock<ILogger<AdminController>>();

        var config = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<FloraAI.API.Mappings.MappingProfile>(), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _controller = new AdminController(_serviceMock.Object, _loggerMock.Object, _mapper);
    }

    [Fact]
    public async Task Refresh_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new DiagnosisScanRequestDto { PlantType = "Tomato", ConditionName = "Blight" };
        var condition = new ConditionsDictionary { Id = 1, PlantType = "Tomato", ConditionName = "Blight" };
        
        _serviceMock.Setup(s => s.ForceRefreshConditionAsync("Tomato", "Blight", null))
            .ReturnsAsync(condition);

        // Act
        var result = await _controller.Refresh(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<FloraAI.API.DTOs.Conditions.ConditionResponseDto>(okResult.Value);
        Assert.Equal(condition.ConditionName, response.ConditionName);
    }

    [Fact]
    public async Task Refresh_InvalidRequest_ReturnsBadRequest()
    {
        // Act & Assert
        Assert.IsType<BadRequestObjectResult>(await _controller.Refresh(null!));
        Assert.IsType<BadRequestObjectResult>(await _controller.Refresh(new DiagnosisScanRequestDto { PlantType = "", ConditionName = "" }));
    }
}
