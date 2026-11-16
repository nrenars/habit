using Xunit;

namespace HabitManager.Tests;

public class HabitTests
{
    private const string UserId = "user-1";
    private const string ValidName = "Yoga";
    private const string NewName = "Reading";
    private const string NewNote = "Every evening";
    private const int MaxNameLength = 100;

    // fiksēti datumi, lai testi nav atkarīgi no tā, kad tie tiek palaisti
    private static readonly DateTime Monday = new(2026, 11, 16);
    private static readonly DateTime Tuesday = new(2026, 11, 17);

    private static Habit CreateHabit(WeekDays days = WeekDays.All, string name = ValidName)
        => new(UserId, name, Frequency.Daily, string.Empty, days);

    // ---------- konstruktors ----------

    [Fact]
    public void Constructor_ValidArguments_CreatesActiveHabit()
    {
        // Act
        var habit = CreateHabit();

        // Assert
        Assert.Equal(HabitStatus.Active, habit.Status);
    }

    [Fact]
    public void Constructor_NoDaysSpecified_SchedulesAllDays()
    {
        // Act
        var habit = new Habit(UserId, ValidName, Frequency.Daily);

        // Assert
        Assert.Equal(WeekDays.All, habit.ScheduledDays);
    }

    [Fact]
    public void Constructor_NullNote_StoresEmptyNote()
    {
        // Act
        var habit = new Habit(UserId, ValidName, Frequency.Daily, null!);

        // Assert
        Assert.Equal(string.Empty, habit.Note);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankName_ThrowsArgumentException(string name)
    {
        // Act + Assert
        Assert.Throws<ArgumentException>(() => CreateHabit(name: name));
    }

    [Fact]
    public void Constructor_NameLongerThanMaximum_ThrowsArgumentException()
    {
        // Arrange
        var tooLongName = new string('a', MaxNameLength + 1);

        // Act + Assert
        Assert.Throws<ArgumentException>(() => CreateHabit(name: tooLongName));
    }

    [Fact]
    public void Constructor_NameOfMaximumLength_KeepsName()
    {
        // Arrange
        var maxLengthName = new string('a', MaxNameLength);

        // Act
        var habit = CreateHabit(name: maxLengthName);

        // Assert
        Assert.Equal(maxLengthName, habit.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankUserId_ThrowsArgumentException(string userId)
    {
        // Act + Assert
        Assert.Throws<ArgumentException>(() => new Habit(userId, ValidName, Frequency.Daily));
    }

    [Theory]
    [InlineData(WeekDays.None)]
    [InlineData((WeekDays)128)]
    public void Constructor_InvalidDays_ThrowsArgumentException(WeekDays days)
    {
        // Act + Assert
        Assert.Throws<ArgumentException>(() => CreateHabit(days));
    }

    // ---------- grafiks (nedēļas dienas) ----------

    [Theory]
    [InlineData(0, true)]   // pirmdiena
    [InlineData(1, false)]  // otrdiena
    [InlineData(2, true)]   // trešdiena
    [InlineData(3, false)]  // ceturtdiena
    [InlineData(4, true)]   // piektdiena
    [InlineData(5, false)]  // sestdiena
    [InlineData(6, false)]  // svētdiena
    public void IsScheduledOn_MondayWednesdayFridaySchedule_MatchesOnlyScheduledDays(int daysAfterMonday, bool expected)
    {
        // Arrange
        var habit = CreateHabit(WeekDays.Monday | WeekDays.Wednesday | WeekDays.Friday);

        // Act
        var actual = habit.IsScheduledOn(Monday.AddDays(daysAfterMonday));

        // Assert
        Assert.Equal(expected, actual);
    }

    // ---------- izpildes atzīmēšana ----------

    [Fact]
    public void MarkCompleted_ScheduledDay_RecordsCompletionForThatDate()
    {
        // Arrange
        var habit = CreateHabit();

        // Act
        var completion = habit.MarkCompleted(Monday);

        // Assert
        Assert.Equal(Monday, completion.Date);
    }

    [Fact]
    public void MarkCompleted_ScheduledDay_MarksDateAsCompleted()
    {
        // Arrange
        var habit = CreateHabit();

        // Act
        habit.MarkCompleted(Monday);

        // Assert
        Assert.True(habit.IsCompletedOn(Monday));
    }

    [Fact]
    public void MarkCompleted_DateWithTimeOfDay_StoresDateWithoutTime()
    {
        // Arrange
        var habit = CreateHabit();

        // Act
        var completion = habit.MarkCompleted(Monday.AddHours(15));

        // Assert
        Assert.Equal(Monday, completion.Date);
    }

    [Fact]
    public void MarkCompleted_ArchivedHabit_ThrowsInvalidOperationException()
    {
        // Arrange
        var habit = CreateHabit();
        habit.Archive();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => habit.MarkCompleted(Monday));
    }

    [Fact]
    public void MarkCompleted_SameDateTwice_ThrowsInvalidOperationException()
    {
        // Arrange
        var habit = CreateHabit();
        habit.MarkCompleted(Monday);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => habit.MarkCompleted(Monday));
    }

    [Fact]
    public void MarkCompleted_DayOutsideSchedule_ThrowsInvalidOperationException()
    {
        // Arrange
        var habit = CreateHabit(WeekDays.Monday);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => habit.MarkCompleted(Tuesday));
    }

