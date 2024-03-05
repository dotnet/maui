using System;

namespace Microsoft.Maui.Platform;

public class MauiWebView : WebKit.WebView, IWebViewDelegate
{

	public const string AssetBaseUrl = "file:///gtk_asset/";
	readonly WebViewHandler _handler;

	public MauiWebView(WebViewHandler handler) : base()
	{
		_handler = handler ?? throw new ArgumentNullException(nameof(handler));
	}

	void IWebViewDelegate.LoadHtml(string? html, string? baseUrl)
	{
		_handler.CurrentNavigationEvent = WebNavigationEvent.NewPage;

		LoadHtml(html ?? string.Empty, baseUrl ?? AssetBaseUrl);
	}

	void IWebViewDelegate.LoadUrl(string? url)
	{
		if (!_handler.NavigatingCanceled(url))
		{
			if (_handler != null)
			{
				_handler.CurrentNavigationEvent = WebNavigationEvent.NewPage;
			}

			if (url != null && !url.StartsWith('/') && !System.Uri.IsWellFormedUriString(url, UriKind.Absolute))
			{
				// URLs like "index.html" can't possibly load, so try "file:///android_asset/index.html"
				url = AssetBaseUrl + url;
			}

			LoadUri(url ?? string.Empty);
		}
	}

}