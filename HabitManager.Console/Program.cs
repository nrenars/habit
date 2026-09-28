using System;
using System.Linq;
using HabitManager;
using TestData;

ITestDataFactory factory = new TestDataFactoryList();
factory.CreateTestData();

var habits = factory.GetTestData().ToList();

Console.WriteLine($"Habit count: {habits.Count}");
Console.WriteLine();

var userIds = habits.Select(h => h.UserId).Distinct().OrderBy(id => id);

foreach (var userId in userIds)
{
    Console.WriteLine($"User: {userId}");

    var userHabits = habits.Where(h => h.UserId == userId);

    foreach (var habit in userHabits)
    {
        var completedToday = habit.IsCompletedToday() ? "yes" : "no";
        Console.WriteLine($"  - {habit.Name} [{habit.Status}] — completed today: {completedToday}");
    }

    Console.WriteLine();
}

var demoHabit = habits.First(h => h.Status == HabitStatus.Active);

if (!demoHabit.IsCompletedToday())
{
    demoHabit.MarkCompleted(DateTime.Today);
    Console.WriteLine($"Atzimeju \"{demoHabit.Name}\" ka izpilditu sodien.");
}
else
{
    Console.WriteLine($"\"{demoHabit.Name}\" jau ija atzimets ka izpildits sodien.");
}
