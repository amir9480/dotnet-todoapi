using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TodoApi.Interfaces;
using TodoApi.Models;

namespace TodoApi.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
        ChangeTracker.Tracked += OnEntityTracked;
        ChangeTracker.StateChanged += OnEntityStateChanged;
    }

    public DbSet<TodoItem> TodoItems { get; set; }

    private static void OnEntityTracked(object? sender, EntityTrackedEventArgs trackedEvent)
    {
        if (trackedEvent.FromQuery || trackedEvent.Entry.State != EntityState.Added ||
            trackedEvent.Entry.Entity is not IEntityTimestamps entity) return;
        entity.CreatedDate = DateTime.Now;
        entity.UpdatedDate = DateTime.Now;
    }

    private static void OnEntityStateChanged(object? sender, EntityStateChangedEventArgs changedEvent)
    {
        if (changedEvent.NewState == EntityState.Modified && changedEvent.Entry.Entity is IEntityTimestamps entity)
            entity.UpdatedDate = DateTime.Now;
    }
}