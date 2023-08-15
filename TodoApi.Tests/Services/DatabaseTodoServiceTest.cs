using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Tests.Services;

public class DatabaseTodoServiceTest
{
    private const string TEST_USER_ID = "testUserId";
    private const string TEST_USER_USERNAME = "testUsername";
    private const string TEST_USER_EMAIL = "test@example.com";

    private readonly ApplicationUser user;
    private ApplicationDbContext dbContext;
    private DatabaseTodoService todoService;

    public DatabaseTodoServiceTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;

        dbContext = new ApplicationDbContext(options);
        todoService = new DatabaseTodoService(dbContext);
        user = new ApplicationUser
        {
            Id = TEST_USER_ID,
            UserName = TEST_USER_USERNAME,
            Email = TEST_USER_EMAIL
        };
    }

    [Fact]
    public void CreateTodoItem_Should_AddNewItemToDatabase()
    {
        // Arrange
        var text = "Sample todo item";

        // Act
        var result = todoService.CreateTodoItem(user, text);

        // Assert
        Assert.Single(dbContext.TodoItems);
        var savedItem = dbContext.TodoItems.First();
        Assert.Equal(text, savedItem.Text);
        Assert.Equal(savedItem.Id, result.Id);
        Assert.Equal(user.Id, savedItem.UserId);
    }
}
