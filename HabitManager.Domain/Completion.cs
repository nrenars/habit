using System.Text.Json.Serialization;

namespace HabitManager;

public class HabitCompletion
{
    public Guid Id { get; }
    public Guid HabitId { get; }
    public DateTime Date { get; }

    public HabitCompletion(Guid habitId, DateTime date)
    {
        Id = Guid.NewGuid();
        HabitId = habitId;
        Date = date.Date;
    }

    [JsonConstructor]
    public HabitCompletion(Guid id, Guid habitId, DateTime date)
    {
        Id = id;
        HabitId = habitId;
        Date = date.Date;
    }
}
