using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

namespace TodoApi.Tests;

public class DbContextFixture : IDisposable
{
    public ApplicationDbContext DbContext { get; }

    public DbContextFixture()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        DbContext = new ApplicationDbContext(options);
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}
