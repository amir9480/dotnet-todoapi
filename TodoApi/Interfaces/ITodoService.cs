using TodoApi.Models;
using TodoApi.ResourceModels;

namespace TodoApi.Interfaces;

/// <summary>
/// Interface to implement todo managment.
/// </summary>
public interface ITodoService
{
    /// <summary>
    /// Create new todo for given user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="text"></param>
    /// <returns>New created todo item.</returns>
    public TodoItem CreateTodoItem(ApplicationUser user, string text);

    /// <summary>
    /// Mark todo as completed.
    /// </summary>
    /// <param name="item"></param>
    public void MarkCompleted(TodoItem item);

    /// <summary>
    /// Mark todo as uncompleted.
    /// </summary>
    /// <param name="item"></param>
    public void MarkIncompleted(TodoItem item);
}
