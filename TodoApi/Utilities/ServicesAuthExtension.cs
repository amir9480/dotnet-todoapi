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
                        var userIdClaim = context.Principal?.FindAll(ClaimTypes.NameIdentifier).Where(f => f.Value != "JWTServiceAccessToken").First();
                        if (userIdClaim == null)
                        {
                            context.Fail("User ID claim not found in the token.");
                        }
                        else
                        {
                            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                            var user = await userManager.FindByIdAsync(userIdClaim.Value);
                            if (user == null)
                            {
                                context.Fail("User not found in the database.");
                            }
                        }
                    }
                };
                var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
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
                        Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY") ?? "")
                    ),
                };
            });

        services.Configure<IdentityOptions>(options =>
        {
            // Password settings.
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings.
            options.User.RequireUniqueEmail = true;
        });
    }
}
