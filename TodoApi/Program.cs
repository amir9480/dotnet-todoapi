using TodoApi.Data;
using TodoApi.Services;
using TodoApi.Interfaces;
using TodoApi.Utilities;
using DotNetEnv.Configuration;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());

builder.Services.AddControllers();
builder.Services.AddScoped<IAuthTokenManagerService, JWTTokenManagerService>();
builder.Services.AddScoped<ITodoService, DatabaseTodoService>();

if (builder.Environment.IsEnvironment("Testing") == false)
{
    string databaseName = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new InvalidOperationException("DB_NAME environment variable is not set.");

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite($"Data Source={databaseName}"));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerSupport();
    builder.Services.AddAuthSupport();
}

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

public partial class Program { }
