using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FloraAI.API.Controllers;
using FloraAI.API.Services.Interfaces;
using FloraAI.API.DTOs.Sync;
using FloraAI.API.DTOs.Diagnosis;

namespace FloraAI.Tests;

public class SyncControllerTests
{
    private readonly SyncController _controller;
    private readonly Mock<ISyncService> _serviceMock;
    private readonly Mock<ILogger<SyncController>> _loggerMock;

    public SyncControllerTests()
    {
        _serviceMock = new Mock<ISyncService>();
        _loggerMock = new Mock<ILogger<SyncController>>();
        _controller = new SyncController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Pull_ValidDate_ReturnsOk()
    {
        // Arrange
        _serviceMock.Setup(s => s.PullConditionsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new SyncPullResponseDto { NewConditions = new List<SyncConditionDto>() });

        // Act
        var result = await _controller.Pull(DateTime.UtcNow.AddDays(-1));

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Push_ValidList_ReturnsOk()
    {
        // Arrange
        var request = new SyncPushRequestDto 
        { 
            PendingScans = new List<PendingScanDto> 
            { 
                new PendingScanDto { PlantType = "T", ConditionName = "C" } 
            } 
        };
        _serviceMock.Setup(s => s.PushPendingScansAsync(It.IsAny<List<DiagnosisScanRequestDto>>()))
            .ReturnsAsync(new SyncPushResponseDto { DiagnosisResults = new List<SyncDiagnosisResultDto>() });

        // Act
        var result = await _controller.Push(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Status_ReturnsOk()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetSyncStatusAsync())
            .ReturnsAsync(new { count = 1 });

        // Act
        var result = await _controller.GetSyncStatus();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
