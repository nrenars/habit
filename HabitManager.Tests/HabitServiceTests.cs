using HabitManager.Application;
using HabitManager.Shared;
using Moq;
using Xunit;

namespace HabitManager.Tests;

// IHabitRepository tiek aizstāts ar Moq objektu, tāpēc testiem nav vajadzīga datubāze.
// Setup(...)  -> stub (kontrolē, ko atgriež atkarība)
// Verify(...) -> mock (pārbauda, vai serviss atkarību izsauca)
public class HabitServiceTests
{
    private const string UserId = "user-1";
    private const string OtherUserId = "user-2";
    private const string HabitName = "Yoga";
    private const string NewName = "Reading";

    // pirmdiena; serviss "domā", ka šodien ir šis datums (skat. FixedTimeProvider)
    private static readonly DateTime Today = new(2026, 11, 16);

    private static Habit CreateHabit(string userId = UserId, WeekDays days = WeekDays.All, string name = HabitName)
        => new(userId, name, Frequency.Daily, string.Empty, days);

    private static HabitService CreateService(Mock<IHabitRepository> repository)
        => new(repository.Object, new FixedTimeProvider(Today));

    private static Mock<IHabitRepository> RepositoryReturning(params Habit[] habits)
    {
        var repository = new Mock<IHabitRepository>();
        repository.Setup(r => r.GetByUserAsync(UserId)).ReturnsAsync((IReadOnlyList<Habit>)habits);
        foreach (var habit in habits)
            repository.Setup(r => r.GetByIdAsync(habit.Id)).ReturnsAsync(habit);
        return repository;
    }

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_ArchivedHabitExists_ReturnsOnlyActiveByDefault()
    {
        // Arrange
        var active = CreateHabit();
        var archived = CreateHabit(name: NewName);
        archived.Archive();
        var service = CreateService(RepositoryReturning(active, archived));

        // Act
        var result = await service.GetAllAsync(UserId);

        // Assert
        var dto = Assert.Single(result);
        Assert.Equal(active.Id, dto.Id);
    }

    [Fact]
    public async Task GetAllAsync_IncludeArchivedRequested_ReturnsAllHabits()
    {
        // Arrange
        var active = CreateHabit();
        var archived = CreateHabit(name: NewName);
        archived.Archive();
        var service = CreateService(RepositoryReturning(active, archived));

        // Act
        var result = await service.GetAllAsync(UserId, includeArchived: true);

        // Assert
        Assert.Equal(2, result.Count);
    }

    // ---------- GetAsync ----------

    [Fact]
    public async Task GetAsync_HabitExists_ReturnsThatHabit()
    {
        // Arrange
        var habit = CreateHabit();
        var service = CreateService(RepositoryReturning(habit));

        // Act
        var result = await service.GetAsync(UserId, habit.Id);

        // Assert
        Assert.Equal(habit.Id, result.Id);
    }

