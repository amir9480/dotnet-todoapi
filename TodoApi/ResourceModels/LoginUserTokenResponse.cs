namespace TodoApi.ResourceModels;

public class LoginUserTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiration { get; set; } = DateTime.Now;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiration { get; set; } = DateTime.Now;
}
