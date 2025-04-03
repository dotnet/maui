namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28714, "[iOS] WebView BackgroundColor is not setting correctly", PlatformAffected.iOS, isInternetRequired: true)]
public partial class Issue28714 : ContentPage
{
	public Issue28714()
	{
		BackgroundColor = Colors.YellowGreen;
		var verticalStackLayout = new VerticalStackLayout();
		verticalStackLayout.Spacing = 20;

		var webView = new WebView()
		{
			HeightRequest = 300,
			WidthRequest = 400,
			BackgroundColor = Colors.Transparent,
			Source = new HtmlWebViewSource
			{
				Html = @"
    <!DOCTYPE html>
    <html lang='en'>
    <body>
        <h1>Welcome to WebView</h1>
        <p id='message'></p>
    </body>
    </html>"
			}

		};

		var label = new Label
		{
			Text = "WebView BackgroundColor",
			AutomationId = "label"
		};

		verticalStackLayout.Add(label);
		verticalStackLayout.Add(webView);

		Content = verticalStackLayout;
	}
}