    [Fact]
    public async Task GetAsync_HabitDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var unknownId = Guid.NewGuid();
        var repository = new Mock<IHabitRepository>();
        repository.Setup(r => r.GetByIdAsync(unknownId)).ReturnsAsync((Habit?)null);
        var service = CreateService(repository);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(UserId, unknownId));
    }

    [Fact]
    public async Task GetAsync_HabitBelongsToOtherUser_ThrowsNotFoundException()
    {
        // Arrange
        var habitOfOtherUser = CreateHabit(OtherUserId);
        var service = CreateService(RepositoryReturning(habitOfOtherUser));

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(UserId, habitOfOtherUser.Id));
    }

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesHabitForGivenUser()
    {
        // Arrange
        var repository = new Mock<IHabitRepository>();
        var service = CreateService(repository);
        var request = new CreateHabitRequest
        {
            Name = HabitName,
            Frequency = Frequency.Daily,
            ScheduledDays = WeekDays.Weekend
        };

        // Act
        await service.CreateAsync(UserId, request);

        // Assert
        repository.Verify(r => r.AddAsync(It.Is<Habit>(h =>
            h.UserId == UserId &&
            h.Name == HabitName &&
            h.ScheduledDays == WeekDays.Weekend)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NameWithSurroundingSpaces_TrimsName()
    {
        // Arrange
        var service = CreateService(new Mock<IHabitRepository>());
        var request = new CreateHabitRequest { Name = $"  {HabitName}  ", Frequency = Frequency.Daily };

        // Act
        var result = await service.CreateAsync(UserId, request);

        // Assert
        Assert.Equal(HabitName, result.Name);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsArgumentExceptionAndSavesNothing()
    {
        // Arrange
        var repository = new Mock<IHabitRepository>();
        var service = CreateService(repository);
        var request = new CreateHabitRequest { Name = string.Empty, Frequency = Frequency.Daily };

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(UserId, request));

        // Assert
        repository.Verify(r => r.AddAsync(It.IsAny<Habit>()), Times.Never);
    }

    // ---------- UpdateAsync ----------

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedHabit()
    {
        // Arrange
        var habit = CreateHabit();
        var service = CreateService(RepositoryReturning(habit));
        var request = new UpdateHabitRequest { Name = NewName, Frequency = Frequency.Weekly, ScheduledDays = WeekDays.Monday };

        // Act
        var result = await service.UpdateAsync(UserId, habit.Id, request);

        // Assert
        Assert.Equal(NewName, result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_SavesChanges()
    {
        // Arrange
        var habit = CreateHabit();
        var repository = RepositoryReturning(habit);
        var service = CreateService(repository);
        var request = new UpdateHabitRequest { Name = NewName, Frequency = Frequency.Weekly, ScheduledDays = WeekDays.Monday };

        // Act
        await service.UpdateAsync(UserId, habit.Id, request);

        // Assert
        repository.Verify(r => r.UpdateAsync(habit), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ArchivedHabit_ThrowsInvalidOperationAndSavesNothing()
    {
        // Arrange
        var habit = CreateHabit();
        habit.Archive();
        var repository = RepositoryReturning(habit);
        var service = CreateService(repository);
        var request = new UpdateHabitRequest { Name = NewName, Frequency = Frequency.Daily, ScheduledDays = WeekDays.All };

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(UserId, habit.Id, request));

        // Assert
        repository.Verify(r => r.UpdateAsync(It.IsAny<Habit>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_HabitDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var service = CreateService(new Mock<IHabitRepository>());
        var request = new UpdateHabitRequest { Name = NewName, Frequency = Frequency.Daily, ScheduledDays = WeekDays.All };

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(UserId, Guid.NewGuid(), request));
    }

    // ---------- ArchiveAsync ----------

    [Fact]
    public async Task ArchiveAsync_ActiveHabit_ReturnsArchivedHabit()
    {
        // Arrange
        var habit = CreateHabit();
        var service = CreateService(RepositoryReturning(habit));

        // Act
        var result = await service.ArchiveAsync(UserId, habit.Id);

        // Assert
        Assert.Equal(HabitStatus.Archived, result.Status);
    }

    [Fact]
    public async Task ArchiveAsync_ActiveHabit_SavesChanges()
    {
        // Arrange
        var habit = CreateHabit();
        var repository = RepositoryReturning(habit);
        var service = CreateService(repository);

        // Act
        await service.ArchiveAsync(UserId, habit.Id);

        // Assert
        repository.Verify(r => r.UpdateAsync(habit), Times.Once);
    }

    [Fact]
    public async Task ArchiveAsync_AlreadyArchivedHabit_DoesNotSaveAgain()
    {
        // Arrange
        var habit = CreateHabit();
        habit.Archive();
        var repository = RepositoryReturning(habit);
        var service = CreateService(repository);

        // Act
        await service.ArchiveAsync(UserId, habit.Id);

        // Assert
        repository.Verify(r => r.UpdateAsync(It.IsAny<Habit>()), Times.Never);
    }

    // ---------- CompleteAsync ----------

    [Fact]
    public async Task CompleteAsync_NoDateGiven_CompletesHabitForToday()
    {
        // Arrange
        var habit = CreateHabit();
        var service = CreateService(RepositoryReturning(habit));

        // Act
        var completion = await service.CompleteAsync(UserId, habit.Id, null);

        // Assert
        Assert.Equal(Today, completion.Date);
    }

    [Fact]
    public async Task CompleteAsync_PastDateGiven_CompletesHabitForThatDate()
    {
        // Arrange
        var habit = CreateHabit();
        var pastDate = Today.AddDays(-2);
        var service = CreateService(RepositoryReturning(habit));

        // Act
        var completion = await service.CompleteAsync(UserId, habit.Id, pastDate);

        // Assert
        Assert.Equal(pastDate, completion.Date);
    }

    [Fact]
    public async Task CompleteAsync_ValidRequest_SavesChanges()
    {
        // Arrange
        var habit = CreateHabit();
        var repository = RepositoryReturning(habit);
        var service = CreateService(repository);

        // Act
        await service.CompleteAsync(UserId, habit.Id, null);

        // Assert
        repository.Verify(r => r.UpdateAsync(habit), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_FutureDate_ThrowsArgumentException()
    {
        // Arrange
        var habit = CreateHabit();
        var service = CreateService(RepositoryReturning(habit));
        var tomorrow = Today.AddDays(1);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CompleteAsync(UserId, habit.Id, tomorrow));
    }

    [Fact]
    public async Task CompleteAsync_AlreadyCompletedToday_ThrowsInvalidOperationAndSavesNothing()
    {
        // Arrange
        var habit = CreateHabit();
        habit.MarkCompleted(Today);
        var repository = RepositoryReturning(habit);
        var service = CreateService(repository);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CompleteAsync(UserId, habit.Id, null));

        // Assert
        repository.Verify(r => r.UpdateAsync(It.IsAny<Habit>()), Times.Never);
    }

    [Fact]
    public async Task CompleteAsync_TodayNotInSchedule_ThrowsInvalidOperationException()
    {
        // Arrange: šodien ir pirmdiena, ieradums ir tikai otrdienās
        var habit = CreateHabit(days: WeekDays.Tuesday);
        var service = CreateService(RepositoryReturning(habit));

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CompleteAsync(UserId, habit.Id, null));
    }

    [Fact]
    public async Task CompleteAsync_ArchivedHabit_ThrowsInvalidOperationException()
    {
        // Arrange
        var habit = CreateHabit();
        habit.Archive();
        var service = CreateService(RepositoryReturning(habit));

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CompleteAsync(UserId, habit.Id, null));
    }

    [Fact]
    public async Task CompleteAsync_HabitBelongsToOtherUser_ThrowsNotFoundException()
    {
        // Arrange
        var habitOfOtherUser = CreateHabit(OtherUserId);
        var service = CreateService(RepositoryReturning(habitOfOtherUser));

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.CompleteAsync(UserId, habitOfOtherUser.Id, null));
    }

    // ---------- GetCompletionsAsync ----------

    [Fact]
    public async Task GetCompletionsAsync_HabitHasCompletions_ReturnsNewestFirst()
    {
        // Arrange
        var habit = CreateHabit();
        var older = Today.AddDays(-2);
        habit.MarkCompleted(older);
        habit.MarkCompleted(Today);
        var service = CreateService(RepositoryReturning(habit));

        // Act
        var result = await service.GetCompletionsAsync(UserId, habit.Id);

        // Assert
        Assert.Equal(new[] { Today, older }, result.Select(c => c.Date));
    }
}
