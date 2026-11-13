namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30515, "[iOS] WebView.Reload() with HtmlWebViewSource returns WebNavigationResult.Failure in Navigated event", PlatformAffected.iOS)]
public class Issue30515 : ContentPage
{
	WebView webView;
	Label statusTextLabel;
	Label statusValueLabel;

	public Issue30515()
	{
		Title = "Issue 30515";
		
		// Create the navigation status labels - split into static text and dynamic value
		statusTextLabel = new Label
		{
			Text = "Status:",
			FontSize = 14,
			TextColor = Colors.Gray,
			VerticalOptions = LayoutOptions.Center
		};
		
		statusValueLabel = new Label
		{
			Text = "Ready",
			AutomationId = "NavigationStatusLabel",
			FontSize = 14,
			TextColor = Colors.Blue,
			VerticalOptions = LayoutOptions.Center
		};
		
		// Create a horizontal stack for the status display
		var statusContainer = new StackLayout
		{
			Orientation = StackOrientation.Horizontal,
			Spacing = 5,
			HorizontalOptions = LayoutOptions.Center,
			Children = { statusTextLabel, statusValueLabel }
		};

		// Create the WebView
		webView = new WebView
		{
			HeightRequest = 300,
			WidthRequest = 400,
			Source = new HtmlWebViewSource
			{
				Html = @"
				<html>
				<head>
					<title>HTML WebView Source</title>
				</head>
				<body style='font-family:sans-serif; padding:20px;'>
					<h1>WebView Feature Matrix</h1>
					<p>This page demonstrates various capabilities of the .NET MAUI WebView control, such as:</p>
					<ul>
						<li>Rendering HTML content</li>
						<li>Executing JavaScript</li>
						<li>Cookie management</li>
						<li>Back/Forward navigation</li>
					</ul>
					<h2>Test Content</h2>
					<p>
						This is a longer body paragraph to help test the <strong>EvaluateJavaScript</strong> functionality 
						and how it extracts body text. You can use this text to verify substring operations and test scrolling 
						or formatting in the WebView.
						
					</p>
					<p>
						Try interacting with navigation buttons, loading multiple pages, or checking cookie behavior.
					</p>
					
					<h2>Navigation Test Links</h2>
					<p>Click these links to test URL navigation:</p>
					<ul>
						<li><a href='https://www.google.com'>Google</a></li>
						<li><a href='https://www.microsoft.com'>Microsoft</a></li>
						<li><a href='https://github.com'>GitHub</a></li>
						<li><a href='https://docs.microsoft.com/dotnet/maui/'>MAUI Documentation</a></li>
						<li><a href='https://httpbin.org/html'>HTTPBin HTML Test</a></li>
					</ul>
					
					<footer style='margin-top:40px; font-size:0.9em; color:gray;'>Generated for testing WebView features.</footer>
				</body>
				</html>"
			}
		};
		
		// Set up event handlers
		webView.Navigated += OnWebViewNavigated;
		
		// Create the reload button
		var reloadButton = new Button
		{
			Text = "Reload",
			AutomationId = "ReloadButton",
			HorizontalOptions = LayoutOptions.Center
		};
		reloadButton.Clicked += OnReloadClicked;
		
		// Add all elements to the page
		Content = new StackLayout
		{
			Padding = new Thickness(10),
			Spacing = 10,
			Children =
			{
				statusContainer,
				webView,
				new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					Spacing = 10,
					HorizontalOptions = LayoutOptions.Center,
					Children =
					{
						reloadButton
					}
				}
			}
		};
	}

	void OnReloadClicked(object sender, EventArgs e)
	{
		statusValueLabel.Text = "Reloading...";
		webView.Reload();
	}

	void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
	{
		statusValueLabel.Text = e.Result.ToString();
		statusValueLabel.TextColor = Colors.Green;
	}
}