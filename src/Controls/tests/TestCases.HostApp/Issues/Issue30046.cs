namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30046, "[Android] Switch Shadow Does Not Follow Thumb when Toggle On or Off", PlatformAffected.Android)]
public class Issue30046 : ContentPage
{
	public Issue30046()
	{
		var switchControl = new Switch
		{
			AutomationId = "ShadowSwitch",
			HorizontalOptions = LayoutOptions.Center,
			Shadow = new Shadow
			{
				Brush = Colors.Red,
				Offset = new Point(0, 0),
				Radius = 10,
				Opacity = 1
			}
		};

		var statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Switch Off",
			HorizontalOptions = LayoutOptions.Center
		};

		switchControl.Toggled += (s, e) =>
		{
			statusLabel.Text = e.Value ? "Switch On" : "Switch Off";
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(24),
			Spacing = 14,
			Children = { switchControl, statusLabel }
		};
	}
}
