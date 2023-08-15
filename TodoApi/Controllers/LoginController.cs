using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Interfaces;
using TodoApi.Models;
using TodoApi.ResourceModels;

namespace TodoApi.Controllers;

/// <summary>
/// Controller to login user with email and password and get access token and refresh token.
/// </summary>
[ApiController]
[Route("Auth/[controller]")]
public class LoginController : ControllerBase
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IAuthTokenManagerService tokenCreationService;

    public LoginController(UserManager<ApplicationUser> userManager, IAuthTokenManagerService tokenCreationService)
    {
        this.userManager = userManager;
        this.tokenCreationService = tokenCreationService;
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type=typeof(LoginUserTokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type=typeof(string))]
    public async Task<IActionResult> Login([FromForm] LoginUserRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Email);

        if (user == null)
        {
            return BadRequest("Wrong credentials");
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            return BadRequest("Wrong credentials");
        }

        LoginUserTokenResponse token = tokenCreationService.CreateToken(user);

        return Ok(token);
    }
}
