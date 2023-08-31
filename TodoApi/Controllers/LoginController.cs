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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthTokenManagerService _tokenCreationService;

    public LoginController(UserManager<ApplicationUser> userManager, IAuthTokenManagerService tokenCreationService)
    {
        _userManager = userManager;
        _tokenCreationService = tokenCreationService;
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginUserTokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    public async Task<IActionResult> Login([FromForm] LoginUserRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Email);

        if (user == null)
            return BadRequest("Wrong credentials");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
            return BadRequest("Wrong credentials");

        var token = _tokenCreationService.CreateToken(user);

        return Ok(token);
    }
}