using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests.Services;

public class DatabaseTodoServiceTest: IClassFixture<DbContextFixture>
{
    private readonly ApplicationUser _user;
    private readonly ApplicationUser _user2;
    private ApplicationDbContext _context;
    private DatabaseTodoService _todoService;

    public DatabaseTodoServiceTest(DbContextFixture fixture)
    {
        _context = fixture.DbContext;
        _todoService = new DatabaseTodoService(_context);
        _user = new ApplicationUser
        {
            UserName = "testUsername",
            Email = "test@example.com"
        };
        _user2 = new ApplicationUser
        {
            UserName = "testUsername2",
            Email = "test2@example.com"
        };
        _context.Users.AddRange(_user, _user2);
        _context.SaveChanges();
    }

    [Fact]
    public void CreateTodoItem_Should_AddNewItemToDatabase()
    {
        // Arrange
        var text = "Sample todo item";

        // Act
        var item = _todoService.CreateTodoItem(_user, text);

        // Assert
        var savedItem = _context.TodoItems.Where(todoItem => todoItem.Id == item.Id).First();
        Assert.NotNull(savedItem);
        Assert.Equal(text, savedItem.Text);
        Assert.Equal(savedItem.Id, item.Id);
        Assert.Equal(_user.Id, savedItem.UserId);
    }

    [Fact]
    public void FindTodoById_Should_ReturnMatchingTodoItem()
    {
        // Arrange
        var item1 = new TodoItem { UserId = _user.Id, Text = "Sample Todo 1" };
        var item2 = new TodoItem { UserId = _user.Id, Text = "Sample Todo 2" };
        _context.TodoItems.AddRange(item1, item2);
        _context.SaveChanges();

        // Act
        var result = _todoService.FindTodoItemById(item2.Id);

        // Assert
        Assert.Equal(item2, result);
    }

    [Fact]
    public void FindTodoById_Should_ReturnNullWhenDoesntExists()
    {
        // Act
        var result = _todoService.FindTodoItemById(321654);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MarkCompleted_Should_UpdateItemInDatabase()
    {
        // Arrange
        var item = new TodoItem { UserId = _user.Id, Text = "Sample Todo", IsCompleted = false };
        _context.TodoItems.Add(item);
        _context.SaveChanges();

        // Act
        _todoService.MarkCompleted(item);

        // Assert
        Assert.True(item.IsCompleted);
        _context.Entry(item).Reload();
        Assert.True(item.IsCompleted);
    }

    [Fact]
    public void MarkIncompleted_Should_CallMarkCompletedWithFalse()
    {
        // Arrange
        var item = new TodoItem { UserId = _user.Id, Text = "Sample Todo", IsCompleted = true };
        _context.TodoItems.Add(item);
        _context.SaveChanges();

        // Act
        _todoService.MarkInCompleted(item);

        // Assert
        Assert.False(item.IsCompleted);
        _context.Entry(item).Reload();
        Assert.False(item.IsCompleted);
    }

    [Fact]
    public void UpdateTodoItemText_Should_UpdateItemTextInDatabase()
    {
        // Arrange
        var item = new TodoItem { UserId = _user.Id, Text = "Old Text" };
        _context.TodoItems.Add(item);
        _context.SaveChanges();
        var newText = "New text";

        // Act
        var result = _todoService.UpdateTodoItemText(item, newText);

        // Assert
        Assert.Equal(newText, result.Text);
        _context.Entry(result).Reload();
        Assert.Equal(newText, result.Text);
    }

    [Fact]
    public void RemoveTodoItem_Should_RemoveItemFromDatabase()
    {
        // Arrange
        var item = new TodoItem { UserId = _user.Id, Text = "Sample Todo" };
        _context.TodoItems.Add(item);
        _context.SaveChanges();

        // Act
        _todoService.DeleteTodoItem(item);

        // Assert
        Assert.DoesNotContain(item, _context.TodoItems);
    }

    [Fact]
    public void GetTodoItems_Should_ReturnItemsForUser()
    {
        // Arrange
        var item1 = new TodoItem { UserId = _user.Id, Text = "Todo Item 1"};
        var item2 = new TodoItem { UserId = _user.Id, Text = "Todo Item 2" };
        var item3 = new TodoItem { UserId = _user2.Id, Text = "Todo Item 3" };
        _context.TodoItems.AddRange(item1, item2, item3);
        _context.SaveChanges();

        // Act
        var result = _todoService.GetTodoItems(_user);

        // Assert
        Assert.Contains(item1, result);
        Assert.Contains(item2, result);
        Assert.DoesNotContain(item3, result);
    }
}
