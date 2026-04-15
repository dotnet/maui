namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 99999, "SearchHandler Query not shown on iOS/MacCatalyst at load time", PlatformAffected.iOS)]
public class Issue99999 : Shell
{
	public Issue99999()
	{
		var contentPage = new _99999SearchPage();
		Items.Add(new ShellContent
		{
			Title = "Search Test",
			Content = contentPage
		});
	}

	public class _99999SearchPage : ContentPage
	{
		public const string PreSetQuery = "InitialQuery";

		public _99999SearchPage()
		{
			Title = "Search Test";

			var searchHandler = new SearchHandler
			{
				Placeholder = "Search...",
				Query = PreSetQuery,
				AutomationId = "Issue99999SearchHandler"
			};

			Shell.SetSearchHandler(this, searchHandler);

			var statusLabel = new Label
			{
				AutomationId = "Issue99999StatusLabel",
				Text = "Page Loaded"
			};

			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Children = { statusLabel }
			};
		}
	}
}
