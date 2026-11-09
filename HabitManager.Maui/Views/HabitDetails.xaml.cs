using HabitManager.Maui.Services;
using HabitManager.Shared;

namespace HabitManager.Maui.Views;

[QueryProperty(nameof(Habit), "Habit")]
public partial class HabitDetails : ContentPage
{
    private readonly IHabitApiClient _api;
    private HabitDto _habit = null!;

    public HabitDto Habit
    {
        get => _habit;
        set
        {
            _habit = value;
            BindingContext = value;
        }
    }

    public HabitDetails(IHabitApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_habit is null) return;

        try
        {
            Habit = await _api.GetHabitAsync(_habit.Id);
        }
        catch (ApiException ex)
        {
            await DisplayAlertAsync("Kļūda", ex.Message, "OK");
        }
    }

    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EditHabit),
            new Dictionary<string, object> { ["Habit"] = Habit });
    }

    private async void OnArchiveButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            await _api.ArchiveHabitAsync(Habit.Id);
            await Shell.Current.GoToAsync("..");
        }
        catch (ApiException ex)
        {
            await DisplayAlertAsync("Kļūda", ex.Message, "OK");
        }
    }
}
