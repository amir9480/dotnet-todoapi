using System.ComponentModel.DataAnnotations;

namespace TodoApi.ResourceModels;

public class MeResponse
{
    [Required] public required string Id { get; set; }
    public string? Email { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

}
