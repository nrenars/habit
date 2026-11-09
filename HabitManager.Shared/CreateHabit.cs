namespace HabitManager.Shared
{
    public class CreateHabitRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public Frequency Frequency { get; set; }
        public WeekDays ScheduledDays { get; set; } = WeekDays.All;
    }
}
