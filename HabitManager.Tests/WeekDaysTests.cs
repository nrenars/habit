using Xunit;

namespace HabitManager.Tests;

public class WeekDaysTests
{
    [Theory]
    [InlineData(DayOfWeek.Monday, WeekDays.Monday)]
    [InlineData(DayOfWeek.Tuesday, WeekDays.Tuesday)]
    [InlineData(DayOfWeek.Wednesday, WeekDays.Wednesday)]
    [InlineData(DayOfWeek.Thursday, WeekDays.Thursday)]
    [InlineData(DayOfWeek.Friday, WeekDays.Friday)]
    [InlineData(DayOfWeek.Saturday, WeekDays.Saturday)]
    [InlineData(DayOfWeek.Sunday, WeekDays.Sunday)]
    public void ToWeekDays_DayOfWeek_ReturnsMatchingFlag(DayOfWeek day, WeekDays expected)
    {
        // Act
        var actual = day.ToWeekDays();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void All_Always_ContainsEveryDayOfWeek()
    {
        // Arrange
        var daysOfWeek = Enum.GetValues<DayOfWeek>();

        // Assert
        Assert.All(daysOfWeek, day => Assert.True(WeekDays.All.HasFlag(day.ToWeekDays())));
    }

    [Fact]
    public void Workdays_Always_DoesNotContainWeekend()
    {
        // Act
        var overlap = WeekDays.Workdays & WeekDays.Weekend;

        // Assert
        Assert.Equal(WeekDays.None, overlap);
    }
}
