using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using TodoApi.Interfaces;
using TodoApi.ResourceModels;

namespace TodoApi.Controllers;

/// <summary>
/// Controller to regenerate access token and refresh token for user.
/// </summary>
[ApiController]
[Route("Auth/[controller]")]
public class RefreshTokenController : ControllerBase
{
    private readonly IAuthTokenManagerService _tokenManager;

    public RefreshTokenController(IAuthTokenManagerService tokenManager)
    {
        _tokenManager = tokenManager;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginUserTokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    public IActionResult RefreshToken([FromForm] RefreshTokenRequest request)
    {
        try
        {
            var accessToken = HttpContext.Request.Headers[HeaderNames.Authorization].ToString();

            var user = _tokenManager.FindUserByToken(accessToken);

            if (user != null && user.RefreshToken == request.RefreshToken)
            {
                var resetToken = _tokenManager.CreateToken(user);

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