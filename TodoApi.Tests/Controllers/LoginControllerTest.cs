using Microsoft.AspNetCore.Identity;
using Moq;
using TodoApi.Models;
using TodoApi.Interfaces;
using TodoApi.ResourceModels;
using System.Text.Json;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Utilities;

namespace TodoApi.Tests.Controllers;


public class LoginControllerTests : IClassFixture<WebTestFixture>
{
    private readonly WebTestFixture _fixture;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IAuthTokenManagerService> _tokenCreationServiceMock;

    public LoginControllerTests(WebTestFixture fixture)
    {
        _fixture = fixture;
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
        _tokenCreationServiceMock = new Mock<IAuthTokenManagerService>();
    }

    [Fact]
    public async Task Login_WithNonExistingUser_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginUserRequest { Email = "nonexistinguser@example.com", Password = "password" };
        _userManagerMock.Setup(m => m.FindByNameAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);

        var httpClient = _fixture.WithServices(
                services => services.AddScoped<UserManager<ApplicationUser>>(serviceProvider => _userManagerMock.Object)
            )
            .CreateClient();

        // Act
        var response = await httpClient.PostAsync("/Auth/Login", request.ToFormUrlEncodedContent());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Equal("Wrong credentials", responseContent);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginUserRequest { Email = "existinguser@example.com", Password = "wrongpassword" };
        var existingUser = new ApplicationUser { UserName = request.Email };
        _userManagerMock.Setup(m => m.FindByNameAsync(request.Email)).ReturnsAsync(existingUser);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(existingUser, request.Password)).ReturnsAsync(false);

        var httpClient = _fixture.WithServices(
                services => services.AddScoped<UserManager<ApplicationUser>>(serviceProvider => _userManagerMock.Object)
            )
            .CreateClient();

        // Act
        var response = await httpClient.PostAsync("/Auth/Login", request.ToFormUrlEncodedContent());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Equal("Wrong credentials", responseContent);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        var request = new LoginUserRequest { Email = "existinguser@example.com", Password = "correctpassword" };
        var existingUser = new ApplicationUser { UserName = request.Email };
        _userManagerMock.Setup(m => m.FindByNameAsync(request.Email)).ReturnsAsync(existingUser);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(existingUser, request.Password)).ReturnsAsync(true);
        var expectedToken = new LoginUserTokenResponse { AccessToken = "some-token" };
        _tokenCreationServiceMock.Setup(m => m.CreateToken(existingUser)).Returns(expectedToken);

        var httpClient = _fixture.WithServices(services => {
                services.AddScoped<UserManager<ApplicationUser>>(serviceProvider => _userManagerMock.Object);
                services.AddScoped<IAuthTokenManagerService>(serviceProvider => _tokenCreationServiceMock.Object);
            })
            .CreateClient();

        // Act
        var response = await httpClient.PostAsync("/Auth/Login", request.ToFormUrlEncodedContent());

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<LoginUserTokenResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(responseObject);
        Assert.Equal(expectedToken.AccessToken, responseObject.AccessToken);
    }
}
