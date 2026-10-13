namespace HabitManager.Maui.Views;
using HabitManager;

public partial class NewHabit : ContentPage
{
	public NewHabit()
	{
		InitializeComponent();
	}

	private async void On(object sender, EventArgs e){
		await Shell.Current.GoToAsync("..");
	}

	private async void OnAddNewHabitClicked(object sender, EventArgs e){
		return;
	}
}
