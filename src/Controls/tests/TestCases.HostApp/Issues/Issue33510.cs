using System.Text;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33510, "[Android] RefreshView triggers pull-to-refresh immediately when scrolling up inside a WebView", PlatformAffected.Android)]
public class Issue33510 : TestContentPage
{
	WebView _webView;
	RefreshView _refreshView;
	Label _statusLabel;
	Label _scrollTopLabel;
	bool _isWebViewLoaded;

	protected override void Init()
	{
		Title = "RefreshView + WebView";

		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Loading WebView..."
		};

		_scrollTopLabel = new Label
		{
			AutomationId = "ScrollTopLabel",
			Text = "ScrollTop: unavailable"
		};

		var scrollWebViewButton = new Button
		{
			AutomationId = "ScrollWebViewButton",
			Text = "Scroll down in WebView"
		};

		scrollWebViewButton.Clicked += async (_, _) =>
		{
			if (!_isWebViewLoaded)
			{
				return;
			}

			var result = await _webView.EvaluateJavaScriptAsync("window.scrollInnerContainerTo(900);");
			_scrollTopLabel.Text = $"ScrollTop: {NormalizeJavaScriptNumber(result)}";
		};

		var readScrollTopButton = new Button
		{
			AutomationId = "ReadScrollTopButton",
			Text = "Read WebView scroll position"
		};

		readScrollTopButton.Clicked += async (_, _) => await UpdateScrollStatusAsync();

		var scrollWebViewToTopButton = new Button
		{
			AutomationId = "ScrollWebViewToTopButton",
			Text = "Scroll WebView to top"
		};

		scrollWebViewToTopButton.Clicked += async (_, _) =>
		{
			if (!_isWebViewLoaded)
			{
				return;
			}

			var result = await _webView.EvaluateJavaScriptAsync("window.scrollInnerContainerTo(0);");
			_scrollTopLabel.Text = $"ScrollTop: {NormalizeJavaScriptNumber(result)}";
		};

		_webView = new WebView
		{
			AutomationId = "TestWebView",
		};

		_webView.Navigated += async (_, _) =>
		{
			_isWebViewLoaded = true;
			_statusLabel.Text = "WebView ready. Scroll down, then drag downward inside the WebView.";
			await UpdateScrollStatusAsync();
		};

		_webView.Source = new HtmlWebViewSource
		{
			Html = CreateHtml()
		};

		var webViewContainer = new ContentView
		{
			AutomationId = "TestWebViewContainer",
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
			Content = _webView
		};

		_refreshView = new RefreshView
		{
			AutomationId = "TestRefreshView",
			Content = webViewContainer
		};

		_refreshView.Command = new Command(async () =>
		{
			_statusLabel.Text = "Refresh triggered";

			await Task.Delay(150);
			_refreshView.IsRefreshing = false;

			await UpdateScrollStatusAsync();
		});

		var controlsLayout = new VerticalStackLayout
		{
			Padding = new Thickness(12),
			Spacing = 8,
			Children =
			{
				new Label
				{
					Text = "This page reproduces issue #33510. On Android, RefreshView should not refresh while the WebView can still scroll upward internally."
				},
				_statusLabel,
				_scrollTopLabel,
				scrollWebViewButton,
				scrollWebViewToTopButton,
				readScrollTopButton
			}
		};

		var separator = new BoxView
		{
			HeightRequest = 1,
			Color = Colors.LightGray
		};

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		grid.Add(controlsLayout);
		Grid.SetRow(controlsLayout, 0);

		grid.Add(separator);
		Grid.SetRow(separator, 1);

		grid.Add(_refreshView);
		Grid.SetRow(_refreshView, 2);

		Content = grid;
	}

	async Task UpdateScrollStatusAsync()
	{
		if (!_isWebViewLoaded)
		{
			_scrollTopLabel.Text = "ScrollTop: unavailable";
			return;
		}

		var result = await _webView.EvaluateJavaScriptAsync("window.getInnerScrollTop();");
		_scrollTopLabel.Text = $"ScrollTop: {NormalizeJavaScriptNumber(result)}";
	}

	static string NormalizeJavaScriptNumber(string result)
	{
		if (string.IsNullOrWhiteSpace(result))
			return "unknown";

		return result.Trim().Trim('"');
	}

	static string CreateHtml()
	{
		var rows = string.Join(Environment.NewLine, Enumerable.Range(1, 30).Select(index =>
			$"<div class='row'>Scrollable row {index}</div>"));

		var html = new StringBuilder();
		html.AppendLine("<!DOCTYPE html>");
		html.AppendLine("<html>");
		html.AppendLine("<head>");
		html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0' />");
		html.AppendLine("<style>");
		html.AppendLine("html, body { margin: 0; padding: 0; height: 100%; overflow: hidden; font-family: sans-serif; background: white; }");
		html.AppendLine(".header { position: fixed; top: 0; left: 0; right: 0; height: 56px; padding: 16px; box-sizing: border-box; background: #0b5fff; color: white; font-weight: 600; z-index: 1; }");
		html.AppendLine("#scroll-container { position: absolute; top: 56px; left: 0; right: 0; bottom: 0; overflow-y: auto; -webkit-overflow-scrolling: touch; background: #f5f5f5; }");
		html.AppendLine(".row { margin: 12px; height: 120px; border-radius: 12px; display: flex; align-items: center; justify-content: center; background: white; border: 1px solid #d0d0d0; }");
		html.AppendLine("</style>");
		html.AppendLine("</head>");
		html.AppendLine("<body>");
		html.AppendLine("<div class='header'>Internal scroll container</div>");
		html.AppendLine("<div id='scroll-container'>");
		html.AppendLine("<div class='row'>Top marker</div>");
		html.AppendLine(rows);
		html.AppendLine("</div>");
		html.AppendLine("<script>");
		html.AppendLine("window.notifyRefreshViewScrollState = function () {");
		html.AppendLine(" if (window.mauiRefreshViewHost && typeof window.mauiRefreshViewHost.setCanScrollUp === 'function') {");
		html.AppendLine(" var container = document.getElementById('scroll-container');");
		html.AppendLine(" window.mauiRefreshViewHost.setCanScrollUp(container.scrollTop > 0);");
		html.AppendLine(" }");
		html.AppendLine("};");
		html.AppendLine("window.getInnerScrollTop = function () {");
		html.AppendLine(" return document.getElementById('scroll-container').scrollTop.toString();");
		html.AppendLine("};");
		html.AppendLine("window.scrollInnerContainerTo = function (value) {");
		html.AppendLine(" var container = document.getElementById('scroll-container');");
		html.AppendLine(" container.scrollTop = value;");
		html.AppendLine(" window.notifyRefreshViewScrollState();");
		html.AppendLine(" return container.scrollTop.toString();");
		html.AppendLine("};");
		html.AppendLine("window.notifyRefreshViewScrollState();");
		html.AppendLine("</script>");
		html.AppendLine("</body>");
		html.AppendLine("</html>");

		return html.ToString();
	}
}
