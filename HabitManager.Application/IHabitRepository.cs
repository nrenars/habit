namespace HabitManager.Application;

public interface IHabitRepository
{
    Task<IReadOnlyList<Habit>> GetByUserAsync(string userId);
    Task<Habit?> GetByIdAsync(Guid id);
    Task AddAsync(Habit habit);
    Task UpdateAsync(Habit habit);
}
