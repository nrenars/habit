using System;
using System.Collections.Generic;
using System.Text;
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
