using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace TodoApi.Models;

public class ApplicationUser : IdentityUser
{
    //Todo: Please Use Required  specifier

    /// <summary>
    /// API user refresh token.
    /// </summary>
    [JsonIgnore]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// User refresh token expiry time.
    /// </summary>
    public DateTime? RefreshTokenExpiryTime { get; set; }

    /// <summary>
    /// Todo items of each user.
    /// </summary>
    [JsonIgnore]
    public ICollection<TodoItem> TodoItems { get; set; } = null!;
}