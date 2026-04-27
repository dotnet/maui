namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35088, "SearchHandler.BackgroundColor cannot be reset to null", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue35088 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;

		var searchHandler = new SearchHandler
		{
			Placeholder = "Search here...",
			SearchBoxVisibility = SearchBoxVisibility.Expanded,
			BackgroundColor = Colors.YellowGreen,
		};

		var resetColorButton = new Button
		{
			Text = "Reset BackgroundColor to null",
			AutomationId = "ResetColorButton",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		resetColorButton.Clicked += (s, e) =>
		{
			searchHandler.BackgroundColor = null;
		};

		var page = new ContentPage
		{
			Title = "Issue35088",
			Content = new VerticalStackLayout
			{
				Spacing = 10,
				Padding = 20,
				Children =
				{
					resetColorButton,
				}
			}
		};

		Shell.SetSearchHandler(page, searchHandler);
		AddContentPage(page, "Home");
	}
}
