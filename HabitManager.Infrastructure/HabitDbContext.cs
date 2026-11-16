using Microsoft.EntityFrameworkCore;

namespace HabitManager.Infrastructure;

public class HabitDbContext(DbContextOptions<HabitDbContext> options) : DbContext(options)
{
    public DbSet<HabitEntity> Habits => Set<HabitEntity>();
    public DbSet<HabitCompletionEntity> Completions => Set<HabitCompletionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HabitEntity>(e =>
        {
            e.ToTable("Habits");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.UserId).IsRequired().HasMaxLength(100);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Frequency).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.UserId);

            e.HasMany(x => x.Completions)
                .WithOne(c => c.Habit)
                .HasForeignKey(c => c.HabitId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HabitCompletionEntity>(e =>
        {
            e.ToTable("HabitCompletions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();

            e.HasIndex(x => new { x.HabitId, x.Date }).IsUnique();
        });
    }
}
