using TestData;

namespace HabitManager.Maui.Views;

public partial class HabitList : ContentPage
{
	public HabitList(){
		InitializeComponent();

		var factory = new TestDataFactoryArray();
		factory.CreateTestData();
		HabitsView.ItemsSource = factory.GetTestData();
	}
		
	private async void OnHabitSelected(object? sender, SelectionChangedEventArgs e){
	    if (e.CurrentSelection.FirstOrDefault() is not Habit habit)
		return;

	    HabitsView.SelectedItem = null;

	    await Shell.Current.GoToAsync(nameof(HabitDetails),
		new Dictionary<string, object> { ["Habit"] = habit });
	}

	private async void OnNewHabitClicked(object? sender, EventArgs e){
		await Shell.Current.GoToAsync(nameof(NewHabit));
	}

	private async void OnHabitCompleteButton(object? sender, EventArgs e)
	{
	    if (sender is not Button { BindingContext: Habit habit })
		return;

	    try
	    {
		habit.MarkCompleted(DateTime.Today);
	    }
	    catch (InvalidOperationException ex)
	    {
		await DisplayAlertAsync("Nevar atzīmēt", ex.Message, "OK");
	    }
	}
}
