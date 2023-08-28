using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Tests.Services;

public class DatabaseTodoServiceTest: IClassFixture<DbContextFixture>
{
    private readonly ApplicationUser user;
    private readonly ApplicationUser user2;
    private ApplicationDbContext dbContext;
    private DatabaseTodoService todoService;

    public DatabaseTodoServiceTest(DbContextFixture fixture)
    {
        dbContext = fixture.DbContext;
        todoService = new DatabaseTodoService(dbContext);
        user = new ApplicationUser
        {
            UserName = "testUsername",
            Email = "test@example.com"
        };
        user2 = new ApplicationUser
        {
            UserName = "testUsername2",
            Email = "test2@example.com"
        };
        dbContext.Users.AddRange(user, user2);
        dbContext.SaveChanges();
    }

    [Fact]
    public void CreateTodoItem_Should_AddNewItemToDatabase()
    {
        // Arrange
        var text = "Sample todo item";

        // Act
        var item = todoService.CreateTodoItem(user, text);

        // Assert
        var savedItem = dbContext.TodoItems.Where(todoItem => todoItem.Id == item.Id).First();
        Assert.NotNull(savedItem);
        Assert.Equal(text, savedItem.Text);
        Assert.Equal(savedItem.Id, item.Id);
        Assert.Equal(user.Id, savedItem.UserId);
    }

    [Fact]
    public void FindTodoById_Should_ReturnMatchingTodoItem()
    {
        // Arrange
        var item1 = new TodoItem { UserId = user.Id, Text = "Sample Todo 1" };
        var item2 = new TodoItem { UserId = user.Id, Text = "Sample Todo 2" };
        dbContext.TodoItems.AddRange(item1, item2);
        dbContext.SaveChanges();

        // Act
        var result = todoService.FindTodoItemById(item2.Id);

        // Assert
        Assert.Equal(item2, result);
    }

    [Fact]
    public void FindTodoById_Should_ReturnNullWhenDoesntExists()
    {
        // Act
        var result = todoService.FindTodoItemById(321654);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MarkCompleted_Should_UpdateItemInDatabase()
    {
        // Arrange
        var item = new TodoItem { UserId = user.Id, Text = "Sample Todo", IsCompleted = false };
        dbContext.TodoItems.Add(item);
        dbContext.SaveChanges();

        // Act
        todoService.MarkCompleted(item);

        // Assert
        Assert.True(item.IsCompleted);
        dbContext.Entry(item).Reload();
        Assert.True(item.IsCompleted);
    }

    [Fact]
    public void MarkIncompleted_Should_CallMarkCompletedWithFalse()
    {
        // Arrange
        var item = new TodoItem { UserId = user.Id, Text = "Sample Todo", IsCompleted = true };
        dbContext.TodoItems.Add(item);
        dbContext.SaveChanges();

        // Act
        todoService.MarkIncompleted(item);

        // Assert
        Assert.False(item.IsCompleted);
        dbContext.Entry(item).Reload();
        Assert.False(item.IsCompleted);
    }

    [Fact]
    public void UpdateTodoItemText_Should_UpdateItemTextInDatabase()
    {
        // Arrange
        var item = new TodoItem { UserId = user.Id, Text = "Old Text" };
        dbContext.TodoItems.Add(item);
        dbContext.SaveChanges();
        var newText = "New text";

        // Act
        var result = todoService.UpdateTodoItemText(item, newText);

        // Assert
        Assert.Equal(newText, result.Text);
        dbContext.Entry(result).Reload();
        Assert.Equal(newText, result.Text);
    }

    [Fact]
    public void RemoveTodoItem_Should_RemoveItemFromDatabase()
    {
        // Arrange
        var item = new TodoItem { UserId = user.Id, Text = "Sample Todo" };
        dbContext.TodoItems.Add(item);
        dbContext.SaveChanges();

        // Act
        todoService.DeleteTodoItem(item);

        // Assert
        Assert.DoesNotContain(item, dbContext.TodoItems);
    }

    [Fact]
    public void GetTodoItems_Should_ReturnItemsForUser()
    {
        // Arrange
        var item1 = new TodoItem { UserId = user.Id, Text = "Todo Item 1"};
        var item2 = new TodoItem { UserId = user.Id, Text = "Todo Item 2" };
        var item3 = new TodoItem { UserId = user2.Id, Text = "Todo Item 3" };
        dbContext.TodoItems.AddRange(item1, item2, item3);
        dbContext.SaveChanges();

        // Act
        var result = todoService.GetTodoItems(user);

        // Assert
        Assert.Contains(item1, result);
        Assert.Contains(item2, result);
        Assert.DoesNotContain(item3, result);
    }
}
