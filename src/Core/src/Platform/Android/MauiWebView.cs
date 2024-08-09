using System;
using System.Web;
using Android.Content;
using Android.Webkit;

namespace Microsoft.Maui.Platform
{
	public class MauiWebView : WebView, IWebViewDelegate
	{
		public const string AssetBaseUrl = "file:///android_asset/";

		readonly WebViewHandler _handler;

		public MauiWebView(WebViewHandler handler, Context context) : base(context)
		{
			_handler = handler ?? throw new ArgumentNullException(nameof(handler));
		}

		void IWebViewDelegate.LoadHtml(string? html, string? baseUrl)
		{
			if (_handler != null)
				_handler.CurrentNavigationEvent = WebNavigationEvent.NewPage;

			LoadDataWithBaseURL(baseUrl ?? AssetBaseUrl, html ?? string.Empty, "text/html", "UTF-8", null);
		}

		void IWebViewDelegate.LoadUrl(string? url)
		{
			if (!_handler.NavigatingCanceled(url))
			{
				if (_handler != null)
				{
					_handler.CurrentNavigationEvent = WebNavigationEvent.NewPage;
				}

				string? encodedUrl = url;
				if (!string.IsNullOrEmpty(encodedUrl))
				{
					int questionMarkIndex = encodedUrl.IndexOf('?', StringComparison.InvariantCulture);

					if (questionMarkIndex != -1)
					{
						string baseUrl = encodedUrl.Substring(0, questionMarkIndex + 1);
						string queryString = encodedUrl.Substring(questionMarkIndex + 1);

						// URI encode the part after the '?'
						string encodedPart = HttpUtility.UrlEncode(queryString);
						encodedUrl = baseUrl + encodedPart;
					}
				}

				if (url != null && !url.StartsWith('/') && !Uri.IsWellFormedUriString(encodedUrl, UriKind.Absolute))
				{
					// URLs like "index.html" can't possibly load, so try "file:///android_asset/index.html"
					url = AssetBaseUrl + url;
				}

				LoadUrl(url ?? string.Empty);
			}
		}
	}
}