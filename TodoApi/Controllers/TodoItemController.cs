using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Interfaces;
using TodoApi.Models;
using TodoApi.ResourceModels;
using TodoApi.Utilities;

namespace TodoApi.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class TodoItemController : ControllerBase
{
    private readonly ITodoService todoService;

    public TodoItemController(ITodoService todoService)
    {
        this.todoService = todoService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type=typeof(UnauthorizedResult))]
    public IActionResult Index()
    {
        ApplicationUser user = HttpContext.GetApplicationUser();

        var todoItems = todoService.GetTodoItems(user);

        return Ok(todoItems);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type=typeof(TodoItem))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type=typeof(BadRequestResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type=typeof(UnauthorizedResult))]
    public IActionResult Create([FromForm] NewTodoRequest model)
    {
        ApplicationUser user = HttpContext.GetApplicationUser();

        TodoItem item = todoService.CreateTodoItem(user, model.Text);

        return Created("", item);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type=typeof(TodoItem))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type=typeof(BadRequestResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type=typeof(UnauthorizedResult))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type=typeof(NotFoundResult))]
    public IActionResult Update(int id, [FromForm] UpdateTodoRequest model)
    {
        return RunTodoItemAction(id, (TodoItem todoItem) => todoService.UpdateTodoItemText(todoItem, model.Text));
    }

    [HttpPatch("{id}/MarkCompleted")]
    [ProducesResponseType(StatusCodes.Status200OK, Type=typeof(TodoItem))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type=typeof(UnauthorizedResult))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type=typeof(NotFoundResult))]
    public IActionResult MarkCompleted(int id)
    {
        return RunTodoItemAction(id, todoService.MarkCompleted);
    }

    [HttpPatch("{id}/MarkIncompleted")]
    [ProducesResponseType(StatusCodes.Status200OK, Type=typeof(TodoItem))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type=typeof(UnauthorizedResult))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type=typeof(NotFoundResult))]
    public IActionResult MarkIncompleted(int id)
    {
        return RunTodoItemAction(id, todoService.MarkIncompleted);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent, Type=typeof(NoContentResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type=typeof(UnauthorizedResult))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type=typeof(NotFoundResult))]
    public IActionResult Delete(int id)
    {
        IActionResult result = RunTodoItemAction(id, todoService.DeleteTodoItem);

        if (result is OkObjectResult)
        {
            return NoContent();
        }
        else
        {
            return result;
        }
    }

    private IActionResult RunTodoItemAction(int id, Action<TodoItem> action)
    {
        var todoItem = todoService.FindTodoItemById(id);

        if (todoItem == null)
        {
            return NotFound();
        }

        ApplicationUser user = HttpContext.GetApplicationUser();

        // Prevent user changing other users todos
        if (UsersOwnsTodoItem(user, todoItem) == false)
        {
            return Forbid();
        }

        action(todoItem);

        return Ok(todoItem);
    }

    private bool UsersOwnsTodoItem(ApplicationUser user, TodoItem todoItem)
    {
        return todoItem.UserId == user.Id;
    }
}
