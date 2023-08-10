namespace TodoApi.ResourceModels;

public class LoginUserTokenResponse
{
    public string AccessToken { get; set; } = null!;

    public DateTime AccessTokenExpiration { get; set; } = DateTime.Now;

    public string RefreshToken { get; set; } = null!;

    public DateTime RefreshTokenExpiration { get; set; } = DateTime.Now;
}
