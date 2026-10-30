namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35516, "SearchHandler Query should update dynamically", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue35516 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;

		var searchHandler = new SearchHandler
		{
			SearchBoxVisibility = SearchBoxVisibility.Expanded,
			Placeholder = "Search",
			TextTransform = TextTransform.Uppercase,
			Query = "Initial query"
		};

		var page = new ContentPage
		{
			Title = "Dynamic query",
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
				{
					new Button
					{
						Text = "Update query",
						AutomationId = "UpdateQueryButton",
						Command = new Command(() => searchHandler.Query = "Hello World")
					}
				}
			}
		};

		Shell.SetSearchHandler(page, searchHandler);
		AddContentPage(page, "Home");
	}
}
