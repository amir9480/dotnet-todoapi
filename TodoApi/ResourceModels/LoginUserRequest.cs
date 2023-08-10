using System.ComponentModel.DataAnnotations;

namespace TodoApi.ResourceModels;

public class LoginUserRequest
{
    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
