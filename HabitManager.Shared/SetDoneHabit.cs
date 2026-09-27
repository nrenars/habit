using System;

namespace HabitManager.Shared
{
    public class MarkHabitCompletedRequest
    {
        public Guid HabitId { get; set; }

        public DateTime? Date { get; set; }
    }
}
