namespace HabitManager.Maui.Views;

public partial class WeekDayPicker : ContentView
{
    public WeekDayPicker()
    {
        InitializeComponent();
        SelectedDays = WeekDays.All;
    }

    private (CheckBox Box, WeekDays Day)[] Boxes => new[]
    {
        (MonBox, WeekDays.Monday), (TueBox, WeekDays.Tuesday), (WedBox, WeekDays.Wednesday),
        (ThuBox, WeekDays.Thursday), (FriBox, WeekDays.Friday),
        (SatBox, WeekDays.Saturday), (SunBox, WeekDays.Sunday)
    };

    public WeekDays SelectedDays
    {
        get => Boxes.Where(b => b.Box.IsChecked).Aggregate(WeekDays.None, (acc, b) => acc | b.Day);
        set
        {
            foreach (var (box, day) in Boxes)
                box.IsChecked = value.HasFlag(day);
        }
    }
}
