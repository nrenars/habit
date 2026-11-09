using HabitManager.Maui.Views;

namespace HabitManager.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(HabitList), typeof(HabitList));
		Routing.RegisterRoute(nameof(NewHabit), typeof(NewHabit));
		Routing.RegisterRoute(nameof(EditHabit), typeof(EditHabit));
		Routing.RegisterRoute(nameof(HabitDetails), typeof(HabitDetails));
		Routing.RegisterRoute(nameof(CompletedHabitList), typeof(CompletedHabitList));
	}
}
