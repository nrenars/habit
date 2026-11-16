using HabitManager.Shared;
using Xunit;

namespace HabitManager.Tests;

public class HabitDtoTests
{
    [Fact]
    public void IsArchived_ArchivedStatus_ReturnsTrue()
    {
        // Arrange
        var dto = new HabitDto { Status = HabitStatus.Archived };

        // Act + Assert
        Assert.True(dto.IsArchived);
    }

    [Fact]
    public void CanEdit_ArchivedStatus_ReturnsFalse()
    {
        // Arrange
        var dto = new HabitDto { Status = HabitStatus.Archived };

        // Act + Assert
        Assert.False(dto.CanEdit);
    }

    [Fact]
    public void CanEdit_ActiveStatus_ReturnsTrue()
    {
        // Arrange
        var dto = new HabitDto { Status = HabitStatus.Active };

        // Act + Assert
        Assert.True(dto.CanEdit);
    }
}
