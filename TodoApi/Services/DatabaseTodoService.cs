using TodoApi.Data;
using TodoApi.Interfaces;
using TodoApi.Models;

namespace TodoApi.Services;

public class DatabaseTodoService : ITodoService
{
    private ApplicationDbContext _context;

    public DatabaseTodoService(ApplicationDbContext context) => _context = context;

    public TodoItem CreateTodoItem(ApplicationUser user, string text)
    {
        var newItem = new TodoItem
        {
            Text = text,
            UserId = user.Id,
        };

        _context.TodoItems.Add(newItem);

        _context.SaveChanges();

        return newItem;
    }

    public TodoItem? FindTodoItemById(int id) =>
        _context.TodoItems
            .FirstOrDefault(todoItem => todoItem.Id == id);

    public void MarkCompleted(TodoItem item) => UpdateIsCompleted(item, true);

    public void MarkInCompleted(TodoItem item) => UpdateIsCompleted(item, false);

    private void UpdateIsCompleted(TodoItem item, bool isCompleted)
    {
        item.IsCompleted = isCompleted;
        _context.TodoItems.Update(item);
        _context.SaveChanges();
    }

    public TodoItem UpdateTodoItemText(TodoItem item, string text)
    {
        item.Text = text;
        _context.TodoItems.Update(item);
        _context.SaveChanges();

        return item;
    }

    public void DeleteTodoItem(TodoItem item)
    {
        _context.TodoItems.Remove(item);
        _context.SaveChanges();
    }

    public ICollection<TodoItem> GetTodoItems(ApplicationUser user)
    {
        return _context.TodoItems
            .Where(todoItem => todoItem.UserId == user.Id)
            .OrderBy(todoItem => todoItem.IsCompleted)
            .ToList();
    }
}