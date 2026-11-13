namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29493, "[Windows] SearchHandler APIs are not functioning properly", PlatformAffected.UWP)]
public class Issue29493 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;

		var searchHandler = new SearchHandler
		{
			Placeholder = "Type a fruit name to search",
			PlaceholderColor = Colors.Red
		};

		var button = new Button
		{
			Text = "Set SearchHandler Values",
			AutomationId = "button",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		button.Clicked += (s, e) =>
		{
			searchHandler.TextColor = Colors.YellowGreen;
			searchHandler.CancelButtonColor = Colors.Red;
			searchHandler.HorizontalTextAlignment = TextAlignment.End;
			searchHandler.TextTransform = TextTransform.Uppercase;
			searchHandler.Query = "Apple";
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
