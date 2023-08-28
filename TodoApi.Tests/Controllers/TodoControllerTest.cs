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
    private readonly HttpClient client;
    private readonly Mock<ITodoService> todoServiceMock;
    private readonly ApplicationUser user;
    private readonly string accessTokenHeader;

    public TodoItemControllerTest(WebTestFixture fixture)
    {
        todoServiceMock = new Mock<ITodoService>();
        client = fixture.WithServices(
                services => services.AddScoped<ITodoService>(serviceProvider => todoServiceMock.Object)
            )
            .CreateClient();
        user = fixture.User;
        accessTokenHeader = $"Bearer {fixture.UserAccessToken}";
    }

    [Fact]
    public async Task Create_TodoItem_ReturnsOkStatusCodeAndCreatedTodoItem()
    {
        // Arrange
        client.DefaultRequestHeaders.Add("Authorization", accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = user.Id,
            User = user,
        };

        var request = new NewTodoRequest
        {
            Text = todoItem.Text
        };

        todoServiceMock.Setup(todoService => todoService.CreateTodoItem(It.Is<ApplicationUser>(u => u.Id == user.Id), request.Text))
            .Returns(todoItem);

        // Act
        var response = await client.PostAsync("/TodoItem", request.ToFormUrlEncodedContent());
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TodoItem>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        todoServiceMock.Verify(todoService => todoService.CreateTodoItem(It.Is<ApplicationUser>(u => u.Id == user.Id), request.Text), Times.Once);
    }


    [Fact]
    public async Task Create_TodosAsGuest_ReturnsUnauthorized()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = user.Id,
            User = user,
        };
        var request = new NewTodoRequest
        {
            Text = "test"
        };

        todoServiceMock.Setup(todoService => todoService.CreateTodoItem(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Returns(todoItem);

        // Act
        var response = await client.PostAsync("/TodoItem", request.ToFormUrlEncodedContent());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        todoServiceMock.Verify(todoService => todoService.CreateTodoItem(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Update_ExistingItem_ReturnsOkStatusCodeAndUpdatedItem()
    {
        // Arrange
        client.DefaultRequestHeaders.Add("Authorization", accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = user.Id,
            User = user,
        };
        var updatedTodoItem = new TodoItem
        {
            Id = todoItem.Id,
            Text = "New Todo Text",
            UserId = user.Id,
            User = user,
        };

        var request = new UpdateTodoRequest
        {
            Text = updatedTodoItem.Text
        };

        todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        todoServiceMock.Setup(todoService => todoService.UpdateTodoItemText(todoItem, updatedTodoItem.Text))
            .Returns(updatedTodoItem);

        // Act
        var response = await client.PutAsync("/TodoItem/" + todoItem.Id, request.ToFormUrlEncodedContent());
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TodoItem>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        todoServiceMock.Verify(todoService => todoService.UpdateTodoItemText(todoItem, updatedTodoItem.Text), Times.Once);
    }


    [Fact]
    public async Task Update_TodosAsGuest_ReturnsUnauthorized()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = user.Id,
            User = user,
        };
        var request = new UpdateTodoRequest
        {
            Text = "test"
        };

        todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await client.PutAsync("/TodoItem/" + todoItem.Id, request.ToFormUrlEncodedContent());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        todoServiceMock.Verify(todoService => todoService.FindTodoItemById(It.IsAny<int>()), Times.Never);
        todoServiceMock.Verify(todoService => todoService.UpdateTodoItemText(It.IsAny<TodoItem>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Update_TodosOfOtherUsers_ReturnsForbidden()
    {
        // Arrange
        client.DefaultRequestHeaders.Add("Authorization", accessTokenHeader);
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

        todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await client.PutAsync("/TodoItem/" + todoItem.Id, request.ToFormUrlEncodedContent());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        todoServiceMock.Verify(todoService => todoService.UpdateTodoItemText(It.IsAny<TodoItem>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task MarkCompleted_ExistingItem_ReturnsOkStatusCodeAndUpdatedItem()
    {
        // Arrange
        client.DefaultRequestHeaders.Add("Authorization", accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            IsCompleted = false,
            Text = "Test",
            UserId = user.Id,
            User = user,
        };

        todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        todoServiceMock.Setup(todoService => todoService.MarkCompleted(todoItem));

        // Act
        var response = await client.PatchAsync("/TodoItem/" + todoItem.Id + "/MarkCompleted", null);
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TodoItem>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        todoServiceMock.Verify(todoService => todoService.MarkCompleted(todoItem), Times.Once);
    }


    [Fact]
    public async Task MarkCompleted_TodosAsGuest_ReturnsUnauthorized()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = user.Id,
            User = user,
        };

        todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await client.PatchAsync("/TodoItem/" + todoItem.Id + "/MarkCompleted", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        todoServiceMock.Verify(todoService => todoService.FindTodoItemById(It.IsAny<int>()), Times.Never);
        todoServiceMock.Verify(todoService => todoService.MarkCompleted(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task MarkCompleted_TodosOfOtherUsers_ReturnsForbidden()
    {
        // Arrange
        client.DefaultRequestHeaders.Add("Authorization", accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = "random",
        };

        todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await client.PatchAsync("/TodoItem/" + todoItem.Id + "/MarkCompleted", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        todoServiceMock.Verify(todoService => todoService.MarkCompleted(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task MarkIncompleted_ExistingItem_ReturnsOkStatusCodeAndUpdatedItem()
    {
        // Arrange
        client.DefaultRequestHeaders.Add("Authorization", accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            IsCompleted = false,
            Text = "Test",
            UserId = user.Id,
            User = user,
        };

        todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        todoServiceMock.Setup(todoService => todoService.MarkIncompleted(todoItem));

        // Act
        var response = await client.PatchAsync("/TodoItem/" + todoItem.Id + "/MarkIncompleted", null);
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TodoItem>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        todoServiceMock.Verify(todoService => todoService.MarkIncompleted(todoItem), Times.Once);
    }


    [Fact]
    public async Task MarkIncompleted_TodosAsGuest_ReturnsUnauthorized()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = user.Id,
            User = user,
        };

        todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await client.PatchAsync("/TodoItem/" + todoItem.Id + "/MarkIncompleted", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        todoServiceMock.Verify(todoService => todoService.FindTodoItemById(It.IsAny<int>()), Times.Never);
        todoServiceMock.Verify(todoService => todoService.MarkIncompleted(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task MarkIncompleted_TodosOfOtherUsers_ReturnsForbidden()
    {
        // Arrange
        client.DefaultRequestHeaders.Add("Authorization", accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = "random",
        };

        todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await client.PatchAsync("/TodoItem/" + todoItem.Id + "/MarkIncompleted", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        todoServiceMock.Verify(todoService => todoService.MarkIncompleted(It.IsAny<TodoItem>()), Times.Never);
    }


    [Fact]
    public async Task Delete_ExistingItem_ReturnsOkNoContent()
    {
        // Arrange
        client.DefaultRequestHeaders.Add("Authorization", accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            IsCompleted = false,
            Text = "Test",
            UserId = user.Id,
            User = user,
        };

        todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        todoServiceMock.Setup(todoService => todoService.DeleteTodoItem(todoItem));

        // Act
        var response = await client.DeleteAsync("/TodoItem/" + todoItem.Id);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        todoServiceMock.Verify(todoService => todoService.DeleteTodoItem(todoItem), Times.Once);
    }


    [Fact]
    public async Task Delete_TodosAsGuest_ReturnsUnauthorized()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = user.Id,
            User = user,
        };

        todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await client.DeleteAsync("/TodoItem/" + todoItem.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        todoServiceMock.Verify(todoService => todoService.FindTodoItemById(It.IsAny<int>()), Times.Never);
        todoServiceMock.Verify(todoService => todoService.DeleteTodoItem(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task Delete_TodosOfOtherUsers_ReturnsForbidden()
    {
        // Arrange
        client.DefaultRequestHeaders.Add("Authorization", accessTokenHeader);
        var todoItem = new TodoItem
        {
            Id = 1,
            Text = "Test",
            UserId = "random",
        };

        todoServiceMock.Setup(todoService => todoService.FindTodoItemById(todoItem.Id))
            .Returns(todoItem);

        // Act
        var response = await client.DeleteAsync("/TodoItem/" + todoItem.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        todoServiceMock.Verify(todoService => todoService.FindTodoItemById(todoItem.Id), Times.Once);
        todoServiceMock.Verify(todoService => todoService.DeleteTodoItem(It.IsAny<TodoItem>()), Times.Never);
    }
}
