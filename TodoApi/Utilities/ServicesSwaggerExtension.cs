using Microsoft.OpenApi.Models;

namespace TodoApi.Utilities;

public static class ServicesSwaggerExtension
{
    /// <summary>
    /// Configure application services to support swagger api documentation.
    /// </summary>
    public static void AddSwaggerSupport(this IServiceCollection services)
    {
        services.AddSwaggerGen(swaggerOption =>
        {
            var securityDefinition = new OpenApiSecurityScheme
            {
                Name = "Bearer",
                BearerFormat = "JWT",
                Scheme = "bearer",
                Description = "Specify the authorization token.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http
            };
            swaggerOption.AddSecurityDefinition("jwt_auth", securityDefinition);

            // Make sure swagger UI requires a Bearer token specified
            var securityScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "jwt_auth",
                    Type = ReferenceType.SecurityScheme
                }
            };
            var securityRequirements = new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() },
            };
            swaggerOption.AddSecurityRequirement(securityRequirements);
        });
    }
}