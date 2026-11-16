using HabitManager.Application;
using Xunit;

namespace HabitManager.Tests;

public class HabitMapperTests
{
    private const string UserId = "user-1";
    private const string HabitName = "Yoga";

    private static readonly DateTime Monday = new(2026, 11, 16);
    private static readonly DateTime Tuesday = new(2026, 11, 17);

    [Fact]
    public void ToDto_Habit_CopiesNameFrequencyAndSchedule()
    {
        // Arrange
        var habit = new Habit(UserId, HabitName, Frequency.Weekly, string.Empty, WeekDays.Weekend);

        // Act
        var dto = habit.ToDto();

        // Assert
        Assert.Equal(HabitName, dto.Name);
        Assert.Equal(Frequency.Weekly, dto.Frequency);
        Assert.Equal(WeekDays.Weekend, dto.ScheduledDays);
    }

    [Fact]
    public void ToDto_HabitWithTwoCompletions_ReportsCompletionCount()
    {
        // Arrange
        var habit = new Habit(UserId, HabitName, Frequency.Daily);
        habit.MarkCompleted(Monday);
        habit.MarkCompleted(Tuesday);

        // Act
        var dto = habit.ToDto();

        // Assert
        Assert.Equal(2, dto.CompletionCount);
    }

    [Fact]
    public void ToDto_ArchivedHabit_CannotBeCompletedToday()
    {
        // Arrange
        var habit = new Habit(UserId, HabitName, Frequency.Daily);
        habit.Archive();

        // Act
        var dto = habit.ToDto();

        // Assert
        Assert.False(dto.CanCompleteToday);
    }

    [Fact]
    public void ToDto_Completion_CopiesHabitIdAndDate()
    {
        // Arrange
        var habit = new Habit(UserId, HabitName, Frequency.Daily);
        var completion = habit.MarkCompleted(Monday);

        // Act
        var dto = completion.ToDto();

        // Assert
        Assert.Equal(habit.Id, dto.HabitId);
        Assert.Equal(Monday, dto.Date);
    }
}
