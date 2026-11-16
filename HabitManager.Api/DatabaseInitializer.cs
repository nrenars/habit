using HabitManager.Application;
using HabitManager.Infrastructure;
using Microsoft.EntityFrameworkCore;
using TestData;

namespace HabitManager.Api;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<HabitDbContext>();
        await db.Database.MigrateAsync();

        if (await db.Habits.AnyAsync())
            return;

        var repository = scope.ServiceProvider.GetRequiredService<IHabitRepository>();
        var factory = new TestDataFactoryList();
        factory.CreateTestData();

        foreach (var habit in factory.GetTestData())
            await repository.AddAsync(habit);
    }
}
