namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 10805, "Date Format property in DatePicker not working on Windows with date Format", PlatformAffected.UWP)]
public class Issue10805 : ContentPage
{
	public Issue10805()
	{
		var label = new Label
		{
			Text = "Verify DatePicker DateFormat",
			AutomationId = "Label",
		};

		var datePicker = new DatePicker
		{
			Format = "dd.MM.yyyy",
			Date = new DateTime(2022, 2, 25),
		};

		Content = new StackLayout
		{
			Children =
			{
				label,
				datePicker
			},
		};
	}
}
