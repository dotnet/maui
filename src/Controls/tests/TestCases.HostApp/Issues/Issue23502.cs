namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23502, "WebView Navigated event is not triggered", PlatformAffected.Android, isInternetRequired: true)]
public partial class Issue23502 : ContentPage
{
	public Issue23502()
	{

		var verticalStackLayout = new VerticalStackLayout();
		verticalStackLayout.Spacing = 20;
		var webview = new WebView()
		{
			HeightRequest = 300,
			AutomationId = "webView",
			Source = new HtmlWebViewSource
			{
				Html = @"<HTML><BODY><H1>.NET MAUI</H1><P>Welcome to WebView.</P></BODY></HTML>"
			},
		};

		var label1 = new Label
		{
			Text = "WebView Navigating event is not triggered",
			AutomationId = "navigatingLabel"
		};

		var label = new Label
		{
			Text = "WebView Navigated event is not triggered",
			AutomationId = "navigatedLabel"
		};

		webview.Navigating += (s, e) =>
		{
			label1.Text = "Navigating event is triggered";
		};

		webview.Navigated += (s, e) =>
		{
			label.Text = "Navigated event is triggered";
		};

		verticalStackLayout.Add(label1);
		verticalStackLayout.Add(label);
		verticalStackLayout.Add(webview);

		Content = verticalStackLayout;
	}
}