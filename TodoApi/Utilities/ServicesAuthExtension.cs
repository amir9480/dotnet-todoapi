using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Utilities;

public static class ServicesAuthExtension
{
    /// <summary>
    /// Configure application services to support JWT authentication.
    /// </summary>
    public static void AddAuthSupport(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var serviceProvider = services.BuildServiceProvider();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier);
                        if (userIdClaim == null)
                            context.Fail("User ID claim not found in the token.");
                        else
                        {
                            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>() ??
                                              throw new Exception();
                            var user = await userManager.FindByIdAsync(userIdClaim.Value);
                            if (user == null) context.Fail("User not found in the database.");
                            context.HttpContext.Items["ApplicationUser"] = user;
                        }
                    }
                };
                var configuration = serviceProvider.GetService<IConfiguration>();
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = configuration?["Jwt:Audience"] ?? "TodoApi",
                    ValidIssuer = configuration?["Jwt:Issuer"] ?? "TodoApi",
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY") ?? ""))
                };
            });

        services.Configure<IdentityOptions>(options =>
        {
            var env = serviceProvider.GetService<IWebHostEnvironment>();

            // Password settings.
            options.Password.RequireDigit = env?.IsEnvironment("Testing") != true;
            options.Password.RequireLowercase = env?.IsEnvironment("Testing") != true;
            options.Password.RequireNonAlphanumeric = env?.IsEnvironment("Testing") != true;
            options.Password.RequireUppercase = env?.IsEnvironment("Testing") != true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = env?.IsEnvironment("Testing") == true ? 0 : 1;
            // Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
            // User settings.
            options.User.RequireUniqueEmail = true;
        });
    }
}