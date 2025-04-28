namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 11677, "[iOS][maccatalyst] SearchBar BackgroundColor is black when set to transparent", PlatformAffected.iOS)]
public class Issue11677 : ContentPage
{
	public Issue11677()
	{
		Content = new VerticalStackLayout
		{
			Children =
			{
				new SearchBar
				{
					BackgroundColor = Colors.Transparent,
					Placeholder = "Search"
				},
				new Label
				{
					Text = "Test SearchBar with Transparent Background",
					FontSize = 16,
					TextColor = Colors.Black,
					AutomationId = "Label"
				}
			}
		};
	}
}



	