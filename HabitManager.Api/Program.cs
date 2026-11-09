using System.Text.Json.Serialization;
using HabitManager.Api;
using HabitManager.Application;
using HabitManager.Infrastructure;
using TestData;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddSingleton<IHabitRepository>(_ =>
{
    var factory = new TestDataFactoryList();
    factory.CreateTestData();
    return new InMemoryHabitRepository(factory.GetTestData());
});
builder.Services.AddScoped<HabitService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    await next();
    Console.WriteLine($"{DateTime.Now:T} {context.Request.Method} {context.Request.Path} -> {context.Response.StatusCode}");
});

app.UseExceptionHandler();
app.MapHabitEndpoints();

app.Run();
