using TodoApi.Models;
using TodoApi.ResourceModels;

namespace TodoApi.Interfaces;

/// <summary>
/// Interface to implement auth token managment.
/// </summary>
public interface IAuthTokenManagerService
{
    /// <summary>
    /// Create new access token and refresh token for user.
    /// </summary>
    /// <param name="user"></param>
    /// <returns>Access token and refresh token and their validity lifetime.</returns>
    LoginUserTokenResponse CreateToken(ApplicationUser user);

    /// <summary>
    /// Find user model based on access token.
    /// </summary>
    /// <remarks>
    /// Validity time should be ignored.
    /// </remarks>
    /// <param name="accessToken"></param>
    /// <returns>Return user based on token, null if acccess token is invalid.</returns>
    ApplicationUser? FindUserByToken(string accessToken);
}
