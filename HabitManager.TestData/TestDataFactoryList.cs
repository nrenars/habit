using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using HabitManager;

namespace UzdevumuTestData
{
    public class TestDataFactoryList : ITestDataFactory, ISaveLoad
    {
        private List<Habit> testData;

        private string fileName;
            public string FileName { get => fileName; set => fileName = value; }

            public void CreateTestData()
        {
            testData = new List<Habit>();
            testData.Add(new Habit(1, "Fitness", "Pushups, squats, pull-ups"));
            testData.Add(new Habit(2, "Studying", "complete homework"));
        }

        public string ReturnTestData()
        {
            string s = "List" + "\n";
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
            string jsonString = JsonSerializer.Serialize(testData);
            File.WriteAllText(FileName, jsonString);
            return true;
        }

        public bool LoadFromFile()
        {
            if (File.Exists(FileName))
            {
                string jsonString = File.ReadAllText(FileName);
                testData = JsonSerializer.Deserialize<List<Habit>>(jsonString);

            }
            return true;
        }
    }
}
