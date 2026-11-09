using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HabitManager.Shared;

namespace HabitManager.Maui.Services;

public class ApiException(string message) : Exception(message);

public interface IHabitApiClient
{
    Task<List<HabitDto>> GetHabitsAsync(bool includeArchived = false);
    Task<HabitDto> GetHabitAsync(Guid id);
    Task<HabitDto> CreateHabitAsync(CreateHabitRequest request);
    Task<HabitDto> UpdateHabitAsync(Guid id, UpdateHabitRequest request);
    Task<HabitDto> ArchiveHabitAsync(Guid id);
    Task<HabitCompletionDto> CompleteHabitAsync(Guid id, DateTime? date = null);
    Task<List<HabitCompletionDto>> GetCompletionsAsync(Guid id);
}

public class HabitApiClient(HttpClient http) : IHabitApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<List<HabitDto>> GetHabitsAsync(bool includeArchived = false)
        => SendAsync<List<HabitDto>>(HttpMethod.Get,
            $"api/habits?includeArchived={includeArchived.ToString().ToLowerInvariant()}");

    public Task<HabitDto> GetHabitAsync(Guid id)
        => SendAsync<HabitDto>(HttpMethod.Get, $"api/habits/{id}");

    public Task<HabitDto> CreateHabitAsync(CreateHabitRequest request)
        => SendAsync<HabitDto>(HttpMethod.Post, "api/habits", request);

    public Task<HabitDto> UpdateHabitAsync(Guid id, UpdateHabitRequest request)
        => SendAsync<HabitDto>(HttpMethod.Put, $"api/habits/{id}", request);

    public Task<HabitDto> ArchiveHabitAsync(Guid id)
        => SendAsync<HabitDto>(HttpMethod.Post, $"api/habits/{id}/archive");

    public Task<HabitCompletionDto> CompleteHabitAsync(Guid id, DateTime? date = null)
        => SendAsync<HabitCompletionDto>(HttpMethod.Post, $"api/habits/{id}/completions",
            new MarkHabitCompletedRequest { Date = date });

    public Task<List<HabitCompletionDto>> GetCompletionsAsync(Guid id)
        => SendAsync<List<HabitCompletionDto>>(HttpMethod.Get, $"api/habits/{id}/completions");

    private async Task<T> SendAsync<T>(HttpMethod method, string url, object? body = null)
    {
        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(method, url);
            if (body is not null)
                request.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);
            response = await http.SendAsync(request);
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("Serveris neatbild. Mēģini vēlreiz.");
        }
        catch (Exception)
        {
            throw new ApiException("Nevar sazināties ar serveri. Pārbaudi, vai API ir palaists.");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw new ApiException(await ReadErrorAsync(response));

            var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            return result ?? throw new ApiException("Serveris atgrieza tukšu atbildi.");
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDto>(JsonOptions);
            if (!string.IsNullOrWhiteSpace(problem?.Detail))
                return problem.Detail;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
        }

        return $"Servera kļūda ({(int)response.StatusCode}).";
    }

    private record ProblemDto(string? Title, string? Detail);
}
