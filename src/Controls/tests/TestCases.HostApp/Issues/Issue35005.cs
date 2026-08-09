namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35005, "SearchHandler Query not shown on iOS/MacCatalyst at load time", PlatformAffected.iOS)]
public class Issue35005 : Shell
{
	public Issue35005()
	{
		var contentPage = new Issue35005SearchPage();
		Items.Add(new ShellContent
		{
			Title = "Search Test",
			Content = contentPage
		});
	}

	public class Issue35005SearchPage : ContentPage
	{
		public const string PreSetQuery = "InitialQuery";

		public Issue35005SearchPage()
		{
			Title = "Search Test";

			var searchHandler = new SearchHandler
			{
				Placeholder = "Search...",
				Query = PreSetQuery,
				AutomationId = "Issue35005SearchHandler"
			};

			Shell.SetSearchHandler(this, searchHandler);

			var statusLabel = new Label
			{
				AutomationId = "Issue35005StatusLabel",
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
