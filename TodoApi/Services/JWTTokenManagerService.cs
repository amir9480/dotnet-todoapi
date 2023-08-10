using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Interfaces;
using TodoApi.Models;
using TodoApi.ResourceModels;

namespace TodoApi.Services;

/// <summary>
/// Service class for auth token managment using JWT tokens.
/// </summary>
public class JWTTokenManagerService : IAuthTokenManagerService
{
    private readonly IConfiguration configuration;
    private readonly UserManager<ApplicationUser> userManager;

    public JWTTokenManagerService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
    {
        this.configuration = configuration;
        this.userManager = userManager;
    }

    /// <summary>
    /// Find a user based on JWT access token.
    /// </summary>
    /// <remarks>
    /// This method can not be used for refresh tokens.
    /// </remarks>
    /// <param name="tokenString"></param>
    /// <returns>User moodel. null if token is invalid.</returns>
    public ApplicationUser? FindUserByToken(string tokenString)
    {
        if (tokenString.StartsWith("bearer", StringComparison.InvariantCultureIgnoreCase))
        {
            tokenString = tokenString.Substring(7);
        }

        try
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);
            var username = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (username != null)
            {
                return userManager.FindByNameAsync(username).GetAwaiter().GetResult();
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    /// <summary>
    /// Create new JWT access token and refresh token for the givn user.
    /// </summary>
    /// <param name="user">User to create access token and refresh token for.</param>
    /// <returns>New access token and refresh token and their validity lifetime.</returns>
    public LoginUserTokenResponse CreateToken(ApplicationUser user)
    {
        DateTime accessTokenExpiration = DateTime.UtcNow.AddSeconds(int.Parse(configuration["Jwt:AccessTokenExpiration"] ?? "3600"));
        DateTime refreshTokenExpiration = DateTime.UtcNow.AddDays(int.Parse(configuration["Jwt:AccessTokenExpiration"] ?? "7"));

        String accessToken = CreateJwtToken(user, accessTokenExpiration);
        String refreshToken = CreateRefreshToken(user, refreshTokenExpiration);

        return new LoginUserTokenResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiration = accessTokenExpiration,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpiration,
        };
    }

    /// <summary>
    /// Create JWT access token for given user.
    /// </summary>
    /// <param name="user">User to create access token for.</param>
    /// <param name="expiration">Access token expiration date time.</param>
    /// <returns>JWT token as string.</returns>
    private String CreateJwtToken(ApplicationUser user, DateTime expiration)
    {
        Claim[] claims = CreateClaims(user);
        SigningCredentials credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY") ?? "")),
            SecurityAlgorithms.HmacSha256
        );

        JwtSecurityToken token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: expiration,
            signingCredentials: credentials
        );

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Create refresh token for given user and save it to database.
    /// </summary>
    /// <param name="user">User to create refresh token for.</param>
    /// <param name="expiration">Refresh token expiration date time.</param>
    /// <returns>New refresh token for given user.</returns>
    private string CreateRefreshToken(ApplicationUser user, DateTime expiration)
    {
        string refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = expiration;
        userManager.UpdateAsync(user);

        return refreshToken;
    }

    /// <summary>
    /// Generate random refresh token.
    /// </summary>
    /// <returns>Random token that can be used as refresh token.</returns>
    private static string GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        string hashedToken = Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password: Convert.ToBase64String(randomNumber),
                salt: Array.Empty<byte>(),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 64
            )
        );

        return hashedToken;
    }

    /// <summary>
    /// Create claims for new JWT token for given user.
    /// </summary>
    /// <param name="user"></param>
    /// <returns>Array of claims to create JWT token.</returns>
    private Claim[] CreateClaims(ApplicationUser user)
    {
        return new[] {
            new Claim(JwtRegisteredClaimNames.Sub, configuration["Jwt:Subject"] ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? "")
        };
    }
}
