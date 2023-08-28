using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using TodoApi.Models;
using TodoApi.ResourceModels;
using TodoApi.Services;

namespace TodoApi.Tests.Services;

public class JWTTokenManagerServiceTest
{
    private const string TEST_ISSUER = "testIssuer";
    private const string TEST_AUDIENCE = "testAudience";

    private const string TEST_USER_ID = "testUserId";
    private const string TEST_USER_USERNAME = "testUsername";
    private const string TEST_USER_EMAIL = "test@example.com";

    private readonly Mock<IConfiguration> configurationMock;
    private readonly Mock<UserManager<ApplicationUser>> userManagerMock;
    private readonly JWTTokenManagerService jwtService;
    private readonly ApplicationUser user;

    public JWTTokenManagerServiceTest()
    {
        configurationMock = new Mock<IConfiguration>();
        userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null
        );

        configurationMock.SetupGet(c => c["Jwt:Issuer"]).Returns(TEST_ISSUER);
        configurationMock.SetupGet(c => c["Jwt:Audience"]).Returns(TEST_AUDIENCE);

        jwtService = new JWTTokenManagerService(configurationMock.Object, userManagerMock.Object);
        user = new ApplicationUser
        {
            Id = TEST_USER_ID,
            UserName = TEST_USER_USERNAME,
            Email = TEST_USER_EMAIL
        };
    }

    [Fact]
    public void CreateToken_ReturnsLoginUserTokenResponse()
    {
        // Arrange
        userManagerMock
            .Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        LoginUserTokenResponse result = jwtService.CreateToken(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.RefreshToken, result.RefreshToken);
        Assert.Equal(user.RefreshTokenExpiryTime, result.RefreshTokenExpiration);
        Assert.NotEmpty(result.AccessToken);
        Assert.True(DateTime.UtcNow.AddMinutes(10) - result.AccessTokenExpiration < TimeSpan.FromSeconds(3));
        userManagerMock.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public void CreateToken_GeneratesValidJwtToken()
    {
        // Arrange
        userManagerMock
            .Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        LoginUserTokenResponse result = jwtService.CreateToken(user);

        // Assert
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        JwtSecurityToken token = tokenHandler.ReadJwtToken(result.AccessToken);
        Assert.Equal(TEST_ISSUER, token.Issuer);
        Assert.Equal(TEST_AUDIENCE, token.Audiences.FirstOrDefault());
        Assert.Equal(TEST_USER_ID, token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(TEST_USER_USERNAME, token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value);
        Assert.Equal(TEST_USER_EMAIL, token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value);
        userManagerMock.Verify(um => um.UpdateAsync(It.Is<ApplicationUser>(u => u.RefreshToken == result.RefreshToken && u.RefreshTokenExpiryTime == result.RefreshTokenExpiration)), Times.Once);
    }

    [Fact]
    public void FindUserByToken_WithValidToken_ReturnsUser()
    {
        // Arrange
        userManagerMock
            .Setup(um => um.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        string accessToken = jwtService.CreateToken(user).AccessToken;
        Assert.NotNull(accessToken);

        // Act
        var result = jwtService.FindUserByToken(accessToken);

        // Assert
        Assert.NotNull(user);
        userManagerMock.Verify(u => u.FindByNameAsync(user.UserName ?? ""), Times.Once);
        Assert.NotNull(result);
        Assert.Equal(user.UserName, result.UserName);
    }

    [Fact]
    public void FindUserByToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        userManagerMock
            .Setup(um => um.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = jwtService.FindUserByToken("invalid_token");

        // Assert
        userManagerMock.Verify(u => u.FindByNameAsync(It.IsAny<string>()), Times.Never);
        Assert.Null(result);
    }
}
