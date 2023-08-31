using System.Net;
using System.Text.Json;
using TodoApi.Models;
using TodoApi.ResourceModels;

namespace TodoApi.Tests.Controllers;

public class MeControllerTest : IClassFixture<WebTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApplicationUser _user;
    private readonly string _accessTokenHeader;

    public MeControllerTest(WebTestFixture fixture)
    {
        _client = fixture.CreateClient();
        _user = fixture.User;
        _accessTokenHeader = $"Bearer {fixture.UserAccessToken}";
    }

    [Fact]
    public async Task Me_Should_ReturnCurrentUser()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/Auth/Me");
        request.Headers.Add("Authorization", _accessTokenHeader);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        MeResponse? responseObject = JsonSerializer.Deserialize<MeResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(responseObject);
        Assert.Equal(_user.Id, responseObject.Id);
        Assert.Equal(_user.Email, responseObject.Email);
        Assert.Equal(_user.RefreshTokenExpiryTime, responseObject.RefreshTokenExpiryTime);
    }

    [Fact]
    public async Task Me_Should_ReturnUnauthorized_WhenNoTokenProvided()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/Auth/Me");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
