using System.ComponentModel.DataAnnotations;

namespace TodoApi.ResourceModels;

public class RegisterUserRequest
{
    [EmailAddress]
    [Required]
    public required string Email { get; set; }
    
    [Required] public required string Password { get; set; }
}
