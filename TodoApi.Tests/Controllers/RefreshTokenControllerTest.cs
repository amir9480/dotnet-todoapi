using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using TodoApi.Controllers;
using TodoApi.Interfaces;
using TodoApi.Models;
using TodoApi.ResourceModels;

namespace TodoApi.Tests.Controllers;

public class RefreshTokenControllerTest
{
    private const string VALID_ACCESS_TOKEN = "valid_access_token";
    private const string VALID_REFRESH_TOKEN = "valid_refresh_token";
    private const string INVALID_REFRESH_TOKEN = "invalid_refresh_token";
    private const string NEW_RESET_TOKEN = "new_reset_token";

    private readonly RefreshTokenController controller;
    private readonly Mock<UserManager<ApplicationUser>> userManagerMock;
    private readonly Mock<IAuthTokenManagerService> tokenServiceMock;

    public RefreshTokenControllerTest()
    {
        userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null
        );
        tokenServiceMock = new Mock<IAuthTokenManagerService>();

        controller = new RefreshTokenController(userManagerMock.Object, tokenServiceMock.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[HeaderNames.Authorization] = new StringValues(VALID_ACCESS_TOKEN);
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public void RefreshToken_WithValidTokenAndMatchingRefreshToken_ReturnsOkResult()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = VALID_REFRESH_TOKEN
        };

        var user = new ApplicationUser
        {
            RefreshToken = VALID_REFRESH_TOKEN
        };

        tokenServiceMock.Setup(t => t.FindUserByToken(VALID_ACCESS_TOKEN)).Returns(user);
        tokenServiceMock.Setup(t => t.CreateToken(user)).Returns(new LoginUserTokenResponse
        {
            AccessToken = VALID_ACCESS_TOKEN,
            AccessTokenExpiration = DateTime.UtcNow.AddDays(1),
            RefreshToken = NEW_RESET_TOKEN,
            RefreshTokenExpiration = DateTime.UtcNow.AddDays(1),
        });

        // Act
        var result = controller.RefreshToken(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginUserTokenResponse>(((OkObjectResult)result).Value);
        Assert.Equal(VALID_ACCESS_TOKEN, response.AccessToken);
        Assert.Equal(NEW_RESET_TOKEN, response.RefreshToken);
    }

    [Fact]
    public void RefreshToken_WithValidTokenAndMismatchingRefreshToken_ReturnsUnauthorizedResult()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = INVALID_REFRESH_TOKEN
        };

        var user = new ApplicationUser
        {
            RefreshToken = VALID_REFRESH_TOKEN
        };

        tokenServiceMock.Setup(t => t.FindUserByToken(VALID_ACCESS_TOKEN)).Returns(user);

        // Act
        var result = controller.RefreshToken(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void RefreshToken_WithInvalidToken_ReturnsUnauthorizedResult()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = VALID_REFRESH_TOKEN
        };

        tokenServiceMock.Setup(t => t.FindUserByToken(VALID_ACCESS_TOKEN)).Returns((ApplicationUser?)null);

        // Act
        var result = controller.RefreshToken(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void RefreshToken_WithInvalidModelState_ReturnsBadRequestResult()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = ""
        };

        controller.ModelState.AddModelError("Error", "Invalid model state");

        // Act
        var result = controller.RefreshToken(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Wrong credentials", badRequestResult.Value);
    }
}
