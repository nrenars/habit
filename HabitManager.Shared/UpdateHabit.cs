using System;

namespace HabitManager.Shared
{
    public class UpdateHabitRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public Frequency Frequency { get; set; }
    }
}
