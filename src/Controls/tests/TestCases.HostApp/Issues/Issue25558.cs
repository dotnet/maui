namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25558, "ImageButton does not scale Image correctly", PlatformAffected.Android)]
public class Issue25558 : ContentPage
{
	public Issue25558()
	{
		var verticalStackLayout = new VerticalStackLayout
		{
			Padding = 10
		};

		var imageButton = new ImageButton
		{
			AutomationId = "imageButton",
			Source = "groceries.png",
			BackgroundColor = Colors.Blue
		};

		verticalStackLayout.Add(imageButton);
		Content = verticalStackLayout;
	}
}