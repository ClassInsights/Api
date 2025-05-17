using Microsoft.EntityFrameworkCore;

namespace Api.Models.Database;

public class ClassInsightsContext(DbContextOptions<ClassInsightsContext> options) : DbContext(options)
{
    public virtual DbSet<Class> Classes { get; set; }
    public virtual DbSet<Computer> Computers { get; set; }
    public virtual DbSet<Lesson> Lessons { get; set; }
    public virtual DbSet<Log> Logs { get; set; }
    public virtual DbSet<Room> Rooms { get; set; }
    public virtual DbSet<Subject> Subjects { get; set; }
    public virtual DbSet<User> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>().Property(x => x.Enabled).HasDefaultValue(false);
    }
}