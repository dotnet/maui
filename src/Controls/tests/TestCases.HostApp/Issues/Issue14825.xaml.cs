#nullable enable
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14825, "Capture WebView screenshot", PlatformAffected.UWP)]
public partial class Issue14825 : ContentPage
{
	public Issue14825()
	{
		InitializeComponent();
	}

	private async void CaptureButton_Clicked(object? sender, EventArgs e)
	{
		IScreenshotResult? result = await myWebView.CaptureAsync();

		if (result != null)
		{
			// Intentionally no "using" because ImageSource requires a valid stream.
			Stream stream = await result.OpenReadAsync(ScreenshotFormat.Png, 100);

			screenshotResult.Add(new Label() { Text = $"Your screenshot ({myWebView.Width}x{myWebView.Height}):" });
			screenshotResult.Add(new Image() { Source = ImageSource.FromStream(() => stream), WidthRequest = myWebView.Width, HeightRequest = myWebView.Height, HorizontalOptions = LayoutOptions.Start });
		}
	}
}