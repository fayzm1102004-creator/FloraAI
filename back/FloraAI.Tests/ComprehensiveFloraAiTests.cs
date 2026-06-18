using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;
using Xunit;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;



using FloraAI.API.Controllers;
using FloraAI.API.Data;
using FloraAI.API.DTOs.User;
using FloraAI.API.DTOs.Diagnosis;
using FloraAI.API.Models.Entities;
using FloraAI.API.Services;
using FloraAI.API.Services.Interfaces;

namespace FloraAI.Tests;

/*
|--------------------------------------------------------------------------
| COMPREHENSIVE FLORA AI TEST SUITE
|--------------------------------------------------------------------------
| This file consolidates core Unit and Integration tests for Auth and Diagnosis.
| Targeted Code Coverage: 90%+
| Targeted Success Rate: 100% (Happy Paths)
*/

#region 1. UNIT TESTS (Logic & Validations)

public class AuthUnitTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly AuthController _controller;

    public AuthUnitTests()
    {
        _userServiceMock = new Mock<IUserService>();
        var loggerMock = new Mock<ILogger<AuthController>>();
        var blacklistMock = new Mock<ITokenBlacklistService>();
        _controller = new AuthController(_userServiceMock.Object, loggerMock.Object, blacklistMock.Object);
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldReturnOk()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "user@test.com", Password = "Password123!" };
        _userServiceMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AuthResponseDto 
            { 
                Token = "MockToken", 
                RefreshToken = "MockRefresh",
                User = new UserResponseDto { Email = loginDto.Email, FullName = "Test" } 
            });


        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponseDto>().Subject;
        response.Token.Should().Be("MockToken");
    }

    [Fact]
    public async Task Register_ExistingEmail_ShouldReturnConflict()
    {
        // Arrange
        var regDto = new UserRegisterDto { Email = "exists@test.com", Password = "P", FullName = "N" };
        _userServiceMock.Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Email exists"));

        // Act
        var result = await _controller.Register(regDto, new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>().Object);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }
}

public class DiagnosisUnitTests
{
    private readonly Mock<IConditionService> _conditionMock;
    private readonly DiagnosisController _controller;

    public DiagnosisUnitTests()
    {
        _conditionMock = new Mock<IConditionService>();
        var diagnosisServiceMock = new Mock<IDiagnosisService>();
        var userPlantServiceMock = new Mock<IUserPlantService>();
        var loggerMock = new Mock<ILogger<DiagnosisController>>();
        
        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(m => m.Map<DiagnosisScanResponseDto>(It.IsAny<ConditionsDictionary>()))
            .Returns((ConditionsDictionary src) => new DiagnosisScanResponseDto 
            { 
                PlantType = src.PlantType, 
                ConditionName = src.ConditionName,
                CareAdvice = new CareAdviceDto { Watering = src.WateringAdvice ?? "" }
            });

        // Database Setup

        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        var dbContext = new ApplicationDbContext(options);

        _controller = new DiagnosisController(
            diagnosisServiceMock.Object, 
            _conditionMock.Object, 
            userPlantServiceMock.Object, 
            dbContext, 
            loggerMock.Object, 
            mapperMock.Object);

    }

    [Fact]
    public async Task Scan_ValidRequest_ShouldReturnDiagnosis()
    {
        // Arrange
        var request = new DiagnosisScanRequestDto { PlantType = "Rose", ConditionName = "Healthy" };
        _conditionMock.Setup(s => s.GetOrFetchConditionAsync("Rose", "Healthy", null))
            .ReturnsAsync(new ConditionsDictionary { Id = 1, PlantType = "Rose", ConditionName = "Healthy", WateringAdvice = "Regular" });

        // Act
        var result = await _controller.Scan(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<DiagnosisScanResponseDto>().Subject;
        response.PlantType.Should().Be("Rose");
        response.CareAdvice.Watering.Should().Be("Regular");
    }
}

#endregion

#region 2. FUZZ & BOUNDARY TESTS

public class SecurityAndFuzzTests
{
    [Theory]
    [InlineData("🌵", "Infected")]
    [InlineData("VeryLongPlantNameThatMightCauseBufferIssuesIfManagedPoorlyByTheDatabaseOrApiContract", "Healthy")]
    [InlineData("'; DROP TABLE Users; --", "Malicious")]
    public async Task Diagnosis_ShouldHandleExtremeInputs_WithoutCrashing(string plant, string condition)
    {
        // Arrange
        var conditionServiceMock = new Mock<IConditionService>();
        conditionServiceMock.Setup(s => s.GetOrFetchConditionAsync(It.IsAny<string>(), It.IsAny<string>(), null))
            .ReturnsAsync(new ConditionsDictionary { PlantType = plant, ConditionName = condition });

        var diagnosisService = new DiagnosisService(conditionServiceMock.Object, null!, new Mock<ILogger<DiagnosisService>>().Object);

        // Act
        var result = await diagnosisService.ScanPlantAsync(plant, condition);

        // Assert
        result.Should().NotBeNull();
        result.PlantType.Should().Be(plant);
    }
}

#endregion

#region 3. INTEGRATION TESTS (E2E & Security)

public class FloraAiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FloraAiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real DB and its options
                var descriptors = services.Where(d => d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true).ToList();
                foreach (var d in descriptors) services.Remove(d);

                // Add In-Memory DB
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                });

            });
        });
    }

    [Fact]
    public async Task Full_Workflow_Registration_To_Diagnosis()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = $"e2e_{Guid.NewGuid()}@flora.ai";
        
        // 1. Register
        var regRequest = new UserRegisterDto { FullName = "QA", Email = email, Password = "SecurePass123!" };
        var regResponse = await client.PostAsJsonAsync("/api/Auth/register", regRequest);
        
        regResponse.StatusCode.Should().Be(HttpStatusCode.Created, $"Registration failed with: {await regResponse.Content.ReadAsStringAsync()}");
        var authData = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        // 2. Scan (Authenticated)
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authData!.Token);
        var scanResponse = await client.PostAsJsonAsync("/api/Diagnosis/scan", new DiagnosisScanRequestDto { PlantType = "Monstera", ConditionName = "Healthy" });
        
        // Assert
        scanResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/api/UserPlants/user/1", "GET")]
    [InlineData("/api/Auth/logout", "POST")]
    public async Task ProtectedEndpoints_MissingToken_ShouldReturn401(string url, string method)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = method == "POST" 
            ? await client.PostAsync(url, null) 
            : await client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"Endpoint {url} with method {method} should be protected. Actual status: {response.StatusCode}");
    }
}

#endregion

