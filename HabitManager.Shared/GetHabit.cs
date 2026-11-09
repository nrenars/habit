using System.Text.Json.Serialization;

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
        public WeekDays ScheduledDays { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime EditedAt { get; set; }
        public int CompletionCount { get; set; }
        public bool IsCompletedToday { get; set; }
        public bool IsScheduledToday { get; set; }
        public bool CanCompleteToday { get; set; }

        [JsonIgnore] public bool IsArchived => Status == HabitStatus.Archived;
        [JsonIgnore] public bool CanEdit => !IsArchived;
    }
}
