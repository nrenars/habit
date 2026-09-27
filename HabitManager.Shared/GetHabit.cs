using System;

namespace HabitManager.Shared
{
    public class HabitDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public HabitStatus Status { get; set; }
        public Frequency Frequency { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime EditedAt { get; set; }
        public bool IsCompletedToday { get; set; }
    }
}
