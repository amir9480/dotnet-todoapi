

using TodoApi.Data;
using TodoApi.Interfaces;
using TodoApi.Models;

public class DatabaseTodoService: ITodoService
{
    private ApplicationDbContext dbContext;

    public DatabaseTodoService(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public TodoItem CreateTodoItem(ApplicationUser user, string text)
    {
        TodoItem newItem = new TodoItem {
            Text = text,
            UserId = user.Id
        };

        dbContext.TodoItems.Add(newItem);

        dbContext.SaveChanges();

        return newItem;
    }

    public TodoItem? FindTodoItemById(int id)
    {
        return dbContext.TodoItems
            .Where(todoItem => todoItem.Id == id)
            .FirstOrDefault<TodoItem?>();
    }

    public void MarkCompleted(TodoItem item)
    {
        UpdateIsCompleted(item, true);
    }

    public void MarkIncompleted(TodoItem item)
    {
        UpdateIsCompleted(item, false);
    }

    private void UpdateIsCompleted(TodoItem item, bool isCompleted)
    {
        item.IsCompleted = isCompleted;

        dbContext.TodoItems.Update(item);

        dbContext.SaveChanges();
    }

    public TodoItem UpdateTodoItemText(TodoItem item, string text)
    {
        item.Text = text;

        dbContext.TodoItems.Update(item);

        dbContext.SaveChanges();

        return item;
    }

    public void DeleteTodoItem(TodoItem item)
    {
        dbContext.TodoItems.Remove(item);

        dbContext.SaveChanges();
    }

    public ICollection<TodoItem> GetTodoItems(ApplicationUser user)
    {
        return dbContext.TodoItems
            .Where(todoItem => todoItem.UserId == user.Id)
            .OrderBy(todoItem => todoItem.IsCompleted)
            .ToList();
    }
}
