using Microsoft.Maui.Media;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30010, "Loading the captured screenshot from webview content to Image control does not visible", PlatformAffected.Android)]
public class Issue30010 : ContentPage
{
	readonly Image _resultImage;
	readonly Label _statusLabel;
	readonly Button _screenshotButton;

	public Issue30010()
	{
		_statusLabel = new Label
		{
			Text = "Waiting for WebView to load...",
			AutomationId = "StatusLabel",
			Margin = new Thickness(12)
		};

		var webView = new WebView
		{
			HeightRequest = 200,
			Source = new HtmlWebViewSource
			{
				Html = @"<HTML><BODY><H1>.NET MAUI</H1><P>Welcome to WebView.</P></BODY></HTML>"
			}
		};
		webView.Navigated += (s, e) =>
		{
			_statusLabel.Text = "WebView loaded";
			_screenshotButton.IsEnabled = true;
		};

		_screenshotButton = new Button
		{
			Text = "Take Screenshot",
			AutomationId = "TakeScreenshotButton",
			IsEnabled = false
		};
		_screenshotButton.Clicked += OnTakeScreenshotClicked;

		_resultImage = new Image
		{
			AutomationId = "ResultImage",
			HeightRequest = 300
		};

		Content = new VerticalStackLayout
		{
			Children =
			{
				new Label { Text = "Issue 30010", FontAttributes = FontAttributes.Bold, Margin = new Thickness(12) },
				_statusLabel,
				webView,
				_screenshotButton,
				_resultImage
			}
		};
	}

	async void OnTakeScreenshotClicked(object? sender, EventArgs e)
	{
		try
		{
			var screenshot = await Screenshot.CaptureAsync();
			var stream = await screenshot.OpenReadAsync(ScreenshotFormat.Png);

			// Read into a byte array so the stream can be consumed multiple times
			using var ms = new MemoryStream();
			await stream.CopyToAsync(ms);
			var bytes = ms.ToArray();

			_resultImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
			_statusLabel.Text = "Screenshot captured";
		}
		catch (Exception ex)
		{
			_statusLabel.Text = $"Error: {ex.Message}";
		}
	}
}
