namespace TodoApi.ResourceModels;

public class MeResponse
{
    public string Id { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public DateTime? RefreshTokenExpiryTime { get; set; } = null;

}
