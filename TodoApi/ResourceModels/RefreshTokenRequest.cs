using System.ComponentModel.DataAnnotations;

namespace TodoApi.ResourceModels;

public class RefreshTokenRequest
{
    [Required] public required string RefreshToken { get; set; }
}
