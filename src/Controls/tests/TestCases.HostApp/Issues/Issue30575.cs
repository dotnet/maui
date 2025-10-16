namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30575, "FlowDirection RightToLeft causes mirrored content in WebView", PlatformAffected.UWP, isInternetRequired: true)]
public class Issue30575 : ContentPage
{
	public Issue30575()
	{
		VerticalStackLayout stackLayout = new VerticalStackLayout();
		WebView webView = new WebView
		{
			HorizontalOptions = LayoutOptions.Start,
			HeightRequest = 400,
			WidthRequest = 400,
			FlowDirection = FlowDirection.RightToLeft,
		};

		webView.Source = new HtmlWebViewSource
		{
			Html = @"
                <html>
                <body>
                <H1>.NET MAUI</H1>
                <p>Welcome to WebView.</p>
                </body>
                </html>
            	"
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
