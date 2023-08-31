using System.ComponentModel.DataAnnotations;

namespace TodoApi.ResourceModels;

public class RefreshTokenRequest
{
    //Todo: Please Use Required  specifier
    [Required] public string RefreshToken { get; set; } = null!;
}
