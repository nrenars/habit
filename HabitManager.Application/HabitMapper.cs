using HabitManager.Shared;

namespace HabitManager.Application;

public static class HabitMapper
{
    public static HabitDto ToDto(this Habit habit) => new()
    {
        Id = habit.Id,
        UserId = habit.UserId,
        Name = habit.Name,
        Note = habit.Note,
        Status = habit.Status,
        Frequency = habit.HabitFrequency,
        ScheduledDays = habit.ScheduledDays,
        CreatedAt = habit.CreatedAt,
        EditedAt = habit.EditedAt,        
        CompletionCount = habit.Completions.Count,
        IsCompletedToday = habit.IsCompletedToday(),
        IsScheduledToday = habit.IsScheduledOn(DateTime.Today),
        CanCompleteToday = habit.CanCompleteToday
    };

    public static HabitCompletionDto ToDto(this HabitCompletion completion) => new()
    {
        Id = completion.Id,
        HabitId = completion.HabitId,
        Date = completion.Date
    };
}
