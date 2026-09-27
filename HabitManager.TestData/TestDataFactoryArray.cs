using System;
using System.Collections.Generic;
using HabitManager;

namespace TestData
{
    public class TestDataFactoryArray : ITestDataFactory
    {
        private Habit[] testData;

        public void CreateTestData()
        {
            testData = new Habit[4];

            testData[0] = new Habit("demo-user-1", "Yoga", Frequency.Daily, "1h no rīta");
            testData[0].MarkCompleted(DateTime.Today);
            testData[0].MarkCompleted(DateTime.Today.AddDays(-1));

            testData[1] = new Habit("demo-user-1", "Reading", Frequency.Weekly, "30 min pirms gulētiešanas");

            testData[2] = new Habit("demo-user-2", "Gym", Frequency.Weekly, "Main Street 1");
            testData[2].Archive();

            testData[3] = new Habit("demo-user-2", "Meditation", Frequency.Daily);
            testData[3].MarkCompleted(DateTime.Today.AddDays(-2));
        }

        public string ReturnTestData()
        {
            var s = "Array" + "\n";
            foreach (var habit in testData)
            {
                s += habit.ToString() + "\n";
            }
            return s;
        }

        public IEnumerable<Habit> GetTestData()
        {
            return testData;
        }
    }
}
