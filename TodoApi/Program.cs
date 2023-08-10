using TodoApi.Data;
using TodoApi.Services;
using TodoApi.Interfaces;
using TodoApi.Utilities;
using DotNetEnv.Configuration;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddScoped<IAuthTokenManagerService, JWTTokenManagerService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerSupport();
builder.Services.AddAuthSupport();
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePages();

app.MapControllers();

app.Run();
