using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using HabitManager;

namespace TestData
{
    public class TestDataFactoryList : ITestDataFactory, ISaveLoad
    {
        private List<Habit> testData;

        private string fileName = "testdata.json";
        public string FileName { get => fileName; set => fileName = value; }

        public void CreateTestData()
        {
            testData = new List<Habit>();

            var fitness = new Habit("demo-user-1", "Fitness", Frequency.Daily, "Pushups, squats, pull-ups");
            fitness.MarkCompleted(DateTime.Today);
            fitness.MarkCompleted(DateTime.Today.AddDays(-1));
            testData.Add(fitness);

            testData.Add(new Habit("demo-user-1", "Studying", Frequency.Weekly, "Complete homework"));

            var earlyWakeUp = new Habit("demo-user-2", "Early wake-up", Frequency.Daily);
            earlyWakeUp.Archive();
            testData.Add(earlyWakeUp);

            var reading = new Habit("demo-user-2", "Reading", Frequency.Monthly, "One book per month");
            reading.MarkCompleted(DateTime.Today.AddDays(-3));
            testData.Add(reading);
        }

        public string ReturnTestData()
        {
            var s = "List" + "\n";
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

        public bool SaveToFile()
        {
            var json = JsonSerializer.Serialize(testData);
            File.WriteAllText(FileName, json);
            return true;
        }

        public bool LoadFromFile()
        {
            if (!File.Exists(FileName))
                return false;

            var json = File.ReadAllText(FileName);
            testData = JsonSerializer.Deserialize<List<Habit>>(json) ?? new List<Habit>();
            return true;
        }
    }
}
