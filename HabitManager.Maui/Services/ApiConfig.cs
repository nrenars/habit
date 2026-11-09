namespace HabitManager.Maui.Services;

public static class ApiConfig
{
    public static string BaseUrl => DeviceInfo.Platform == DevicePlatform.Android
        ? "http://10.0.2.2:5080/"
        : "http://localhost:5080/";

    public const string UserId = "demo-user-1";
}
