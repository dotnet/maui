#if ANDROID
using Android.Webkit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AWebView = Android.Webkit.WebView;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34392, "MAUI Handler not working with Custom WebView on Android (ShouldOverrideUrlLoading behavior)", PlatformAffected.Android)]
public class Issue34392 : TestContentPage
{
    public static Label StatusIndicator { get; private set; }
    protected override void Init()
    {
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };

        var titleLabel = new Label
        {
            Text = "Custom WebViewClient Test",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Padding = new Thickness(5)
        };
        grid.Add(titleLabel, 0, 0);

        var statusLabel = new Label
        {
            Text = "FAILED",
            AutomationId = "StatusLabel",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Padding = new Thickness(5)
        };
        Grid.SetRow(statusLabel, 1);
        grid.Add(statusLabel);

        var customWebView = new Issue34392_CustomWebView();
        Grid.SetRow(customWebView, 2);
        grid.Add(customWebView);

        Content = grid;

        StatusIndicator = statusLabel;

        customWebView.CustomHeaders = new Dictionary<string, string>
        {
            { "X-Custom-Header", "MyHeaderValue" },
            { "Authorization", "Bearer sample-token-12345" }
        };

        customWebView.Source = new HtmlWebViewSource
        {
            Html = "<html><body><h1>Custom WebView Test</h1><script>setTimeout(function(){ window.location.href = 'https://example.com/test'; }, 500);</script></body></html>"
        };
    }
}

public class Issue34392_CustomWebView : Microsoft.Maui.Controls.WebView
{
    public static readonly BindableProperty CustomHeadersProperty =
        BindableProperty.Create(nameof(CustomHeaders), typeof(Dictionary<string, string>), typeof(Issue34392_CustomWebView), new Dictionary<string, string>());

    public Dictionary<string, string> CustomHeaders
    {
        get => (Dictionary<string, string>)GetValue(CustomHeadersProperty);
        set => SetValue(CustomHeadersProperty, value);
    }
}

#if ANDROID
#nullable enable
public class Issue34392_CustomWebViewHandler : WebViewHandler
{
	protected override void ConnectHandler(AWebView platformView)
	{
		base.ConnectHandler(platformView);

		var customWebView = VirtualView as Issue34392_CustomWebView;
		var headers = customWebView?.CustomHeaders ?? new Dictionary<string, string>();

		platformView.SetWebViewClient(new Issue34392_CustomWebViewClient(this, headers));
	}
}

public class Issue34392_CustomWebViewClient : MauiWebViewClient
{
	private readonly Dictionary<string, string> _headerParams;
	private bool _alreadyRedirected;

	public Issue34392_CustomWebViewClient(WebViewHandler handler, Dictionary<string, string> headers)
		: base(handler)
	{
		_headerParams = headers;
	}

	public override bool ShouldOverrideUrlLoading(AWebView? view, IWebResourceRequest? request)
	{
		var url = request?.Url?.ToString() ?? string.Empty;

		// Update UI to show this method was called
		MainThread.BeginInvokeOnMainThread(() =>
		{
			if (Issue34392.StatusIndicator != null)
			{
				Issue34392.StatusIndicator.Text = "SUCCESS";
			}
		});

		// Prevent infinite redirect loop — only load with custom headers once
		if (_alreadyRedirected)
			return true;

		_alreadyRedirected = true;

		if (_headerParams.Count > 0)
		{
			view?.LoadUrl(url, _headerParams);
		}
		else
		{
			view?.LoadUrl(url);
		}

		return true;
	}
}
#endif

public static class Issue34392Extensions
{
    public static MauiAppBuilder Issue34392AddHandlers(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
#if ANDROID
            handlers.AddHandler<Issue34392_CustomWebView, Issue34392_CustomWebViewHandler>();
#endif
        });

        return builder;
    }
}
