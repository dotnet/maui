namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23315, "LoadFile in src/Core/src/Platform/iOS/MauiWKWebView.cs ignore directories", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue23315 : TestContentPage
{
	public Issue23315()
	{
	}

	protected override void Init()
	{
		var statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		WebView webView = new WebView
		{
			Source = "foo/bar/baz/test.html"
		};

		webView.Navigated += async (sender, e) =>
		{
			if (e.Result != WebNavigationResult.Success)
			{
				statusLabel.Text = $"Failed";
			}
			else if (e.Result == WebNavigationResult.Success)
			{
				statusLabel.Text = $"Success";
			}
		};

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(),
				new RowDefinition { Height = 100 }
			}
		};

		grid.Add(webView, 0, 0);
		grid.Add(statusLabel, 0, 1);

		Content = grid;
	}
}