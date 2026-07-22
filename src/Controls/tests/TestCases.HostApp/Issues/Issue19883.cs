namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19883, "Switch OnColor not applied correctly and ThumbColor not reset when toggled off", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue19883 : ContentPage
{
	public Issue19883()
	{
		var instructionsLabel = new Label
		{
			Text = "ThumbColor is initially Orange (no OnColor set). Tap the button to reset ThumbColor to null and observe the switch.",
			FontSize = 14,
		};

		var testSwitch = new Switch
		{
			IsToggled = false,
			ThumbColor = Colors.Orange,
			AutomationId = "TestSwitch",
			Margin = new Thickness(10, 20, 15, 0),
			VerticalOptions = LayoutOptions.Center
		};

		var resetThumbColorButton = new Button
		{
			Text = "Reset ThumbColor to null",
			AutomationId = "ResetThumbColorButton",
		};

		resetThumbColorButton.Clicked += (s, e) => testSwitch.ThumbColor = null;

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				instructionsLabel,
				testSwitch,
				resetThumbColorButton
			}
		};
	}
}
