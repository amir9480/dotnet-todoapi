using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TodoApi.Models;
using TodoApi.ResourceModels;
using TodoApi.Utilities;

namespace TodoApi.Tests.Controllers;

public class RegisterControllerTest : IClassFixture<WebTestFixture>
{
    private readonly HttpClient _client;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

    public RegisterControllerTest(WebTestFixture fixture)
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null
        );
        _client = fixture.WithServices(
                services => services.AddScoped<UserManager<ApplicationUser>>(serviceProvider => _userManagerMock.Object)
            )
            .CreateClient();
    }

    [Fact]
    public async Task Register_WithValidModel_ReturnsCreatedResult()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Email = "test@example.com",
            Password = "password"
        };

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var response = await _client.PostAsync("/Auth/Register", request.ToFormUrlEncodedContent());
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RegisterUserRequest>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Empty(result.Password);
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
    }
}
