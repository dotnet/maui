namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28714, "[iOS] WebView BackgroundColor is not setting correctly", PlatformAffected.iOS)]
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

		var button = new Button
		{
			Text = "Change WebView BackgroundColor",
			AutomationId = "button"
		};
		button.Clicked += (s, e) =>
		{
			webView.BackgroundColor = Colors.Red;
		};

		verticalStackLayout.Add(button);
		verticalStackLayout.Add(webView);



		Content = verticalStackLayout;
	}
}