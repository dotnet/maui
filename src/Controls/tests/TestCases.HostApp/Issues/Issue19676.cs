namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19676, "Android Switch Control Thumb Shadow missing when ThumbColor matches background", PlatformAffected.Android)]
public class Issue19676 : ContentPage
{
	public Issue19676()
	{
		BackgroundColor = Color.FromArgb("#F2F5F8");

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				new Label
				{
					Text = "Switch with ThumbColor matching background",
					FontSize = 16,
					FontAttributes = FontAttributes.Bold,
				},
				new Label
				{
					Text = "The switch thumb should be visible with a shadow, even though ThumbColor matches the background.",
					FontSize = 14,
				},
				new Switch
				{
					IsToggled = true,
					ThumbColor = Color.FromArgb("#F2F5F8"),
					OnColor = Colors.Red,
					AutomationId = "TestSwitch",
					Margin = new Thickness(10, 20, 15, 0),
					VerticalOptions = LayoutOptions.Center
				},
				new Label
				{
					Text = "Expected: Switch thumb is visible with shadow",
					FontSize = 14,
					TextColor = Colors.Gray,
				}
			}
		};
	}
}