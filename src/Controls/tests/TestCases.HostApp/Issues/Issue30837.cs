namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30837, "[iOS] TimePicker AM/PM frequently changes when the app is closed and reopened", PlatformAffected.iOS)]
public class Issue30837 : ContentPage
{
	public Issue30837()
	{
		var label = new Label
		{
			Text = "Select Time:",
			FontSize = 16,
			AutomationId = "TimePickerLabel",
			Margin = new Thickness(0, 0, 0, 5)
		};

		var timePicker = new TimePicker
		{
			Time = new TimeSpan(12, 0, 0),
			Format = "hh:mm tt"
		};

		var layout = new StackLayout
		{
			Padding = new Thickness(20),
			Children =
			{
				label,
				timePicker
			}
		};

		Content = layout;
	}

}