namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18420, "[Windows]ViewExtensions RotateYTo and RotateXTo with length 0 crashes on Windows", PlatformAffected.UWP)]
public class Issue18420 : ContentPage
{
	int count = 0;
	Button rotateButton;
	public Issue18420()
	{
		var layout = new VerticalStackLayout
		{
			Spacing = 25,
			Padding = new Thickness(30, 0),
			VerticalOptions = LayoutOptions.Center
		};

		rotateButton = new Button
		{
			Text = "Click me to rotate",
			BackgroundColor= Colors.Red,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId= "RotateButton"
		};

		rotateButton.Clicked += OnRotateButtonClicked;
		layout.Children.Add(rotateButton);
		this.Content = layout;
	}

	private void OnRotateButtonClicked(object sender, EventArgs e)
	{
		count++;
		rotateButton.RotateYTo(10 + count, 0);
	}
}