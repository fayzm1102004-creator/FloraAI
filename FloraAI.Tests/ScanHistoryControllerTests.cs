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
using FloraAI.API.DTOs.ScanHistory;

namespace FloraAI.Tests;

public class ScanHistoryControllerTests
{
    private readonly ScanHistoryController _controller;
    private readonly Mock<IUserPlantService> _userPlantServiceMock;
    private readonly Mock<ILogger<ScanHistoryController>> _loggerMock;

    public ScanHistoryControllerTests()
    {
        _userPlantServiceMock = new Mock<IUserPlantService>();
        _loggerMock = new Mock<ILogger<ScanHistoryController>>();
        _controller = new ScanHistoryController(_userPlantServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetScanHistoryByPlant_Valid_ReturnsOk()
    {
        // Arrange
        _userPlantServiceMock.Setup(s => s.GetScanHistoryAsync(1))
            .ReturnsAsync(new List<ScanHistoryDto>());

        // Act
        var result = await _controller.GetScanHistoryByPlant(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetScanHistoryByUser_Valid_ReturnsOk()
    {
        // Arrange
        _userPlantServiceMock.Setup(s => s.GetUserScanHistoryAsync(1))
            .ReturnsAsync(new List<ScanHistoryDto>());

        // Act
        var result = await _controller.GetScanHistoryByUser(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetLatestScans_Valid_ReturnsOk()
    {
        // Arrange
        _userPlantServiceMock.Setup(s => s.GetLatestScansAsync(1))
            .ReturnsAsync(new List<ScanHistoryDto>());

        // Act
        var result = await _controller.GetLatestScans(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
