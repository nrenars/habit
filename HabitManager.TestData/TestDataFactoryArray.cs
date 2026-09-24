using System;
using System.Collections.Generic;
using System.Text;
using HabitManager;

namespace TestData
{
    public class TestDataFactoryArray : ITestDataFactory
    {
        private Habit[] testData;
        public void CreateTestData()
        {
            testData = new Habit[2];
            testData[0] = new Habit(1, "Yoga", "1h");
            testData[1] = new Habit(2, "Meditation", "10min");
        }

        public string ReturnTestData()
        {
            string s = "Array"+ "\n";
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
