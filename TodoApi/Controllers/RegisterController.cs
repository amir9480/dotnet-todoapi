using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.ResourceModels;

namespace TodoApi.Controllers;

/// <summary>
/// Controller to register new user with email and password.
/// </summary>
[ApiController]
[Route("Auth/[controller]")]
public class RegisterController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResult))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(IEnumerable<IdentityError>))]
    public async Task<IActionResult> RegisterUser([FromForm] RegisterUserRequest model)
    {
        var result = await _userManager.CreateAsync(
            new ApplicationUser { UserName = model.Email, Email = model.Email },
            model.Password
        );

        if (!result.Succeeded)
            return UnprocessableEntity(result.Errors);

        model.Password = "";

        return Created("", model);
    }
}