using System.Collections.Concurrent;
using HabitManager.Application;

namespace HabitManager.Infrastructure;

public class InMemoryHabitRepository : IHabitRepository
{
    private readonly ConcurrentDictionary<Guid, Habit> _habits = new();

    public InMemoryHabitRepository(IEnumerable<Habit>? seed = null)
    {
        if (seed is null) return;
        foreach (var habit in seed)
            _habits[habit.Id] = habit;
    }

    public Task<IReadOnlyList<Habit>> GetByUserAsync(string userId)
    {
        IReadOnlyList<Habit> result = _habits.Values.Where(h => h.UserId == userId).ToList();
        return Task.FromResult(result);
    }

    public Task<Habit?> GetByIdAsync(Guid id)
        => Task.FromResult(_habits.TryGetValue(id, out var habit) ? habit : null);

    public Task AddAsync(Habit habit)
    {
        _habits[habit.Id] = habit;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Habit habit)
    {
        _habits[habit.Id] = habit;
        return Task.CompletedTask;
    }
}
