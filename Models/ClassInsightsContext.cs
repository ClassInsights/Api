using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using System.Reflection.Emit;

namespace Api.Models;

public class ClassInsightsContext : DbContext
{
    public ClassInsightsContext(DbContextOptions<ClassInsightsContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }

    public DbSet<DbModels.TabUsers> TabUsers { get; set; } = null!;
    public DbSet<DbModels.TabLessons> TabLessons { get; set; } = null!;
    public DbSet<DbModels.TabRooms> TabRooms { get; set; } = null!;
    public DbSet<DbModels.TabComputers> TabComputers { get; set; } = null!;
    public DbSet<DbModels.TabClasses> TabClasses { get; set; } = null!;
    public DbSet<DbModels.TabGroups> TabGroups { get; set; } = null!;
}
