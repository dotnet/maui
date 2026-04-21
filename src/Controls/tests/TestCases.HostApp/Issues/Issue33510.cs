namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33510, "[Android] RefreshView triggers pull-to-refresh immediately when scrolling up inside a WebView", PlatformAffected.Android, isInternetRequired: true)]
public class Issue33510 : TestContentPage
{
	RefreshView _refreshView;
	Label _statusLabel;

	protected override void Init()
	{
		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Loading..."
		};

		var webView = new WebView
		{
			AutomationId = "TestWebView",
			Source = new UrlWebViewSource { Url = "https://material.angular.io/components/sidenav/overview" }
		};

		webView.Navigated += (_, _) => _statusLabel.Text = "WebView ready";

		_refreshView = new RefreshView
		{
			AutomationId = "TestRefreshView",
			Content = webView
		};

		_refreshView.Command = new Command(async () =>
		{
			_statusLabel.Text = "Refresh triggered";
			await Task.Delay(150);
			_refreshView.IsRefreshing = false;
		});

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		grid.Add(_statusLabel, 0, 0);
		grid.Add(_refreshView, 0, 1);

		Content = grid;
	}
}
