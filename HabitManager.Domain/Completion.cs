using System;
using System.Text.Json.Serialization;

namespace HabitManager
{
    public class HabitCompletion
    {
        public Guid Id { get; }
        public Guid HabitId { get; }
        public DateTime Date { get; }

        public HabitCompletion(Guid habitId, DateTime date) : this(Guid.NewGuid(), habitId, date) { }

        [JsonConstructor]
        public HabitCompletion(Guid id, Guid habitId, DateTime date)
        {
            Id = id;
            HabitId = habitId;
            Date = date.Date;
        }

        public override string ToString() => $"Habit {HabitId} completed on {Date:yyyy-MM-dd}";
    }
}
