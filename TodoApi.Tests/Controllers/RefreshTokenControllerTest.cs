using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TodoApi.Interfaces;
using TodoApi.Models;
using TodoApi.ResourceModels;
using TodoApi.Utilities;

namespace TodoApi.Tests.Controllers;

public class RefreshTokenControllerTest : IClassFixture<WebTestFixture>
{
    private const string VALID_REFRESH_TOKEN = "valid_refresh_token";
    private const string INVALID_REFRESH_TOKEN = "invalid_refresh_token";
    private const string NEW_ACCESS_TOKEN = "valid_access_token";
    private const string NEW_RESET_TOKEN = "new_reset_token";

    private readonly HttpClient _client;
    private readonly Mock<IAuthTokenManagerService> _tokenServiceMock;
    private string accessTokenHeader;

    public RefreshTokenControllerTest(WebTestFixture fixture)
    {
        _tokenServiceMock = new Mock<IAuthTokenManagerService>();
        _client = fixture.WithServices(
                services => services.AddScoped<IAuthTokenManagerService>(serviceProvider => _tokenServiceMock.Object)
            )
            .CreateClient();
        accessTokenHeader = $"Bearer {fixture.UserAccessToken}";
        _client.DefaultRequestHeaders.Add("Authorization", accessTokenHeader);
    }

    [Fact]
    public async Task RefreshToken_WithValidTokenAndMatchingRefreshToken_ReturnsOkResult()
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

        _tokenServiceMock.Setup(t => t.FindUserByToken(accessTokenHeader)).Returns(user);
        _tokenServiceMock.Setup(t => t.CreateToken(user)).Returns(new LoginUserTokenResponse
        {
            AccessToken = NEW_ACCESS_TOKEN,
            AccessTokenExpiration = DateTime.UtcNow.AddDays(1),
            RefreshToken = NEW_RESET_TOKEN,
            RefreshTokenExpiration = DateTime.UtcNow.AddDays(1),
        });

        // Act
        var response = await _client.PostAsync("/Auth/RefreshToken", request.ToFormUrlEncodedContent());
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LoginUserTokenResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(NEW_ACCESS_TOKEN, result.AccessToken);
        Assert.Equal(NEW_RESET_TOKEN, result.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithValidTokenAndMismatchingRefreshToken_ReturnsUnauthorizedResult()
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

        _tokenServiceMock.Setup(t => t.FindUserByToken(accessTokenHeader)).Returns(user);

        // Act
        var response = await _client.PostAsync("/Auth/RefreshToken", request.ToFormUrlEncodedContent());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorizedResult()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = VALID_REFRESH_TOKEN
        };

        _tokenServiceMock.Setup(t => t.FindUserByToken(accessTokenHeader)).Returns((ApplicationUser?)null);

        // Act
        var response = await _client.PostAsync("/Auth/RefreshToken", request.ToFormUrlEncodedContent());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
