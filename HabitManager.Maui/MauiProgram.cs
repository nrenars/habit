using Microsoft.Extensions.Logging;
using HabitManager.Maui.Services;
namespace HabitManager.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            File.WriteAllText(
                Path.Combine(FileSystem.AppDataDirectory, "crash.txt"),
                e.ExceptionObject.ToString());

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<IHabitApiClient>(_ =>
        {
            var http = new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl),
                Timeout = TimeSpan.FromSeconds(5)
            };
            http.DefaultRequestHeaders.Add("X-User-Id", ApiConfig.UserId);
            return new HabitApiClient(http);
        });

        builder.Services.AddTransient<Views.HabitList>();
        builder.Services.AddTransient<Views.NewHabit>();
        builder.Services.AddTransient<Views.HabitDetails>();
        builder.Services.AddTransient<Views.EditHabit>();
        builder.Services.AddTransient<Views.CompletedHabitList>();

        return builder.Build();
    }
}
