using Microsoft.EntityFrameworkCore;

namespace Api.Models;

public class ClassInsightsContext : DbContext
{
    public ClassInsightsContext(DbContextOptions<ClassInsightsContext> options) : base(options)
        {
        }

        public DbSet<DbModels.TabUsers> TabUsers { get; set; } = null!;
        public DbSet<DbModels.TabLessons> TabLessons { get; set; } = null!;
        public DbSet<DbModels.TabRooms> TabRooms { get; set; } = null!;
        public DbSet<DbModels.TabComputers> TabComputers { get; set; } = null!;
}
