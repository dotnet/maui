namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35788, "[Android] WebView CanGoBack returns true unexpectedly on first page due to spurious about:blank history entry", PlatformAffected.Android)]
public class Issue35788 : ContentPage
{
	const string StatusLabelId = "Issue35788StatusLabel";
	const string NavigateButtonId = "Issue35788NavigateButton";

	readonly Label _statusLabel;
	readonly WebView _webView;

	public Issue35788()
	{
		_statusLabel = new Label
		{
			AutomationId = StatusLabelId,
			Text = "Waiting"
		};

		_webView = new WebView
		{
			HeightRequest = 300
		};

		_webView.Navigated += OnWebViewNavigated;

		var navigateButton = new Button
		{
			AutomationId = NavigateButtonId,
			Text = "Load Page"
		};

		navigateButton.Clicked += (s, e) =>
			_webView.Source = new HtmlWebViewSource { Html = "<html><body><h1>Hello</h1></body></html>" };

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children = { navigateButton, _statusLabel, _webView }
		};
	}

	void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
	{
		if (e.Result == WebNavigationResult.Success)
			_statusLabel.Text = _webView.CanGoBack ? "CanGoBack=True" : "CanGoBack=False";
		else
			_statusLabel.Text = $"NavFailed:{e.Result}";
	}
}
