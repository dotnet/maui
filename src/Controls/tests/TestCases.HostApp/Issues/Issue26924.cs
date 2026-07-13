namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26924, "Font Size of span Element Not Rendering Correctly in Mobile Mode in .NET MAUI Blazor", PlatformAffected.All)]
	public class Issue26924 : TestContentPage
	{
		WebView _webView;
		Label _resultLabel;
		Button _checkButton;

		const string SmallFontSpanId = "smallFontSpan";
		const string paraFontSpanId = "paraFontSpanId";
		internal const string SmallFontCssValue = "4.87761px";

		protected override void Init()
		{
			var html = $@"
				<!DOCTYPE html>
				<html>
					<head></head>
					<body>
					<p>Hii this is Para </p>
						<span id=""{SmallFontSpanId}"" style=""font-size:{SmallFontCssValue};"">tiny span text</span>
					</body>
				</html>";

			_webView = new WebView
			{
				Source = new HtmlWebViewSource { Html = html },
				HeightRequest = 100,
				AutomationId = "SmallFontWebView"
			};

			_resultLabel = new Label
			{
				Text = "Not checked yet",
				AutomationId = "ComputedFontSizeLabel"
			};

			_checkButton = new Button
			{
				Text = "Get computed font size of span",
				AutomationId = "CheckFontSizeButton",
				Command = new Command(async () => await CheckComputedFontSizeAsync())
			};

			// Update the result label once the WebView finishes loading the HTML, so the test
			// can wait for "WebView loaded" before tapping the button instead of racing navigation.
			_webView.Navigated += (_, __) => _resultLabel.Text = "WebView loaded";

			Content = new VerticalStackLayout
			{
				Children =
				{
					new Label { Text = $"Expected computed font-size to stay close to {SmallFontCssValue} (i.e. under 8px), not be clamped up by the platform WebView's minimum font size." },
					_webView,
					_checkButton,
					_resultLabel
				}
			};
		}

		async Task CheckComputedFontSizeAsync()
		{
			var computedFontSize = await _webView.EvaluateJavaScriptAsync(
				$"window.getComputedStyle(document.getElementById('{SmallFontSpanId}')).fontSize");

			_resultLabel.Text = computedFontSize?.Trim('"') ?? string.Empty;
		}
	}
}
