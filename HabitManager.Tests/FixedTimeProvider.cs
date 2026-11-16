namespace HabitManager.Tests;

// Stub: ļauj testam pilnībā kontrolēt "šodienu" (skat. HabitService.TimeProvider)
internal sealed class FixedTimeProvider(DateTime localNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow()
        => new(DateTime.SpecifyKind(localNow, DateTimeKind.Utc));

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
