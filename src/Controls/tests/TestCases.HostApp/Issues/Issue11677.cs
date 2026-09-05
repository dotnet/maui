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
				new Label
				{
					Text = "SearchBar Transparent BackgroundColor",
					FontSize = 16,
					TextColor = Colors.Black,
					AutomationId = "Label"
				},
				new SearchBar
				{
					BackgroundColor = Colors.Transparent,
				},
				new Label
				{
					Text = "SearchBar with null background",
					FontSize = 16,
					TextColor = Colors.Black,
				},
				new SearchBar
				{
					BackgroundColor = null,
				},
				new Label
				{
					Text = "SearchBar with default background",
					FontSize = 16,
					TextColor = Colors.Black,
				},
				new SearchBar
				{
					Background = Brush.Default,
				}
			}
		};

	}
}	