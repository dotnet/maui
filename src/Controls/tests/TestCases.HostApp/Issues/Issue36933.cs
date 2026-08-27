namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36933, "DatePicker and TimePicker Background set to null at runtime does not clear on iOS/MacCatalyst", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue36933 : ContentPage
{
	public Issue36933()
	{
		var datePicker = new DatePicker
		{
			AutomationId = "TestDatePicker",
			Background = new SolidColorBrush(Colors.LightGreen),
			Date = new DateTime(2025, 1, 15)
		};

		var timePicker = new TimePicker
		{
			AutomationId = "TestTimePicker",
			Background = new SolidColorBrush(Colors.LightGreen),
			Time = new TimeSpan(10, 30, 0)
		};

		var clearButton = new Button
		{
			Text = "Set Background to Null",
			AutomationId = "ClearBackgroundButton",
			Command = new Command(() =>
			{
				datePicker.Background = null;
				timePicker.Background = null;
			})
		};

		var setColorButton = new Button
		{
			Text = "Set Background Color",
			AutomationId = "SetBackgroundButton",
			Command = new Command(() =>
			{
				datePicker.Background = new SolidColorBrush(Colors.LightBlue);
				timePicker.Background = new SolidColorBrush(Colors.LightBlue);
			})
		};

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = 30,
			Children =
			{
				datePicker,
				timePicker,
				clearButton,
				setColorButton
			}
		};
	}
}
