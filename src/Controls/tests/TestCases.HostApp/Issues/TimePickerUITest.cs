namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 26593, "TimePicker UI Test", PlatformAffected.iOS)]
public partial class TimePickerUITest : ContentPage
{
	public TimePickerUITest()
	{
		Grid grid = new Grid();

		TimePicker timePicker = new TimePicker
		{
			Time = new TimeSpan(4, 14, 23),
			Format = "hh:mm:ss",
			AutomationId = "TimePicker"
		};

		grid.Children.Add(timePicker);
		Content = grid;
	}
}