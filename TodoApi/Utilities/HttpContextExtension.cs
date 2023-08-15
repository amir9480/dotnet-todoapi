using TodoApi.Models;

namespace TodoApi.Utilities;

public static class HttpContextExtensions
{
    /// <summary>
    /// Get current user from HttpContext.
    /// </summary>
    /// <returns>Current logged in user.</returns>
    /// <exception cref="UnauthorizedAccessException">If no user is logged in.</exception>
    public static ApplicationUser GetApplicationUser(this HttpContext httpContext)
    {
        ApplicationUser? user = httpContext.Items["ApplicationUser"] as ApplicationUser;

        if (user == null)
        {
            throw new UnauthorizedAccessException("Current user not found in HttpContext.");
        }

        return user;
    }
}
