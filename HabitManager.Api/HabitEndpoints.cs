using HabitManager.Application;
using HabitManager.Shared;
using Microsoft.AspNetCore.Mvc;

namespace HabitManager.Api;

public static class HabitEndpoints
{
    private const string UserHeader = "X-User-Id";

    public static void MapHabitEndpoints(this IEndpointRouteBuilder app)
    {
        var habits = app.MapGroup("/api/habits");

        habits.MapGet("/", async ([FromHeader(Name = UserHeader)] string userId,
                                  HabitService service, bool includeArchived = false) =>
            Results.Ok(await service.GetAllAsync(userId, includeArchived)));

        habits.MapGet("/{id:guid}", async ([FromHeader(Name = UserHeader)] string userId,
                                           Guid id, HabitService service) =>
            Results.Ok(await service.GetAsync(userId, id)));

        habits.MapPost("/", async ([FromHeader(Name = UserHeader)] string userId,
                                   CreateHabitRequest request, HabitService service) =>
        {
            var created = await service.CreateAsync(userId, request);
            return Results.Created($"/api/habits/{created.Id}", created);
        });

        habits.MapPut("/{id:guid}", async ([FromHeader(Name = UserHeader)] string userId,
                                           Guid id, UpdateHabitRequest request, HabitService service) =>
            Results.Ok(await service.UpdateAsync(userId, id, request)));

        habits.MapPost("/{id:guid}/archive", async ([FromHeader(Name = UserHeader)] string userId,
                                                    Guid id, HabitService service) =>
            Results.Ok(await service.ArchiveAsync(userId, id)));

        habits.MapGet("/{id:guid}/completions", async ([FromHeader(Name = UserHeader)] string userId,
                                                       Guid id, HabitService service) =>
            Results.Ok(await service.GetCompletionsAsync(userId, id)));

        habits.MapPost("/{id:guid}/completions", async ([FromHeader(Name = UserHeader)] string userId,
                                                        Guid id, MarkHabitCompletedRequest? request,
                                                        HabitService service) =>
        {
            var completion = await service.CompleteAsync(userId, id, request?.Date);
            return Results.Created($"/api/habits/{id}/completions", completion);
        });
    }
}
