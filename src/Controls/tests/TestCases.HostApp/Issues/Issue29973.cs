namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29973, "[Android] SearchHandler default icons are not displayed correctly", PlatformAffected.Android)]
public class Issue29973 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;

		var searchHandler = new SearchHandler
		{
			Placeholder = "Search here",
			PlaceholderColor = Colors.Blue
		};

		var button = new Button
		{
			Text = "Set SearchHandler Values",
			AutomationId = "valueButton",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		button.Clicked += (s, e) =>
		{
			searchHandler.Query = "Maui";
		};

		var contentPage = new ContentPage
		{
			Title = "Home",
			Content = button
		};

		SetSearchHandler(contentPage, searchHandler);

		Items.Add(new ShellContent
		{
			Title = "Home",
			Route = "MainPage",
			Content = contentPage
		});
	}
}
