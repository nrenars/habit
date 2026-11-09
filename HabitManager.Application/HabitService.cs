using HabitManager.Shared;

namespace HabitManager.Application;

public class HabitService(IHabitRepository repository)
{
    public async Task<IReadOnlyList<HabitDto>> GetAllAsync(string userId, bool includeArchived = false)
    {
        var habits = await repository.GetByUserAsync(userId);
        return habits
            .Where(h => includeArchived || !h.IsArchived())
            .OrderBy(h => h.CreatedAt)
            .Select(h => h.ToDto())
            .ToList();
    }

    public async Task<HabitDto> GetAsync(string userId, Guid id)
        => (await GetOwnedAsync(userId, id)).ToDto();

    public async Task<HabitDto> CreateAsync(string userId, CreateHabitRequest request)
    {
        var habit = new Habit(userId, (request.Name ?? "").Trim(), request.Frequency,
                              request.Note, request.ScheduledDays);
        await repository.AddAsync(habit);
        return habit.ToDto();
    }

    public async Task<HabitDto> UpdateAsync(string userId, Guid id, UpdateHabitRequest request)
    {
        var habit = await GetOwnedAsync(userId, id);
        habit.Update((request.Name ?? "").Trim(), request.Note, request.Frequency, request.ScheduledDays);
        await repository.UpdateAsync(habit);
        return habit.ToDto();
    }

    public async Task<HabitDto> ArchiveAsync(string userId, Guid id)
    {
        var habit = await GetOwnedAsync(userId, id);
        if (!habit.IsArchived())
        {
            habit.Archive();
            await repository.UpdateAsync(habit);
        }
        return habit.ToDto();
    }

    public async Task<HabitCompletionDto> CompleteAsync(string userId, Guid id, DateTime? date)
    {
        var habit = await GetOwnedAsync(userId, id);
        var day = (date ?? DateTime.Today).Date;

        if (day > DateTime.Today)
            throw new ArgumentException("Izpildi nevar atzīmēt nākotnes datumam.");

        var completion = habit.MarkCompleted(day);
        await repository.UpdateAsync(habit);
        return completion.ToDto();
    }

    public async Task<IReadOnlyList<HabitCompletionDto>> GetCompletionsAsync(string userId, Guid id)
    {
        var habit = await GetOwnedAsync(userId, id);
        return habit.Completions
            .OrderByDescending(c => c.Date)
            .Select(c => c.ToDto())
            .ToList();
    }

    // cita lietotāja ieradumu atgriež kā "nav atrasts", lai neatklātu, ka tāds eksistē
    private async Task<Habit> GetOwnedAsync(string userId, Guid id)
    {
        var habit = await repository.GetByIdAsync(id);
        if (habit is null || habit.UserId != userId)
            throw new NotFoundException("Ieradums netika atrasts.");
        return habit;
    }
}
