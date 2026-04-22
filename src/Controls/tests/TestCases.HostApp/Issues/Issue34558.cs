namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34558, "[Windows] WebView renders blank when HybridWebView and WebView coexist in same app", PlatformAffected.UWP)]
public partial class Issue34558 : ContentPage
{
	public Issue34558()
	{
		var hybridStatusLabel = new Label
		{
			AutomationId = "HybridStatusLabel",
			Text = "Test passes if WebView shows content below without any exception",
		};

		// HybridWebView — initializing this creates a separate CoreWebView2Environment (without fix)
		// that conflicts with the regular WebView's default environment.
		var hybridWebView = new HybridWebView
		{
			AutomationId = "HybridWebViewControl",
			HybridRoot = "hybridroot",
			HeightRequest = 120,
			HorizontalOptions = LayoutOptions.Fill,
		};

		var webView = new WebView
		{
			AutomationId = "RegularWebView",
			HeightRequest = 150,
			HorizontalOptions = LayoutOptions.Fill,
			Source = new HtmlWebViewSource { Html = "<html><body><h1>WebView should load here</h1><p>If you see this, the WebView is working.</p></body></html>" },
		};

		// This label is added to the layout only after the WebView successfully navigates,
		// so the UI test can wait for it to appear as proof the WebView rendered content.
		var webViewNavigatedLabel = new Label
		{
			AutomationId = "WebViewNavigatedLabel",
			Text = "WebView successfully navigated and rendered content!",
		};

		var layout = new VerticalStackLayout
		{
			Spacing = 10,
			Padding = new Thickness(16),
			Children =
			{
				hybridStatusLabel,
				hybridWebView,
				webView,
			}
		};

		webView.Navigated += (s, e) =>
		{
			if (e.Result == WebNavigationResult.Success)
			{
				layout.Children.Add(webViewNavigatedLabel);
			}
		};

		Content = layout;
	}
}
