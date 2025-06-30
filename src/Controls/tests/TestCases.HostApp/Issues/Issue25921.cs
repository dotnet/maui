namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25921, "[Windows] Setting BackgroundColor for Slider updates the Maximum Track Color", PlatformAffected.UWP)]
public class Issue25921 : ContentPage
{
	public Issue25921()
	{
		var layout = new VerticalStackLayout();
		layout.VerticalOptions = LayoutOptions.Center;
		layout.Spacing = 20;
		var slider = new Slider()
		{
			WidthRequest = 300,
			Maximum = 100,
			Minimum = 0,
			Value = 50,
			BackgroundColor = Colors.Yellow,
			MaximumTrackColor = Colors.Red,
			MinimumTrackColor = Colors.Fuchsia,
		};

		var secondSlider = new Slider()
		{
			WidthRequest = 300,
			Maximum = 100,
			Minimum = 0,
			Value = 50,
			Background = Colors.Grey,
			MaximumTrackColor = Colors.SpringGreen,
			MinimumTrackColor = Colors.BlueViolet,
		};

		var button = new Button()
		{
			Text = "Change Background Color",
			AutomationId = "ColorChangeButton",
			Command = new Command(() => secondSlider.Background = Colors.Salmon)
		};

		layout.Children.Add(slider);
		layout.Children.Add(secondSlider);
		layout.Children.Add(button);

		Content = layout;
	}
}