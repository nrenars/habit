namespace HabitManager.Tests;

// Stub: aizstāj reālo tīklu, tāpēc API klienta testi ir ātri un neprasa serveri
internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
    : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => respond(request);
}
