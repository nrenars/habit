using System.Net;
using System.Text;
using HabitManager.Maui.Services;
using Xunit;

namespace HabitManager.Tests;

// HabitApiClient tiek testēts bez tīkla: HttpClient saņem StubHttpMessageHandler,
// kas atgriež gatavas atbildes vai met savienojuma kļūdu.
public class HabitApiClientTests
{
    private const string BaseAddress = "http://localhost/";
    private const string HabitName = "Yoga";
    private const string NotFoundDetail = "Habit was not found.";

    private const string HabitJson = """
        {"id":"11111111-1111-1111-1111-111111111111","userId":"user-1","name":"Yoga","note":"",
         "status":"Active","frequency":"Daily","scheduledDays":"Monday, Friday",
         "createdAt":"2026-11-16T08:00:00","editedAt":"2026-11-16T08:00:00",
         "completionCount":0,"isCompletedToday":false,"isScheduledToday":true,"canCompleteToday":true}
        """;

    private static readonly string HabitsJson = $"[{HabitJson}]";

    private static readonly string NotFoundProblemJson =
        $$"""{"status":404,"detail":"{{NotFoundDetail}}"}""";

    private static HabitApiClient CreateClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
    {
        var http = new HttpClient(new StubHttpMessageHandler(respond)) { BaseAddress = new Uri(BaseAddress) };
        return new HabitApiClient(http);
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode status, string json)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    // ---------- atbilžu apstrāde ----------

    [Fact]
    public async Task GetHabitsAsync_ServerReturnsHabits_ReturnsDeserializedHabits()
    {
        // Arrange
        var client = CreateClient(_ => Task.FromResult(JsonResponse(HttpStatusCode.OK, HabitsJson)));

        // Act
        var habits = await client.GetHabitsAsync();

        // Assert
        var habit = Assert.Single(habits);
        Assert.Equal(HabitName, habit.Name);
        Assert.Equal(WeekDays.Monday | WeekDays.Friday, habit.ScheduledDays);
    }

    [Fact]
    public async Task GetHabitAsync_ServerReturnsNotFound_ThrowsApiExceptionWithServerDetail()
    {
        // Arrange
        var client = CreateClient(_ => Task.FromResult(JsonResponse(HttpStatusCode.NotFound, NotFoundProblemJson)));

        // Act
        var exception = await Assert.ThrowsAsync<ApiException>(() => client.GetHabitAsync(Guid.NewGuid()));

        // Assert
        // Šeit tiek pārbaudīts teksts apzināti: servera "detail" ir tas, ko lietotne rāda lietotājam.
        Assert.Equal(NotFoundDetail, exception.Message);
    }

    [Fact]
    public async Task GetHabitAsync_ServerReturnsNonJsonError_ThrowsApiException()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Internal error")
        };
        var client = CreateClient(_ => Task.FromResult(response));

        // Act + Assert
        await Assert.ThrowsAsync<ApiException>(() => client.GetHabitAsync(Guid.NewGuid()));
    }

    // ---------- tīkla kļūmes ----------

    [Fact]
    public async Task GetHabitsAsync_ConnectionFails_ThrowsApiException()
    {
        // Arrange
        var client = CreateClient(_ => throw new HttpRequestException());

        // Act + Assert
        await Assert.ThrowsAsync<ApiException>(() => client.GetHabitsAsync());
    }

    [Fact]
    public async Task GetHabitsAsync_RequestTimesOut_ThrowsApiException()
    {
        // Arrange
        var client = CreateClient(_ => throw new TaskCanceledException());

        // Act + Assert
        await Assert.ThrowsAsync<ApiException>(() => client.GetHabitsAsync());
    }

    // ---------- nosūtītie pieprasījumi ----------

    [Fact]
    public async Task GetHabitsAsync_Always_SendsGetToHabitsEndpoint()
    {
        // Arrange
        HttpRequestMessage? sent = null;
        var client = CreateClient(request =>
        {
            sent = request;
            return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[]"));
        });

        // Act
        await client.GetHabitsAsync();

        // Assert
        Assert.Equal(HttpMethod.Get, sent!.Method);
        Assert.Equal("/api/habits", sent.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task CreateHabitAsync_ValidRequest_PostsToHabitsEndpoint()
    {
        // Arrange
        HttpRequestMessage? sent = null;
        var client = CreateClient(request =>
        {
            sent = request;
            return Task.FromResult(JsonResponse(HttpStatusCode.Created, HabitJson));
        });

        // Act
        await client.CreateHabitAsync(new CreateHabitRequest { Name = HabitName });

        // Assert
        Assert.Equal(HttpMethod.Post, sent!.Method);
        Assert.Equal("/api/habits", sent.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task CreateHabitAsync_ValidRequest_SendsHabitNameInJsonBody()
    {
        // Arrange
        string? body = null;
        var client = CreateClient(async request =>
        {
            body = await request.Content!.ReadAsStringAsync();
            return JsonResponse(HttpStatusCode.Created, HabitJson);
        });

        // Act
        await client.CreateHabitAsync(new CreateHabitRequest { Name = HabitName });

        // Assert
        Assert.Contains(HabitName, body);
    }

    [Fact]
    public async Task ArchiveHabitAsync_Always_PostsToArchiveEndpoint()
    {
        // Arrange
        HttpRequestMessage? sent = null;
        var client = CreateClient(request =>
        {
            sent = request;
            return Task.FromResult(JsonResponse(HttpStatusCode.OK, HabitJson));
        });

        // Act
        await client.ArchiveHabitAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(HttpMethod.Post, sent!.Method);
        Assert.EndsWith("/archive", sent.RequestUri!.AbsolutePath);
    }
}
