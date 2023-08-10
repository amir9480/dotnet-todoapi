using System.ComponentModel.DataAnnotations;

namespace TodoApi.ResourceModels;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
