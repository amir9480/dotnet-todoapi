using System.Net;
using TodoApi.Models;
using TodoApi.ResourceModels;
using TodoApi.Utilities;
using TodoApi.Interfaces;
using Moq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace TodoApi.Tests.Controllers;

public class TodoItemControllerTest : IClassFixture<WebTestFixture>
{
    private readonly HttpClient _client;
    private readonly Mock<ITodoService> _todoServiceMock;
    private readonly ApplicationUser _user;
    private readonly string _accessTokenHeader;

    public TodoItemControllerTest(WebTestFixture fixture)
    {
        _todoServiceMock = new Mock<ITodoService>();
        _client = fixture.WithServices(
                services => services.AddScoped<ITodoService>(serviceProvider => _todoServiceMock.Object)
            )
            .CreateClient();
        _user = fixture.User;
        _accessTokenHeader = $"Bearer {fixture.UserAccessToken}";
    }

    [Fact]
    public async Task Create_TodoItem_ReturnsOkStatusCodeAndCreatedTodoItem()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", _accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = _user.Id,
            User = _user,
        };

        var request = new NewTodoRequest
        {
            Text = todoItem.Text
        };

        _todoServiceMock.Setup(todoService => todoService.CreateTodoItem(It.Is<ApplicationUser>(u => u.Id == _user.Id), request.Text))
            .Returns(todoItem);

        // Act
        var response = await _client.PostAsync("/TodoItem", request.ToFormUrlEncodedContent());
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TodoItem>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        _todoServiceMock.Verify(todoService => todoService.CreateTodoItem(It.Is<ApplicationUser>(u => u.Id == _user.Id), request.Text), Times.Once);
    }


    [Fact]
    public async Task Create_TodosAsGuest_ReturnsUnauthorized()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = _user.Id,
            User = _user,
        };
        var request = new NewTodoRequest
        {
            Text = "test"
        };

        _todoServiceMock.Setup(todoService => todoService.CreateTodoItem(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Returns(todoItem);

        // Act
        var response = await _client.PostAsync("/TodoItem", request.ToFormUrlEncodedContent());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _todoServiceMock.Verify(todoService => todoService.CreateTodoItem(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Update_ExistingItem_ReturnsOkStatusCodeAndUpdatedItem()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", _accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = _user.Id,
            User = _user,
        };
        var updatedTodoItem = new TodoItem
        {
            Id = todoItem.Id,
            Text = "New Todo Text",
            UserId = _user.Id,
            User = _user,
        };

        var request = new UpdateTodoRequest
        {
            Text = updatedTodoItem.Text
        };

        _todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        _todoServiceMock.Setup(todoService => todoService.UpdateTodoItemText(todoItem, updatedTodoItem.Text))
            .Returns(updatedTodoItem);

        // Act
        var response = await _client.PutAsync("/TodoItem/" + todoItem.Id, request.ToFormUrlEncodedContent());
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TodoItem>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        _todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        _todoServiceMock.Verify(todoService => todoService.UpdateTodoItemText(todoItem, updatedTodoItem.Text), Times.Once);
    }


    [Fact]
    public async Task Update_TodosAsGuest_ReturnsUnauthorized()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = _user.Id,
            User = _user,
        };
        var request = new UpdateTodoRequest
        {
            Text = "test"
        };

        _todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await _client.PutAsync("/TodoItem/" + todoItem.Id, request.ToFormUrlEncodedContent());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _todoServiceMock.Verify(todoService => todoService.FindTodoItemById(It.IsAny<int>()), Times.Never);
        _todoServiceMock.Verify(todoService => todoService.UpdateTodoItemText(It.IsAny<TodoItem>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Update_TodosOfOtherUsers_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", _accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = "random",
        };
        var request = new UpdateTodoRequest
        {
            Text = "test"
        };

        _todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await _client.PutAsync("/TodoItem/" + todoItem.Id, request.ToFormUrlEncodedContent());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        _todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        _todoServiceMock.Verify(todoService => todoService.UpdateTodoItemText(It.IsAny<TodoItem>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task MarkCompleted_ExistingItem_ReturnsOkStatusCodeAndUpdatedItem()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", _accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            IsCompleted = false,
            Text = "Test",
            UserId = _user.Id,
            User = _user,
        };

        _todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        _todoServiceMock.Setup(todoService => todoService.MarkCompleted(todoItem));

        // Act
        var response = await _client.PatchAsync("/TodoItem/" + todoItem.Id + "/MarkCompleted", null);
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TodoItem>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        _todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        _todoServiceMock.Verify(todoService => todoService.MarkCompleted(todoItem), Times.Once);
    }


    [Fact]
    public async Task MarkCompleted_TodosAsGuest_ReturnsUnauthorized()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = _user.Id,
            User = _user,
        };

        _todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await _client.PatchAsync("/TodoItem/" + todoItem.Id + "/MarkCompleted", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _todoServiceMock.Verify(todoService => todoService.FindTodoItemById(It.IsAny<int>()), Times.Never);
        _todoServiceMock.Verify(todoService => todoService.MarkCompleted(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task MarkCompleted_TodosOfOtherUsers_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", _accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = "random",
        };

        _todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await _client.PatchAsync("/TodoItem/" + todoItem.Id + "/MarkCompleted", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        _todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        _todoServiceMock.Verify(todoService => todoService.MarkCompleted(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task MarkInCompleted_ExistingItem_ReturnsOkStatusCodeAndUpdatedItem()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", _accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            IsCompleted = false,
            Text = "Test",
            UserId = _user.Id,
            User = _user,
        };

        _todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        _todoServiceMock.Setup(todoService => todoService.MarkInCompleted(todoItem));

        // Act
        var response = await _client.PatchAsync("/TodoItem/" + todoItem.Id + "/MarkInCompleted", null);
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TodoItem>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        _todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        _todoServiceMock.Verify(todoService => todoService.MarkInCompleted(todoItem), Times.Once);
    }


    [Fact]
    public async Task MarkInCompleted_TodosAsGuest_ReturnsUnauthorized()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = _user.Id,
            User = _user,
        };

        _todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await _client.PatchAsync("/TodoItem/" + todoItem.Id + "/MarkInCompleted", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _todoServiceMock.Verify(todoService => todoService.FindTodoItemById(It.IsAny<int>()), Times.Never);
        _todoServiceMock.Verify(todoService => todoService.MarkInCompleted(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task MarkInCompleted_TodosOfOtherUsers_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", _accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = "random",
        };

        _todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await _client.PatchAsync("/TodoItem/" + todoItem.Id + "/MarkInCompleted", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        _todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        _todoServiceMock.Verify(todoService => todoService.MarkInCompleted(It.IsAny<TodoItem>()), Times.Never);
    }


    [Fact]
    public async Task Delete_ExistingItem_ReturnsOkNoContent()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", _accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            IsCompleted = false,
            Text = "Test",
            UserId = _user.Id,
            User = _user,
        };

        _todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        _todoServiceMock.Setup(todoService => todoService.DeleteTodoItem(todoItem));

        // Act
        var response = await _client.DeleteAsync("/TodoItem/" + todoItem.Id);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        _todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        _todoServiceMock.Verify(todoService => todoService.DeleteTodoItem(todoItem), Times.Once);
    }


    [Fact]
    public async Task Delete_TodosAsGuest_ReturnsUnauthorized()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = _user.Id,
            User = _user,
        };

        _todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await _client.DeleteAsync("/TodoItem/" + todoItem.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _todoServiceMock.Verify(todoService => todoService.FindTodoItemById(It.IsAny<int>()), Times.Never);
        _todoServiceMock.Verify(todoService => todoService.DeleteTodoItem(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task Delete_TodosOfOtherUsers_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", _accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = "random",
        };

        _todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await _client.DeleteAsync("/TodoItem/" + todoItem.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        _todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        _todoServiceMock.Verify(todoService => todoService.DeleteTodoItem(It.IsAny<TodoItem>()), Times.Never);
    }
}
