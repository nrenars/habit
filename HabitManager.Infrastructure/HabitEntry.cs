namespace HabitManager.Infrastructure;

public class HabitEntity
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Note { get; set; } = "";
    public HabitStatus Status { get; set; }
    public Frequency Frequency { get; set; }
    public WeekDays ScheduledDays { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime EditedAt { get; set; }

    public List<HabitCompletionEntity> Completions { get; set; } = new();
}

public class HabitCompletionEntity
{
    public Guid Id { get; set; }
    public Guid HabitId { get; set; }
    public DateOnly Date { get; set; }

    public HabitEntity Habit { get; set; } = null!;
}
