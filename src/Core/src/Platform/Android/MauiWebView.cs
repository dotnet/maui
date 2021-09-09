using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Webkit;

namespace Microsoft.Maui
{
	public class MauiWebView : WebView, IWebViewDelegate
	{
		public const string AssetBaseUrl = "file:///android_asset/";

		WebViewClient? _webViewClient;
		WebChromeClient? _webChromeClient;

		public MauiWebView(Context context) : base(context)
		{

			_webViewClient = GetWebViewClient();
			SetWebViewClient(_webViewClient);

			_webChromeClient = GetWebChromeClient();
			SetWebChromeClient(_webChromeClient);
		}

		void IWebViewDelegate.LoadHtml(string? html, string? baseUrl)
		{
			LoadDataWithBaseURL(baseUrl ?? AssetBaseUrl, html ?? string.Empty, "text/html", "UTF-8", null);
		}

		void IWebViewDelegate.LoadUrl(string? url)
		{
			LoadUrl(url ?? string.Empty);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_webViewClient?.Dispose();
				_webChromeClient?.Dispose();
			}
			base.Dispose(disposing);
		}

		protected virtual WebViewClient GetWebViewClient() =>
			new WebViewClient();

		protected virtual WebChromeClient GetWebChromeClient() =>
			new WebChromeClient();
	}
}
