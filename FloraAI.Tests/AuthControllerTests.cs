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
        var user = new UserResponseDto { Id = 1, FullName = "John Doe", Email = "john@example.com" };
        _userServiceMock.Setup(s => s.RegisterAsync(registerDto.FullName, registerDto.Email, registerDto.Password))
                        .ReturnsAsync(user);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var responseDto = Assert.IsType<UserResponseDto>(createdResult.Value);
        Assert.Equal(user.Id, responseDto.Id);
        Assert.Equal(user.FullName, responseDto.FullName);
        Assert.Equal(user.Email, responseDto.Email);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "john@example.com", Password = "SecurePass123!" };
        var user = new UserResponseDto { Id = 1, FullName = "John Doe", Email = "john@example.com" };
        _userServiceMock.Setup(s => s.LoginAsync(loginDto.Email, loginDto.Password))
                        .ReturnsAsync(user);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDto = Assert.IsType<UserResponseDto>(okResult.Value);
        Assert.Equal(user.Id, responseDto.Id);
        Assert.Equal(user.FullName, responseDto.FullName);
        Assert.Equal(user.Email, responseDto.Email);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "john@example.com", Password = "WrongPass" };
        _userServiceMock.Setup(s => s.LoginAsync(loginDto.Email, loginDto.Password))
                        .ReturnsAsync((UserResponseDto?)null);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
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
