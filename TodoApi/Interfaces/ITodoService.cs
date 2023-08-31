using TodoApi.Models;

namespace TodoApi.Interfaces;

/// <summary>
/// Interface to implement todo management.
/// </summary>
public interface ITodoService
{
    /// <summary>
    /// Create new todo: for given user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="text"></param>
    /// <returns>New created todo item.</returns>
    public TodoItem CreateTodoItem(ApplicationUser user, string text);

    /// <summary>
    /// Find todo by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>TodoItem model, returns null if item is not found.</returns>
    public TodoItem? FindTodoItemById(int id);

    /// <summary>
    /// Mark todo as completed.
    /// </summary>
    /// <param name="item"></param>
    public void MarkCompleted(TodoItem item);

    /// <summary>
    /// Mark todo as uncompleted.
    /// </summary>
    /// <param name="item"></param>
    public void MarkInCompleted(TodoItem item);

    /// <summary>
    /// Update todo item text.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="text"></param>
    /// <returns>Item with updated text.</returns>
    public TodoItem UpdateTodoItemText(TodoItem item, string text);

    /// <summary>
    /// Remove todo item.
    /// </summary>
    /// <param name="item"></param>
    public void DeleteTodoItem(TodoItem item);

    /// <summary>
    /// Get todo items for the given user.
    /// </summary>
    /// <param name="user"></param>
    /// <returns>Collection of todo items for given user.</returns>
    public ICollection<TodoItem> GetTodoItems(ApplicationUser user);
}