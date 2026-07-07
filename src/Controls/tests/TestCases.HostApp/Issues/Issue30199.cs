namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "30199", "TimePicker CharacterSpacing Property Not Working on Windows", PlatformAffected.UWP)]

public class Issue30199 : ContentPage
{
	public Issue30199()
	{
		var label = new Label
		{
			AutomationId = "label",
			Text = "Test passes if TimePicker has character Spacing",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		var timePicker = new TimePicker
		{
			AutomationId = "timePicker",
			Time = new TimeSpan(12, 30, 0),
			CharacterSpacing = 10,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Format = "HH:mm"
		};

		Content = new StackLayout
		{
			Children = { label, timePicker },
			Spacing = 20,
			Margin = new Thickness(20)
		};
	}
}