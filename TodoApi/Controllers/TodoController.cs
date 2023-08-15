using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Interfaces;
using TodoApi.Models;
using TodoApi.ResourceModels;
using TodoApi.Utilities;

namespace TodoApi.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class TodoController : ControllerBase
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ITodoService todoService;

    public TodoController(UserManager<ApplicationUser> userManager, ITodoService todoService)
    {
        this.userManager = userManager;
        this.todoService = todoService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type=typeof(LoginUserTokenResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type=typeof(BadRequestResult))]
    public IActionResult Create([FromForm] NewTodoRequest model)
    {
        ApplicationUser user = HttpContext.GetApplicationUser();

        TodoItem item = todoService.CreateTodoItem(user, model.Text);

        return Created("", item);
    }
}
