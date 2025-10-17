namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27627, "[Windows] Border Automation Peer", PlatformAffected.UWP)]
public partial class Issue27627 : ContentPage
{
	public Issue27627()
	{
		var grid = new Grid();

		var border = new Border
		{
			AutomationId = "TestBorder",
			VerticalOptions = LayoutOptions.Center,
			HeightRequest = 100,
			Stroke = Colors.Red
		};

		var label = new Label
		{
			Text = "Test Label",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		border.Content = label;

		grid.Children.Add(border);
		Content = grid;
	}
}