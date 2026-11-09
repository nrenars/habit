using HabitManager.Maui.Services;
using HabitManager.Shared;

namespace HabitManager.Maui.Views;

public partial class NewHabit : ContentPage
{
    private readonly IHabitApiClient _api;

    public NewHabit(IHabitApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    private async void OnAddNewHabitClicked(object? sender, EventArgs e)
    {
        if (FrequencyPicker.SelectedIndex < 0)
        {
            await DisplayAlertAsync("Kļūda", "Izvēlies biežumu.", "OK");
            return;
        }

        var request = new CreateHabitRequest
        {
            Name = NameEntry.Text ?? "",
            Note = NoteEntry.Text ?? "",
            Frequency = (Frequency)FrequencyPicker.SelectedIndex,
            ScheduledDays = DaysPicker.SelectedDays
        };

        try
        {
            await _api.CreateHabitAsync(request);
            await Shell.Current.GoToAsync("..");
        }
        catch (ApiException ex)
        {
            // piem. tukšs nosaukums vai nav izvēlēta neviena diena (validē serveris)
            await DisplayAlertAsync("Kļūda", ex.Message, "OK");
        }
    }
}
