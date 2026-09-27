using System;
using System.Linq;
using HabitManager;
using TestData;

ITestDataFactory factory = new TestDataFactoryList();
factory.CreateTestData();

var habits = factory.GetTestData().ToList();

Console.WriteLine($"Kopējais testa ieradumu skaits: {habits.Count}");
Console.WriteLine();

var userIds = habits.Select(h => h.UserId).Distinct().OrderBy(id => id);

foreach (var userId in userIds)
{
    Console.WriteLine($"Lietotājs: {userId}");

    var userHabits = habits.Where(h => h.UserId == userId);

    foreach (var habit in userHabits)
    {
        var completedToday = habit.IsCompletedToday() ? "jā" : "nē";
        Console.WriteLine($"  - {habit.Name} [{habit.Status}] — šodien izpildīts: {completedToday}");
    }

    Console.WriteLine();
}

// Piemērs, kur tiek izsaukta domēna metode (MarkCompleted / IsCompletedToday).
var demoHabit = habits.First(h => h.Status == HabitStatus.Active);

if (!demoHabit.IsCompletedToday())
{
    demoHabit.MarkCompleted(DateTime.Today);
    Console.WriteLine($"Atzīmēju \"{demoHabit.Name}\" kā izpildītu šodien.");
}
else
{
    Console.WriteLine($"\"{demoHabit.Name}\" jau bija atzīmēts kā izpildīts šodien.");
}
