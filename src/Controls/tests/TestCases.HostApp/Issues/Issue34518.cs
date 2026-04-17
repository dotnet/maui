namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34518, "WebView background color has changed after update, can't override", PlatformAffected.UWP)]
public class Issue34518 : ContentPage
{
	public Issue34518()
	{
		var htmlSource = new HtmlWebViewSource
		{
			Html = "<html><body></body></html>"
		};

		var webViewGreen = new WebView
		{
			Source = htmlSource,
			BackgroundColor = Colors.Green,
			HeightRequest = 150,
			HorizontalOptions = LayoutOptions.Fill
		};


		Content = new VerticalStackLayout
		{
			Padding = new Thickness(0),
			Spacing = 0,
			Children =
			{
				new Label { AutomationId = "WebViewGreen", Text = "WebView Background should be Green:", Margin = new Thickness(4) },
				webViewGreen
			}
		};
	}
}
