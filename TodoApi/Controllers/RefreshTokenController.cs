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
    private readonly IAuthTokenManagerService tokenService;

    public RefreshTokenController(IAuthTokenManagerService tokenService)
    {
        this.tokenService = tokenService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type=typeof(LoginUserTokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type=typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type=typeof(string))]
    public IActionResult RefreshToken([FromForm] RefreshTokenRequest request)
    {
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
