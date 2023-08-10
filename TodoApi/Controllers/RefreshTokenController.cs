using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using TodoApi.Interfaces;
using TodoApi.Models;
using TodoApi.ResourceModels;

namespace TodoApi.Controllers;

/// <summary>
/// Controller to regenerate access token and refresh token for user.
/// </summary>
[ApiController]
[Route("Auth/[controller]")]
public class RefreshTokenController : ControllerBase
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IAuthTokenManagerService tokenService;

    public RefreshTokenController(UserManager<ApplicationUser> userManager, IAuthTokenManagerService tokenService)
    {
        this.userManager = userManager;
        this.tokenService = tokenService;
    }

    [HttpPost]
    public IActionResult RefreshToken([FromForm] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Wrong credentials");
        }

        try
        {
            var accessToken = HttpContext.Request.Headers[HeaderNames.Authorization].ToString();
            ApplicationUser? user = tokenService.FindUserByToken(accessToken);

            if (user != null && user.RefreshToken == request.RefreshToken)
            {
                var resetToken = tokenService.CreateToken(user);

                return Ok(resetToken);
            }
        }
        catch
        {
            return Unauthorized();
        }

        return Unauthorized();
    }
}
