using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;
using TodoApi.Interfaces;
using TodoApi.Models;
using TodoApi.Utilities;

namespace TodoApi.Tests;

public class WebTestFixture : WebApplicationFactory<Program>
{
    public const string TEST_USER_USERNAME = "testUsername";
    public const string TEST_USER_EMAIL = "test@example.com";

    public ApplicationDbContext? DbContext = null;
    public readonly ApplicationUser User;
    public string UserAccessToken = "";

    private string memoryDatabaseName;

    public WebTestFixture()
    {
        memoryDatabaseName = "Testing-" + Guid.NewGuid().ToString();
        User = new ApplicationUser
        {
            UserName = TEST_USER_USERNAME,
            Email = TEST_USER_EMAIL
        };
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var provider = services
              .AddEntityFrameworkInMemoryDatabase()
              .BuildServiceProvider();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(memoryDatabaseName);
                options.UseInternalServiceProvider(provider);
            });
            services.AddAuthSupport();

            var serviceProvider = services.BuildServiceProvider();
            DbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure the database is recreated.
            DbContext.Database.EnsureDeleted();
            DbContext.Database.EnsureCreated();

            UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var result = userManager.CreateAsync(User, "password").GetAwaiter().GetResult();
            Assert.True(result.Succeeded);

            IAuthTokenManagerService tokenManagerService = serviceProvider.GetRequiredService<IAuthTokenManagerService>();
            UserAccessToken = tokenManagerService.CreateToken(User).AccessToken;
        });
    }

    public HttpClient CreateClientWithAuthHeader()
    {
        var client = CreateClient();

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {UserAccessToken}");

        return client;
    }

    public WebApplicationFactory<Program> WithServices(Action<IServiceCollection> configureServices)
    {
        return WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(configureServices);
        });
    }
}
