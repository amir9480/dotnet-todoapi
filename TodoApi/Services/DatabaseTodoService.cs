

using TodoApi.Data;
using TodoApi.Interfaces;
using TodoApi.Models;

public class DatabaseTodoService: ITodoService
{
    private ApplicationDbContext db;

    public DatabaseTodoService(ApplicationDbContext dbContext)
    {
        this.db = dbContext;
    }

    public TodoItem CreateTodoItem(ApplicationUser user, string text)
    {
        TodoItem newItem = new TodoItem {
            Text = text,
            UserId = user.Id
        };

        db.TodoItems.Add(newItem);

        db.SaveChanges();

        return newItem;
    }

    public void MarkCompleted(TodoItem item)
    {
        throw new NotImplementedException();
    }

    public void MarkIncompleted(TodoItem item)
    {
        throw new NotImplementedException();
    }
}
