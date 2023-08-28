using System.Net;
using System.Text.Json;
using TodoApi.Models;
using TodoApi.ResourceModels;

namespace TodoApi.Tests.Controllers;

public class MeControllerTest : IClassFixture<WebTestFixture>
{
    private readonly HttpClient client;
    private readonly ApplicationUser user;
    private readonly string accessTokenHeader;

    public MeControllerTest(WebTestFixture fixture)
    {
        client = fixture.CreateClient();
        user = fixture.User;
        accessTokenHeader = $"Bearer {fixture.UserAccessToken}";
    }

    [Fact]
    public async Task Me_Should_ReturnCurrentUser()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/Auth/Me");
        request.Headers.Add("Authorization", accessTokenHeader);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        MeResponse? responseObject = JsonSerializer.Deserialize<MeResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(responseObject);
        Assert.Equal(user.Id, responseObject.Id);
        Assert.Equal(user.Email, responseObject.Email);
        Assert.Equal(user.RefreshTokenExpiryTime, responseObject.RefreshTokenExpiryTime);
    }

    [Fact]
    public async Task Me_Should_ReturnUnauthorized_WhenNoTokenProvided()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/Auth/Me");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
