namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23315, "LoadFile in src/Core/src/Platform/iOS/MauiWKWebView.cs ignore directories", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue23315 : TestContentPage
{
	public Issue23315()
	{
	}

	protected override void Init()
	{
		WebView webView = new WebView
		{
			Source = "foo/bar/baz/test.html"
		};

		var descriptionLabel = new Label
		{
			Text = "This test verifies that the WebView can load files from subdirectories correctly. If you see a blank screen, it means the test has failed.",
			AutomationId = "DescriptionLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.CenterAndExpand
		};

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(),
				new RowDefinition { Height = 300 }
			}
		};

		grid.Add(webView, 0, 0);
		grid.Add(descriptionLabel, 0, 1);

		Content = grid;
	}
}