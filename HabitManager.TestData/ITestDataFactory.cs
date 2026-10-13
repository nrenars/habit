using HabitManager;

namespace TestData
{
    public interface ITestDataFactory
    {
        void CreateTestData();

        string ReturnTestData();

        IEnumerable<Habit> GetTestData();
    }
}
