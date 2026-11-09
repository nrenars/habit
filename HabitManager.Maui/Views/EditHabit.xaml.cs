using HabitManager.Maui.Services;
using HabitManager.Shared;

namespace HabitManager.Maui.Views;

[QueryProperty(nameof(Habit), "Habit")]
public partial class EditHabit : ContentPage
{
    private readonly IHabitApiClient _api;
    private HabitDto _habit = null!;

    public HabitDto Habit
    {
        get => _habit;
        set
        {
            _habit = value;
            NameEntry.Text = value.Name;
            NoteEntry.Text = value.Note;
            FrequencyPicker.SelectedIndex = (int)value.Frequency;
            DaysPicker.SelectedDays = value.ScheduledDays;
        }
    }

    public EditHabit(IHabitApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    private async void OnSaveButtonClicked(object? sender, EventArgs e)
    {
        if (FrequencyPicker.SelectedIndex < 0)
        {
            await DisplayAlertAsync("Kļūda", "Izvēlies biežumu.", "OK");
            return;
        }

        var request = new UpdateHabitRequest
        {
            Name = NameEntry.Text ?? "",
            Note = NoteEntry.Text ?? "",
            Frequency = (Frequency)FrequencyPicker.SelectedIndex,
            ScheduledDays = DaysPicker.SelectedDays
        };

        try
        {
            await _api.UpdateHabitAsync(_habit.Id, request);
            await Shell.Current.GoToAsync("..");
        }
        catch (ApiException ex)
        {
            await DisplayAlertAsync("Kļūda", ex.Message, "OK");
        }
    }
}
