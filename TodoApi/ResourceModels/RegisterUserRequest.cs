using System.ComponentModel.DataAnnotations;

namespace TodoApi.ResourceModels;

public class RegisterUserRequest
{
    //Todo: Please Use Required  specifier

    [EmailAddress]
    [Required] public string Email { get; set; } = null!;
    [Required] public string Password { get; set; } = null!;
}
