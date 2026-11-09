using HabitManager.Maui.Services;

namespace HabitManager.Maui.Views;

public partial class CompletedHabitList : ContentPage
{
    private readonly IHabitApiClient _api;

    public CompletedHabitList(IHabitApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var habits = await _api.GetHabitsAsync();
            CompletedView.ItemsSource = habits.Where(h => h.IsCompletedToday).ToList();
        }
        catch (ApiException ex)
        {
            await DisplayAlertAsync("Kļūda", ex.Message, "OK");
        }
    }
}
