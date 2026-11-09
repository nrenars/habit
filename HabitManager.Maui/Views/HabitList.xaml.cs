using HabitManager.Maui.Services;
using HabitManager.Shared;

namespace HabitManager.Maui.Views;

public partial class HabitList : ContentPage
{
    private readonly IHabitApiClient _api;

    public HabitList(IHabitApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            HabitsView.ItemsSource = await _api.GetHabitsAsync();
        }
        catch (ApiException ex)
        {
            await DisplayAlertAsync("Kļūda", ex.Message, "OK");
        }
    }

    private async void OnHabitSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not HabitDto habit)
            return;

        HabitsView.SelectedItem = null;

        await Shell.Current.GoToAsync(nameof(HabitDetails),
            new Dictionary<string, object> { ["Habit"] = habit });
    }

    private async void OnNewHabitClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NewHabit));
    }

    private async void OnHabitCompleteButton(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: HabitDto habit })
            return;

        try
        {
            await _api.CompleteHabitAsync(habit.Id);
            await RefreshAsync();
        }
        catch (ApiException ex)
        {
            await DisplayAlertAsync("Nevar atzīmēt", ex.Message, "OK");
        }
    }
}
