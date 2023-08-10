using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Moq;
using TodoApi.Controllers;
using TodoApi.Models;
using TodoApi.Interfaces;
using TodoApi.ResourceModels;

namespace TodoApi.Tests.Controllers;

public class LoginControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> userManagerMock;
    private readonly Mock<IAuthTokenManagerService> tokenCreationServiceMock;
    private readonly LoginController loginController;

    public LoginControllerTests()
    {
        userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null
        );
        tokenCreationServiceMock = new Mock<IAuthTokenManagerService>();
        loginController = new LoginController(userManagerMock.Object, tokenCreationServiceMock.Object);
    }

    [Fact]
    public async Task Login_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        loginController.ModelState.AddModelError("Email", "Email is required");

        // Act
        IActionResult result = await loginController.Login(new LoginUserRequest());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
        Assert.Equal("Wrong credentials", badRequestResult.Value);
    }


    [Fact]
    public async Task Login_WithNonExistingUser_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginUserRequest { Email = "nonexistinguser@example.com", Password = "password" };
        userManagerMock.Setup(m => m.FindByNameAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await loginController.Login(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.Equal("Wrong credentials", badRequestResult.Value);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginUserRequest { Email = "existinguser@example.com", Password = "wrongpassword" };
        var existingUser = new ApplicationUser { UserName = request.Email };
        userManagerMock.Setup(m => m.FindByNameAsync(request.Email)).ReturnsAsync(existingUser);
        userManagerMock.Setup(m => m.CheckPasswordAsync(existingUser, request.Password)).ReturnsAsync(false);

        // Act
        var result = await loginController.Login(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.Equal("Wrong credentials", badRequestResult.Value);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        var request = new LoginUserRequest { Email = "existinguser@example.com", Password = "correctpassword" };
        var existingUser = new ApplicationUser { UserName = request.Email };
        userManagerMock.Setup(m => m.FindByNameAsync(request.Email)).ReturnsAsync(existingUser);
        userManagerMock.Setup(m => m.CheckPasswordAsync(existingUser, request.Password)).ReturnsAsync(true);
        var expectedToken = new LoginUserTokenResponse { AccessToken = "some-token" };
        tokenCreationServiceMock.Setup(m => m.CreateToken(existingUser)).Returns(expectedToken);

        // Act
        var result = await loginController.Login(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.Equal(expectedToken, okResult.Value);
    }
}
