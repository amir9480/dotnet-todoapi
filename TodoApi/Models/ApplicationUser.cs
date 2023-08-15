using Microsoft.AspNetCore.Identity;

namespace TodoApi.Models;

public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// API user refresh token.
    /// </summary>
    public String? RefreshToken { get; set; } = null;

    /// <summary>
    /// User refresh token expiry time.
    /// </summary>
    public DateTime? RefreshTokenExpiryTime { get; set; } = null;

    /// <summary>
    /// Todo items of each user.
    /// </summary>
    public ICollection<TodoItem> TodoItems { get; set; } = null!;
}
