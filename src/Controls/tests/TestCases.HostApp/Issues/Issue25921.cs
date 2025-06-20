namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25921, "[Windows] Setting BackgroundColor for Slider updates the Maximum Track Color", PlatformAffected.UWP)]
public class Issue25921 : ContentPage
{
	public Issue25921()
	{
		var layout = new VerticalStackLayout();
		layout.VerticalOptions = LayoutOptions.Center;
		var slider = new Slider()
		{
			AutomationId = "testSlider",
			WidthRequest = 300,
			Maximum = 100,
			Minimum = 0,
			Value = 50,
			BackgroundColor = Colors.Yellow,
			MaximumTrackColor = Colors.Red,
			MinimumTrackColor = Colors.Fuchsia,
		};

		layout.Children.Add(slider);
		Content = layout;
	}
}