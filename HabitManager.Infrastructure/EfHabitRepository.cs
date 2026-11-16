using HabitManager.Application;
using Microsoft.EntityFrameworkCore;

namespace HabitManager.Infrastructure;

public class EfHabitRepository(HabitDbContext db) : IHabitRepository
{
    public async Task<IReadOnlyList<Habit>> GetByUserAsync(string userId)
    {
        var entities = await db.Habits
            .AsNoTracking()
            .Include(h => h.Completions)
            .Where(h => h.UserId == userId)
            .ToListAsync();

        return entities.Select(ToDomain).ToList();
    }

    public async Task<Habit?> GetByIdAsync(Guid id)
    {
        var entity = await db.Habits
            .AsNoTracking()
            .Include(h => h.Completions)
            .FirstOrDefaultAsync(h => h.Id == id);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task AddAsync(Habit habit)
    {
        db.Habits.Add(ToEntity(habit));
        await SaveAsync();
    }

    public async Task UpdateAsync(Habit habit)
    {
        var entity = await db.Habits
            .Include(h => h.Completions)
            .FirstOrDefaultAsync(h => h.Id == habit.Id)
            ?? throw new NotFoundException("Ieradums netika atrasts.");

        entity.Name = habit.Name;
        entity.Note = habit.Note;
        entity.Status = habit.Status;
        entity.Frequency = habit.HabitFrequency;
        entity.ScheduledDays = habit.ScheduledDays;
        entity.EditedAt = habit.EditedAt;

        var existingIds = entity.Completions.Select(c => c.Id).ToHashSet();
        foreach (var completion in habit.Completions.Where(c => !existingIds.Contains(c.Id)))
        {
            db.Completions.Add(new HabitCompletionEntity
            {
                Id = completion.Id,
                HabitId = habit.Id,
                Date = DateOnly.FromDateTime(completion.Date)
            });
        }

        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // DB līmeņa aizsardzība, ja domēna pārbaudei garām paslīd dublikāts
            throw new InvalidOperationException("Šim ieradumam šajā datumā jau ir izpildes ieraksts.");
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }

    private static Habit ToDomain(HabitEntity e)
    {
        var completions = e.Completions
            .Select(c => new HabitCompletion(c.Id, c.HabitId, c.Date.ToDateTime(TimeOnly.MinValue)))
            .ToList();

        return new Habit(e.Id, e.UserId, e.Name, e.Note, e.Status, e.Frequency,
                         e.ScheduledDays, e.CreatedAt, e.EditedAt, completions);
    }

    private static HabitEntity ToEntity(Habit h) => new()
    {
        Id = h.Id,
        UserId = h.UserId,
        Name = h.Name,
        Note = h.Note,
        Status = h.Status,
        Frequency = h.HabitFrequency,
        ScheduledDays = h.ScheduledDays,
        CreatedAt = h.CreatedAt,
        EditedAt = h.EditedAt,
        Completions = h.Completions.Select(c => new HabitCompletionEntity
        {
            Id = c.Id,
            HabitId = h.Id,
            Date = DateOnly.FromDateTime(c.Date)
        }).ToList()
    };
}
