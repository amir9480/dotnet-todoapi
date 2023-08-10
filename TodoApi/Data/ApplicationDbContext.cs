using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
{
    protected readonly IConfiguration Configuration;

    public ApplicationDbContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        base.OnConfiguring(options);
        var databaseName = Environment.GetEnvironmentVariable("DB_NAME");
        options.UseSqlite($"Data Source={databaseName}" ?? throw new InvalidOperationException("DB_NAME environment variable is not set."));
    }
}
