namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 29325, "Button Shadow Color Transparency Not Applied Correctly", PlatformAffected.Android)]
public class Issue29325 : ContentPage
{
	public Issue29325()
	{
		var verticalStackLayout = new VerticalStackLayout();
		// Create a Button
		var button = new Button
		{
			AutomationId = "Button",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Text = "Button shadow with transparency color",
			Shadow = new Shadow
			{
				Brush = Color.FromArgb("#59000000"),
				Offset = new Point(0, 12),
				Radius = 12,
			}
		};

		// Add the Button to the VerticalStackLayout
		verticalStackLayout.Children.Add(button);

		// Set the Content of the page
		Content = verticalStackLayout;
	}
}