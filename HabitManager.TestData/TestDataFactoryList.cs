using System.Text.Json;
using HabitManager;

namespace TestData
{
    public class TestDataFactoryList : ITestDataFactory, ISaveLoad
    {
        private List<Habit> testData = new();

        private string fileName = "testdata.json";
        public string FileName { get => fileName; set => fileName = value; }

        public void CreateTestData()
        {
            testData = new List<Habit>();

            var yoga = new Habit("demo-user-1", "Yoga", Frequency.Daily, "1h no rīta");
            CompleteRecentScheduledDays(yoga, 5);

            var gym = new Habit("demo-user-1", "Gym", Frequency.Weekly, "Spēka treniņš",
                WeekDays.Monday | WeekDays.Wednesday | WeekDays.Friday);
            CompleteRecentScheduledDays(gym, 4);

            var studying = new Habit("demo-user-1", "Studying", Frequency.Weekly, "Mājasdarbi",
                WeekDays.Tuesday | WeekDays.Thursday);
            CompleteRecentScheduledDays(studying, 2);

            var meditation = new Habit("demo-user-1", "Meditation", Frequency.Daily, "10 min", WeekDays.Workdays);
            CompleteRecentScheduledDays(meditation, 3);

            var reading = new Habit("demo-user-1", "Reading", Frequency.Daily, "30 min pirms gulētiešanas");
            reading.MarkCompleted(DateTime.Today.AddDays(-1));
            reading.MarkCompleted(DateTime.Today.AddDays(-3));

            var earlyWakeUp = new Habit("demo-user-1", "Early wake-up", Frequency.Daily, "6:00", WeekDays.Workdays);
            CompleteRecentScheduledDays(earlyWakeUp, 3);
            earlyWakeUp.Archive();

            var running = new Habit("demo-user-2", "Running", Frequency.Weekly, "5 km",
                WeekDays.Saturday | WeekDays.Sunday);
            CompleteRecentScheduledDays(running, 2);

            var book = new Habit("demo-user-2", "Read a book", Frequency.Monthly, "One book per month");
            book.MarkCompleted(DateTime.Today.AddDays(-3));

            testData.AddRange(new[] { yoga, gym, studying, meditation, reading, earlyWakeUp, running, book });
        }

        // atzīmē pēdējās N ieplānotās dienas kā izpildītas (neliek izpildi uz dienu, kas nav grafikā)
        private static void CompleteRecentScheduledDays(Habit habit, int count)
        {
            var done = 0;
            for (var i = 0; i < 30 && done < count; i++)
            {
                var day = DateTime.Today.AddDays(-i);
                if (!habit.IsScheduledOn(day)) continue;
                habit.MarkCompleted(day);
                done++;
            }
        }

        public string ReturnTestData()
        {
            EnsureData();

            var s = "List" + "\n";
            foreach (var habit in testData)
            {
                s += habit.ToString() + "\n";
            }
            return s;
        }

        public IEnumerable<Habit> GetTestData()
        {
            EnsureData();
            return testData;
        }

        public bool SaveToFile()
        {
            EnsureData();

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

        private void EnsureData()
        {
            if (testData.Count == 0)
                CreateTestData();
        }
    }
}
