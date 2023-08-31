using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.ResourceModels;
using TodoApi.Utilities;

namespace TodoApi.Controllers;

/// <summary>
/// Controller to login user with email and password and get access token and refresh token.
/// </summary>
[ApiController]
[Authorize]
[Route("Auth/[controller]")]
public class MeController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MeResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(UnauthorizedResult))]
    public IActionResult Me()
    {
        var user = HttpContext.GetApplicationUser();

        var response = new MeResponse
        {
            Id = user.Id,
            Email = user.Email,
            RefreshTokenExpiryTime = user.RefreshTokenExpiryTime,
        };

        return Ok(response);
    }
}