    // ---------- labošana ----------

    [Fact]
    public void Update_ValidValues_ReplacesAllEditableFields()
    {
        // Arrange
        var habit = CreateHabit();

        // Act
        habit.Update(NewName, NewNote, Frequency.Weekly, WeekDays.Weekend);

        // Assert
        Assert.Equal(NewName, habit.Name);
        Assert.Equal(NewNote, habit.Note);
        Assert.Equal(Frequency.Weekly, habit.HabitFrequency);
        Assert.Equal(WeekDays.Weekend, habit.ScheduledDays);
    }

    [Fact]
    public void Update_InvalidName_LeavesHabitUnchanged()
    {
        // Arrange
        var habit = CreateHabit(WeekDays.Monday);

        // Act
        Assert.Throws<ArgumentException>(() =>
            habit.Update(string.Empty, NewNote, Frequency.Monthly, WeekDays.Friday));

        // Assert: nekas nav izmainīts pat daļēji
        Assert.Equal(ValidName, habit.Name);
        Assert.Equal(WeekDays.Monday, habit.ScheduledDays);
    }

    [Fact]
    public void Update_NoDaysSelected_ThrowsArgumentException()
    {
        // Arrange
        var habit = CreateHabit();

        // Act + Assert
        Assert.Throws<ArgumentException>(() =>
            habit.Update(NewName, NewNote, Frequency.Daily, WeekDays.None));
    }

    [Fact]
    public void Update_ArchivedHabit_ThrowsInvalidOperationException()
    {
        // Arrange
        var habit = CreateHabit();
        habit.Archive();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() =>
            habit.Update(NewName, NewNote, Frequency.Daily, WeekDays.All));
    }

    // ---------- statuss ----------

    [Fact]
    public void Archive_ActiveHabit_SetsArchivedStatus()
    {
        // Arrange
        var habit = CreateHabit();

        // Act
        habit.Archive();

        // Assert
        Assert.Equal(HabitStatus.Archived, habit.Status);
    }

    [Fact]
    public void Archive_ActiveHabit_DisablesEditing()
    {
        // Arrange
        var habit = CreateHabit();

        // Act
        habit.Archive();

        // Assert
        Assert.False(habit.CanEdit);
    }

    [Fact]
    public void Restore_ArchivedHabit_SetsActiveStatus()
    {
        // Arrange
        var habit = CreateHabit();
        habit.Archive();

        // Act
        habit.Restore();

        // Assert
        Assert.Equal(HabitStatus.Active, habit.Status);
    }

    // ---------- CanCompleteToday ----------
    // Šie testi lieto DateTime.Today (domēna kods to lasa tieši), tāpēc ieradumam tiek
    // dots grafiks "visas dienas", lai rezultāts nebūtu atkarīgs no nedēļas dienas.

    [Fact]
    public void CanCompleteToday_ActiveHabitNotYetCompleted_ReturnsTrue()
    {
        // Arrange
        var habit = CreateHabit(WeekDays.All);

        // Act + Assert
        Assert.True(habit.CanCompleteToday);
    }

    [Fact]
    public void CanCompleteToday_AlreadyCompletedToday_ReturnsFalse()
    {
        // Arrange
        var habit = CreateHabit(WeekDays.All);
        habit.MarkCompleted(DateTime.Today);

        // Act + Assert
        Assert.False(habit.CanCompleteToday);
    }

    [Fact]
    public void CanCompleteToday_ArchivedHabit_ReturnsFalse()
    {
        // Arrange
        var habit = CreateHabit(WeekDays.All);
        habit.Archive();

        // Act + Assert
        Assert.False(habit.CanCompleteToday);
    }
}
