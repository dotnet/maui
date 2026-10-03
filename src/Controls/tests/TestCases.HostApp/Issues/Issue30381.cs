using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30381, "WebView GoBack/GoForward not working for HtmlWebViewSource on iOS", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue30381 : ContentPage
{
	WebView MyWebView;
	Label CanGoBackLabel;
	Label CanGoForwardLabel;
	Button ClickLinkButton;
	Button GoBackButton;
	Button GoForwardButton;
	Button UpdateStatusButton;
	Label PageTitleLabel;
	CancellationTokenSource _serverCancellation;

	public Issue30381()
	{
		CreateUI();
		MyWebView.Navigated += OnWebViewNavigated;
		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
	}

	void CreateUI()
	{
		// Create all UI elements in code
		var instructionLabel = new Label
		{
			Text = "Click the link in WebView to navigate, then test CanGoForward",
			AutomationId = "InstructionLabel",
			FontAttributes = FontAttributes.Bold
		};

		MyWebView = new WebView
		{
			AutomationId = "TestWebView",
			HeightRequest = 300
		};

		GoBackButton = new Button
		{
			Text = "Go Back",
			AutomationId = "GoBackButton"
		};
		GoBackButton.Clicked += OnGoBackClicked;

		GoForwardButton = new Button
		{
			Text = "Go Forward",
			AutomationId = "GoForwardButton"
		};
		GoForwardButton.Clicked += OnGoForwardClicked;

		ClickLinkButton = new Button
		{
			Text = "Click Link",
			AutomationId = "ClickLinkButton",
			IsEnabled = false
		};
		ClickLinkButton.Clicked += OnClickLinkClicked;

		UpdateStatusButton = new Button
		{
			Text = "Get Status",
			AutomationId = "UpdateStatusButton"
		};
		UpdateStatusButton.Clicked += OnUpdateStatusClicked;

		var buttonLayout = new HorizontalStackLayout
		{
			Spacing = 10,
			Children = { GoBackButton, GoForwardButton, ClickLinkButton, UpdateStatusButton }
		};

		CanGoBackLabel = new Label
		{
			AutomationId = "CanGoBackLabel",
			Text = "CanGoBack: False"
		};

		CanGoForwardLabel = new Label
		{
			AutomationId = "CanGoForwardLabel",
			Text = "CanGoForward: False"
		};

		PageTitleLabel = new Label
		{
			AutomationId = "PageTitleLabel",
			Text = "Loading"
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				instructionLabel,
				MyWebView,
				buttonLayout,
				CanGoBackLabel,
				CanGoForwardLabel,
				PageTitleLabel,
			}
		};
	}

	async void OnLoaded(object sender, EventArgs e)
	{
		if (_serverCancellation is { IsCancellationRequested: false })
			return;

		using var cancellation = new CancellationTokenSource();
		_serverCancellation = cancellation;
		var listener = new TcpListener(IPAddress.Loopback, 0);

		try
		{
			listener.Start();
			var port = ((IPEndPoint)listener.LocalEndpoint).Port;
			SetupWebView($"http://127.0.0.1:{port}/");

			// Keep a real URL in native history without depending on an external site.
			const string html = "<html><head><title>Linked page</title></head><body>Linked page</body></html>";
			var response = Encoding.UTF8.GetBytes(
				$"HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: {Encoding.UTF8.GetByteCount(html)}\r\nConnection: close\r\n\r\n{html}");

			while (true)
			{
				using var client = await listener.AcceptTcpClientAsync(cancellation.Token);
				using var stream = client.GetStream();
				using var reader = new StreamReader(stream, leaveOpen: true);
				string line;
				do
				{
					line = await reader.ReadLineAsync(cancellation.Token);
				}
				while (!string.IsNullOrEmpty(line));

				if (line is not null)
					await stream.WriteAsync(response, cancellation.Token);
			}
		}
		catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
		{
		}
		finally
		{
			listener.Stop();
			if (ReferenceEquals(_serverCancellation, cancellation))
				_serverCancellation = null;
		}
	}

	void OnUnloaded(object sender, EventArgs e)
	{
		_serverCancellation?.Cancel();
	}

	void SetupWebView(string linkedPageUrl)
	{
		MyWebView.Source = new HtmlWebViewSource
		{
			Html = $"""
                <html>
                <head><title>Inline page</title></head>
                <body>
                    <h1>Inline page</h1>
                    <a href="{linkedPageUrl}" id="testLink">Open linked page</a>
                </body>
                </html>
                """
		};

		UpdateNavigationStatus();
	}

	async void OnClickLinkClicked(object sender, EventArgs e)
	{
		PageTitleLabel.Text = "Loading";
		await MyWebView.EvaluateJavaScriptAsync("document.getElementById('testLink').click();");
	}

	void OnUpdateStatusClicked(object sender, EventArgs e)
	{
		UpdateNavigationStatus();
	}

	void OnGoBackClicked(object sender, EventArgs e)
	{
		if (MyWebView.CanGoBack)
		{
			PageTitleLabel.Text = "Loading";
			MyWebView.GoBack();
		}
	}

	void OnGoForwardClicked(object sender, EventArgs e)
	{
		if (MyWebView.CanGoForward)
		{
			PageTitleLabel.Text = "Loading";
			MyWebView.GoForward();
		}
	}

	void UpdateNavigationStatus()
	{
		CanGoBackLabel.Text = $"CanGoBack: {MyWebView.CanGoBack}";
		CanGoForwardLabel.Text = $"CanGoForward: {MyWebView.CanGoForward}";
		ClickLinkButton.IsEnabled = PageTitleLabel.Text == "Inline page";
	}

	async void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
	{
		PageTitleLabel.Text = e.Result == WebNavigationResult.Success
			? (await MyWebView.EvaluateJavaScriptAsync("document.title")).Trim('"')
			: $"Navigation failed: {e.Result}";
		UpdateNavigationStatus();
	}
}