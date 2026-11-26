namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35516_1, "SearchHandler CharacterSpacing should apply initially", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue35516_1 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;

		var searchHandler = new SearchHandler
		{
			SearchBoxVisibility = SearchBoxVisibility.Expanded,
			Query = "Search text",
			Placeholder = "Search",
			CharacterSpacing = 5
		};

		var page = new ContentPage
		{
			Title = "Search character spacing",
			Content = new Label
			{
				Text = "The search text should have character spacing of 5.",
				AutomationId = "CharacterSpacingInstructions",
				VerticalOptions = LayoutOptions.Center,
				HorizontalTextAlignment = TextAlignment.Center,
				Padding = 20,
			}
		};

		Shell.SetSearchHandler(page, searchHandler);
		AddContentPage(page, "Home");
	}
}
