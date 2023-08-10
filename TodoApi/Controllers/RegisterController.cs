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
    private readonly UserManager<ApplicationUser> userManager;

    public RegisterController(UserManager<ApplicationUser> userManager)
    {
        this.userManager = userManager;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterUser([FromForm] RegisterUserRequest model)
    {
        if (! ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        IdentityResult result = await userManager.CreateAsync(
            new ApplicationUser() { UserName = model.Email, Email = model.Email },
            model.Password
        );

        if (! result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        model.Password = null!;

        return Created("", model);
    }
}
