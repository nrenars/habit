namespace HabitManager.Maui.Views;

[QueryProperty(nameof(Habit), "Habit")]
public partial class EditHabit : ContentPage
{
	private Habit _habit = null!;

	public Habit Habit
	{
	get => _habit;
	set
	{
	    _habit = value;
	    BindingContext = value;   // XAML var lietot {Binding Name} u.c.
	}
	}

	public EditHabit()
	{
		InitializeComponent();
	}

	private async void OnSaveButtonClicked(object? sender, EventArgs e){
	    await Shell.Current.GoToAsync(nameof(HabitDetails),
		new Dictionary<string, object> { ["Habit"] = Habit });
	}
}
