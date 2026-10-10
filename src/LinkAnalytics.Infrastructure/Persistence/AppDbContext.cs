using LinkAnalytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkAnalytics.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Link> Links => Set<Link>();
    public DbSet<LinkCountryClick> LinkCountryClicks => Set<LinkCountryClick>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}