
using Microsoft.OpenApi.Models;

namespace TodoApi.Utilities;

public static class ServicesSwaggerExtension
{
    /// <summary>
    /// Configure application services to support swagger api documentation.
    /// </summary>
    public static void AddSwaggerSupport(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            OpenApiSecurityScheme securityDefinition = new OpenApiSecurityScheme()
            {
                Name = "Bearer",
                BearerFormat = "JWT",
                Scheme = "bearer",
                Description = "Specify the authorization token.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
            };
            c.AddSecurityDefinition("jwt_auth", securityDefinition);

            // Make sure swagger UI requires a Bearer token specified
            OpenApiSecurityScheme securityScheme = new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference()
                {
                    Id = "jwt_auth",
                    Type = ReferenceType.SecurityScheme
                }
            };
            OpenApiSecurityRequirement securityRequirements = new OpenApiSecurityRequirement()
            {
                {securityScheme, new string[] { }},
            };
            c.AddSecurityRequirement(securityRequirements);
        });
    }
}
