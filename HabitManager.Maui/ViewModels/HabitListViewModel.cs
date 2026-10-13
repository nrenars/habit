using TestData;

namespace HabitManager.Maui.ViewModels;

public class HabitListViewModel
{
    public IEnumerable<Habit> Habits { get; }

    public HabitListViewModel()
    {
        var factory = new TestDataFactoryArray();
        factory.CreateTestData();
        Habits = factory.GetTestData();
    }
}
