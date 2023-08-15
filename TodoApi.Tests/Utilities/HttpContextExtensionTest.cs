using Microsoft.AspNetCore.Http;
using TodoApi.Models;
using TodoApi.Utilities;

namespace TodoApi.Tests.Utilities;

public class HttpContextExtensionTest
{
    [Fact]
    public void GetApplicationUser_ReturnsCurrentUser()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var user = new ApplicationUser();

        httpContext.Items["ApplicationUser"] = user;

        // Act
        var result = httpContext.GetApplicationUser();

        // Assert
        Assert.Equal(user, result);
    }

    [Fact]
    public void GetApplicationUser_ThrowsUnauthorizedAccessException_WhenUserNotPresent()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => httpContext.GetApplicationUser());
    }
}
