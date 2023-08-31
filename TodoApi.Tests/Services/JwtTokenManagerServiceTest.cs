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

public class JwtTokenManagerServiceTest
{
    private const string TEST_ISSUER = "testIssuer";
    private const string TEST_AUDIENCE = "testAudience";

    private const string TEST_USER_ID = "testUserId";
    private const string TEST_USER_USERNAME = "testUsername";
    private const string TEST_USER_EMAIL = "test@example.com";

    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly JwtTokenManagerService _jwtService;
    private readonly ApplicationUser _user;

    public JwtTokenManagerServiceTest()
    {
        _configurationMock = new Mock<IConfiguration>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null
        );

        _configurationMock.SetupGet(c => c["Jwt:Issuer"]).Returns(TEST_ISSUER);
        _configurationMock.SetupGet(c => c["Jwt:Audience"]).Returns(TEST_AUDIENCE);

        _jwtService = new JwtTokenManagerService(_configurationMock.Object, _userManagerMock.Object);
        _user = new ApplicationUser
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
        _userManagerMock
            .Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        LoginUserTokenResponse result = _jwtService.CreateToken(_user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_user.RefreshToken, result.RefreshToken);
        Assert.Equal(_user.RefreshTokenExpiryTime, result.RefreshTokenExpiration);
        Assert.NotEmpty(result.AccessToken);
        Assert.True(DateTime.UtcNow.AddMinutes(10) - result.AccessTokenExpiration < TimeSpan.FromSeconds(3));
        _userManagerMock.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public void CreateToken_GeneratesValidJwtToken()
    {
        // Arrange
        _userManagerMock
            .Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        LoginUserTokenResponse result = _jwtService.CreateToken(_user);

        // Assert
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        JwtSecurityToken token = tokenHandler.ReadJwtToken(result.AccessToken);
        Assert.Equal(TEST_ISSUER, token.Issuer);
        Assert.Equal(TEST_AUDIENCE, token.Audiences.FirstOrDefault());
        Assert.Equal(TEST_USER_ID, token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(TEST_USER_USERNAME, token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value);
        Assert.Equal(TEST_USER_EMAIL, token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value);
        _userManagerMock.Verify(um => um.UpdateAsync(It.Is<ApplicationUser>(u => u.RefreshToken == result.RefreshToken && u.RefreshTokenExpiryTime == result.RefreshTokenExpiration)), Times.Once);
    }

    [Fact]
    public void FindUserByToken_WithValidToken_ReturnsUser()
    {
        // Arrange
        _userManagerMock
            .Setup(um => um.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(_user);

        string accessToken = _jwtService.CreateToken(_user).AccessToken;
        Assert.NotNull(accessToken);

        // Act
        var result = _jwtService.FindUserByToken(accessToken);

        // Assert
        Assert.NotNull(_user);
        _userManagerMock.Verify(u => u.FindByNameAsync(_user.UserName ?? ""), Times.Once);
        Assert.NotNull(result);
        Assert.Equal(_user.UserName, result.UserName);
    }

    [Fact]
    public void FindUserByToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        _userManagerMock
            .Setup(um => um.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = _jwtService.FindUserByToken("invalid_token");

        // Assert
        _userManagerMock.Verify(u => u.FindByNameAsync(It.IsAny<string>()), Times.Never);
        Assert.Null(result);
    }
}
