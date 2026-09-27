using System;

namespace HabitManager.Shared
{
    public class HabitCompletionDto
    {
        public Guid Id { get; set; }
        public Guid HabitId { get; set; }
        public DateTime Date { get; set; }
    }
}
