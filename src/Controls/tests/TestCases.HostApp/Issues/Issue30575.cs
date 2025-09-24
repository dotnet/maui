namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30575, "FlowDirection RightToLeft causes mirrored content in WebView", PlatformAffected.UWP, isInternetRequired: true)]
public class Issue30575 : ContentPage
{
	public Issue30575()
	{
		VerticalStackLayout stackLayout = new VerticalStackLayout();
		WebView webView = new WebView
		{
			HeightRequest = 400,
			WidthRequest = 400,
			FlowDirection = FlowDirection.RightToLeft,
		};

		webView.Source = new UrlWebViewSource
		{
			Url = "https://example.com/"
		};

		Label label = new Label
		{
			AutomationId = "WebViewLabel",
			Text = "The test passes if the content is not mirrored.",
		};

		stackLayout.Children.Add(webView);
		stackLayout.Children.Add(label);
		Content = stackLayout;
	}
}
