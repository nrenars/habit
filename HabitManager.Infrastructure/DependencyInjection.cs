using HabitManager.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HabitManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<HabitDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IHabitRepository, EfHabitRepository>();
        return services;
    }
}
