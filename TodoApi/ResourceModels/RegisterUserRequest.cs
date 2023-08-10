using System.ComponentModel.DataAnnotations;

namespace TodoApi.ResourceModels;

public class RegisterUserRequest
{
    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
