namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35286, "Spacing problem with SearchBar", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue35286 : ContentPage
{
	public Issue35286()
	{
		var content = new VerticalStackLayout
		{
			Padding = 10,
			Spacing = 10,
			Children =
			{
				new Label
				{
					Text = "SearchBar with HeightRequest=35",
					AutomationId = "Issue35286Label1"
				},
				// SearchBar with small HeightRequest - this should NOT have spacing issues
				new SearchBar
				{
					Placeholder = "Search with HeightRequest=35",
					HeightRequest = 35,
					BackgroundColor = Colors.LightBlue,
					AutomationId = "Issue35286SearchBar1"
				},

				new Label
				{
					Text = "SearchBar with HeightRequest=100",
					AutomationId = "Issue35286Label2",
					Margin = new Thickness(0, 20, 0, 0)
				},
				// SearchBar with large HeightRequest - should work normally
				new SearchBar
				{
					Placeholder = "Search with HeightRequest=100",
					HeightRequest = 100,
					BackgroundColor = Colors.LightBlue,
					AutomationId = "Issue35286SearchBar2"
				},
			}
		};

		Content = new ScrollView { Content = content };
	}
}
