namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29547, "The default native colors are not displayed when the Switch on color is not explicitly set", PlatformAffected.UWP)]
public class Issue1 : ContentPage
{
	public Issue1()
	{
		var switchControl = new Switch
		{
			IsToggled = true,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "switchControl"
		};

		var grid = new Grid();
		grid.Children.Add(switchControl);

		Content = grid;
	}
}
