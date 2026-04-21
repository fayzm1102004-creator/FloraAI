using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FloraAI.API.Controllers;
using FloraAI.API.Services.Interfaces;
using FloraAI.API.DTOs.User;

namespace FloraAI.Tests;

public class AuthControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_userServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var registerDto = new UserRegisterDto
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Password = "SecurePass123!"
        };
        var authResponse = new AuthResponseDto 
        { 
            Token = "Access", 
            RefreshToken = "Refresh", 
            User = new UserResponseDto { Id = 1, FullName = "John Doe", Email = "john@example.com" } 
        };
        _userServiceMock.Setup(s => s.RegisterAsync(registerDto.FullName, registerDto.Email, registerDto.Password))
                        .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var responseDto = Assert.IsType<AuthResponseDto>(createdResult.Value);
        Assert.Equal("Access", responseDto.Token);
        Assert.Equal(1, responseDto.User.Id);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "john@example.com", Password = "SecurePass123!" };
        var authResponse = new AuthResponseDto 
        { 
            Token = "Access", 
            RefreshToken = "Refresh", 
            User = new UserResponseDto { Id = 1, FullName = "John Doe", Email = "john@example.com" } 
        };
        _userServiceMock.Setup(s => s.LoginAsync(loginDto.Email, loginDto.Password))
                        .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDto = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("Access", responseDto.Token);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "john@example.com", Password = "WrongPass" };
        _userServiceMock.Setup(s => s.LoginAsync(loginDto.Email, loginDto.Password))
                        .ReturnsAsync((AuthResponseDto?)null);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task RefreshToken_ValidTokens_ReturnsOk()
    {
        // Arrange
        var request = new TokenRequestDto { Token = "old", RefreshToken = "valid" };
        var authResponse = new AuthResponseDto { Token = "new", RefreshToken = "new_refresh", User = new UserResponseDto { Id = 1, FullName = "N", Email = "E" } };
        _userServiceMock.Setup(s => s.RefreshTokenAsync("old", "valid"))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("new", response.Token);
    }
    [Fact]
    public async Task Register_UserExists_ReturnsConflict()
    {
        // Arrange
        var registerDto = new UserRegisterDto { FullName = "N", Email = "exists@example.com", Password = "P" };
        _userServiceMock.Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .ThrowsAsync(new InvalidOperationException("User already exists"));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Login_Exception_Returns500()
    {
        // Arrange
        _userServiceMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .ThrowsAsync(new System.Exception("Critical error"));

        // Act
        var result = await _controller.Login(new UserLoginDto { Email = "e", Password = "p" });

        // Assert
        var result500 = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, result500.StatusCode);
    }
    [Fact]
    public async Task Register_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Email", "Required");
        var request = new UserRegisterDto { FullName = "", Email = "", Password = "" };

        // Act
        var result = await _controller.Register(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Password", "Required");
        var request = new UserLoginDto { Email = "test@test.com", Password = "" };

        // Act
        var result = await _controller.Login(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_GeneralException_Returns500()
    {
        // Arrange
        var request = new UserRegisterDto { FullName = "Test", Email = "error@e.com", Password = "123" };
        _userServiceMock.Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new System.Exception("Critical DB Failure"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
