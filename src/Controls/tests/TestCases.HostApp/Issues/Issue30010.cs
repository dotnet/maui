using Microsoft.Maui.Media;
#if ANDROID
using AColor = Android.Graphics.Color;
using BitmapFactory = Android.Graphics.BitmapFactory;
#endif

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
				Html = @"<HTML><BODY style=""margin:0;background:#00ff00;""><DIV style=""height:200px;background:#00ff00;color:#000;font-size:48px;"">WEBVIEW_MARKER</DIV></BODY></HTML>"
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

	async void OnTakeScreenshotClicked(object sender, EventArgs e)
	{
		try
		{
			var screenshot = await Screenshot.CaptureAsync();
			using var stream = await screenshot.OpenReadAsync(ScreenshotFormat.Png);

			// Read into a byte array so the stream can be consumed multiple times
			using var ms = new MemoryStream();
			await stream.CopyToAsync(ms);
			var bytes = ms.ToArray();

			if (!ContainsWebViewMarker(bytes))
			{
				_statusLabel.Text = "Error: Screenshot missing WebView marker";
				return;
			}

			_resultImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
			_statusLabel.Text = "Screenshot captured";
		}
		catch (Exception ex)
		{
			_statusLabel.Text = $"Error: {ex.Message}";
		}
	}

	static bool ContainsWebViewMarker(byte[] screenshot)
	{
#if ANDROID
		using var bitmap = BitmapFactory.DecodeByteArray(screenshot, 0, screenshot.Length);
		if (bitmap is null)
			return false;

		var markerPixels = 0;
		for (var y = 0; y < bitmap.Height; y += 4)
		{
			for (var x = 0; x < bitmap.Width; x += 4)
			{
				var color = new AColor(bitmap.GetPixel(x, y));
				if (color.G > 200 && color.R < 80 && color.B < 80)
					markerPixels++;

				if (markerPixels > 100)
					return true;
			}
		}
#endif

		return false;
	}
}
