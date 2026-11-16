using System.Text.Json.Serialization;
using HabitManager.Api;
using HabitManager.Application;
using HabitManager.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var connectionString = builder.Configuration.GetConnectionString("Habits")
    ?? $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "habits.db")}";

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddScoped<HabitService>();

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.Use(async (context, next) =>
{
    await next();
    Console.WriteLine($"{DateTime.Now:T} {context.Request.Method} {context.Request.Path} -> {context.Response.StatusCode}");
});

app.UseExceptionHandler();
app.MapHabitEndpoints();

app.Run();
