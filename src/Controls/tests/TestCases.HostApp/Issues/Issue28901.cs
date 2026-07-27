namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28901, "[Windows] Switch control is not sizing properly", PlatformAffected.UWP)]
public class Issue28901 : ContentPage
{
	public Issue28901()
	{
		var switchControl = new Switch
		{
			IsToggled = true,
			HorizontalOptions = LayoutOptions.End,
			AutomationId = "SwitchControl"
		};

		var switchControl2 = new Switch();

		var label = new Label
		{
			Text = "Switch Control",
			VerticalOptions = LayoutOptions.Center,
		};

		var settingsLabel = new Label
		{
			Text = "Change View",
			VerticalOptions = LayoutOptions.Center,
		};

		var horizontalStackLayout = new HorizontalStackLayout
		{
			Children =
			{
				label,
				switchControl2,
				settingsLabel
			},
		};

		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				switchControl,
				horizontalStackLayout,
			}
		};
	}
}