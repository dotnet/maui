namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33772, "Shell SearchHandler SearchBoxVisibility does not update when changed dynamically", PlatformAffected.Android)]
public class Issue33772 : Shell
{
	readonly SearchHandler _searchHandler;
	readonly Label _statusLabel;

	public Issue33772()
	{
		_searchHandler = new SearchHandler
		{
			Placeholder = "Search here...",
			AutomationId = "SearchHandler",
			SearchBoxVisibility = SearchBoxVisibility.Collapsible
		};

		_statusLabel = new Label
		{
			Text = "Current: Collapsible",
			FontSize = 18,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "StatusLabel"
		};

		var expandButton = new Button
		{
			Text = "Change to Expanded",
			AutomationId = "ExpandButton",
			HorizontalOptions = LayoutOptions.Center
		};

		expandButton.Clicked += OnExpandedClicked;

		var collapsibleButton = new Button
		{
			Text = "Change to Collapsible",
			AutomationId = "CollapsibleButton",
			HorizontalOptions = LayoutOptions.Center
		};

		collapsibleButton.Clicked += OnCollapsibleClicked;

		var contentPage = new ContentPage
		{
			Title = "SearchHandler Visibility Test",
			Content = new VerticalStackLayout
			{
				Padding = 10,
				Spacing = 25,
				Children =
				{
					new Label
					{
						Text = "SearchHandler SearchBoxVisibility Test",
						FontSize = 18,
						HorizontalOptions = LayoutOptions.Center,
						AutomationId = "TitleLabel"
					},
					expandButton,
					collapsibleButton,
					_statusLabel
				}
			}
		};

		SetSearchHandler(contentPage, _searchHandler);

		Items.Add(new ShellContent { Content = contentPage });
	}

	void OnExpandedClicked(object sender, EventArgs e)
	{
		_searchHandler.SearchBoxVisibility = SearchBoxVisibility.Expanded;
		_statusLabel.Text = "Current: Expanded";
	}

	void OnCollapsibleClicked(object sender, EventArgs e)
	{
		_searchHandler.SearchBoxVisibility = SearchBoxVisibility.Collapsible;
		_statusLabel.Text = "Current: Collapsible";
	}
}
