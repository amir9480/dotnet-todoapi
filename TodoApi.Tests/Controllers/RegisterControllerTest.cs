using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoApi.Controllers;
using TodoApi.Models;
using TodoApi.ResourceModels;

namespace TodoApi.Tests.Controllers;

public class RegisterControllerTest
{
    private readonly Mock<UserManager<ApplicationUser>> userManagerMock;
    private readonly RegisterController registerController;

    public RegisterControllerTest()
    {
        userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null
        );
        registerController = new RegisterController(userManagerMock.Object);
    }

    [Fact]
    public async Task Register_WithValidModel_ReturnsCreatedResult()
    {
        // Arrange
        var model = new RegisterUserRequest
        {
            Email = "test@example.com",
            Password = "password"
        };

        userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await registerController.RegisterUser(model);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal("", createdResult.Location);
        Assert.Same(model, createdResult.Value);
        Assert.IsType<RegisterUserRequest>(createdResult.Value);
        Assert.Null(((RegisterUserRequest)createdResult.Value).Password);
        userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var model = new RegisterUserRequest();
        registerController.ModelState.AddModelError("Email", "The Email field is required.");

        // Act
        var result = await registerController.RegisterUser(model);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsType<SerializableError>(badRequestResult.Value);
        Assert.Single(errors);
        Assert.Equal("The Email field is required.", ((IEnumerable<string>)errors["Email"]).First());
        userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }
}
