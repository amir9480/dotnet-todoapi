using System.Net;
using TodoApi.Models;
using TodoApi.ResourceModels;
using System.Net.Http.Json;
using TodoApi.Utilities;

namespace TodoApi.Tests.Controllers;

public class TodoControllerTest : IClassFixture<WebTestFixture>
{
    private readonly HttpClient httpClient;

    public TodoControllerTest(WebTestFixture factory)
    {
        httpClient = factory.CreateClientWithAuthHeader();
    }

    [Fact]
    public async Task Create_WithValidModel_ShouldReturnCreatedResult()
    {
        // Arrange
        var request = new NewTodoRequest { Text = "Sample todo item" };

        // Act
        var response = await httpClient.PostAsync("/Todo", request.ToFormUrlEncodedContent());

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdItem = await response.Content.ReadFromJsonAsync<TodoItem>();
        Assert.NotNull(createdItem);
        Assert.Equal(request.Text, createdItem.Text);
    }

    [Fact]
    public async Task Create_WithInvalidModel_ShouldReturnBadRequestResult()
    {
        // Arrange
        var request = new NewTodoRequest { Text = "" };

        // Act
        var response = await httpClient.PostAsync("/Todo", request.ToFormUrlEncodedContent());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
