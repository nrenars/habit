using TestData;
using Xunit;

namespace HabitManager.Tests;

// Pārbauda, ka testa dati (ko API ielādē tukšā datubāzē) ir derīgi un pietiekami daudzveidīgi,
// lai ar tiem varētu demonstrēt visu funkcionalitāti.
public class TestDataFactoryTests
{
    private static List<Habit> CreateTestHabits()
    {
        var factory = new TestDataFactoryList();
        factory.CreateTestData();
        return factory.GetTestData().ToList();
    }

    [Fact]
    public void CreateTestData_Always_ReturnsHabitsOfMoreThanOneUser()
    {
        // Arrange
        var habits = CreateTestHabits();

        // Act
        var userCount = habits.Select(h => h.UserId).Distinct().Count();

        // Assert
        Assert.True(userCount > 1);
    }

    [Fact]
    public void CreateTestData_Always_ContainsArchivedHabit()
    {
        // Arrange
        var habits = CreateTestHabits();

        // Assert
        Assert.Contains(habits, h => h.IsArchived());
    }

    [Fact]
    public void CreateTestData_Always_ContainsHabitWithSpecificWeekdays()
    {
        // Arrange
        var habits = CreateTestHabits();

        // Assert
        Assert.Contains(habits, h => h.ScheduledDays != WeekDays.All);
    }

    [Fact]
    public void CreateTestData_Always_ContainsHabitWithCompletionHistory()
    {
        // Arrange
        var habits = CreateTestHabits();

        // Assert
        Assert.Contains(habits, h => h.Completions.Count > 0);
    }

    [Fact]
    public void CreateTestData_Always_CompletionsFallOnScheduledDaysOnly()
    {
        // Arrange
        var habits = CreateTestHabits();

        // Assert
        Assert.All(habits, habit =>
            Assert.All(habit.Completions, completion =>
                Assert.True(habit.IsScheduledOn(completion.Date))));
    }

    [Fact]
    public void CreateTestData_Always_GivesEveryHabitUniqueId()
    {
        // Arrange
        var habits = CreateTestHabits();

        // Act
        var distinctIdCount = habits.Select(h => h.Id).Distinct().Count();

        // Assert
        Assert.Equal(habits.Count, distinctIdCount);
    }
}
