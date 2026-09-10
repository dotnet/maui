namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19883, "Switch OnColor not applied correctly and ThumbColor not reset when toggled off", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.UWP)]
public class Issue19883 : ContentPage
{
	public Issue19883()
	{
		var instructionsLabel = new Label
		{
			Text = "Toggle the switch on and then off: the thumb must keep its blue ThumbColor in every state and must not revert to the system default when toggled off (when on, the track should use the Orange OnColor). Tapping 'Reset ThumbColor to null' should revert the thumb to the system default color.",
			FontSize = 14,
		};

		var testSwitch = new Switch
		{
			IsToggled = false,
			ThumbColor = Colors.Blue,
			OnColor = Colors.Orange,
			AutomationId = "TestSwitch",
			Margin = new Thickness(10, 20, 15, 0),
			HorizontalOptions = LayoutOptions.Center,
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
