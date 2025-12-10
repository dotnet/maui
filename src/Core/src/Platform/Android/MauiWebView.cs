using System;
using Android.Content;
using Android.Views;
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

		public override bool OnTouchEvent(MotionEvent? e)
		{
			if (e == null)
				return base.OnTouchEvent(e);

			switch (e.Action)
			{
				case MotionEventActions.Down:
					Parent?.RequestDisallowInterceptTouchEvent(true);
					break;

				case MotionEventActions.Move:
					Parent?.RequestDisallowInterceptTouchEvent(true);
					break;

				case MotionEventActions.Up:
				case MotionEventActions.Cancel:
					Parent?.RequestDisallowInterceptTouchEvent(false);
					break;
			}

			return base.OnTouchEvent(e);
		}

		void IWebViewDelegate.LoadHtml(string? html, string? baseUrl)
		{
			_handler?.CurrentNavigationEvent = WebNavigationEvent.NewPage;

			LoadDataWithBaseURL(baseUrl ?? AssetBaseUrl, html ?? string.Empty, "text/html", "UTF-8", null);
		}

		void IWebViewDelegate.LoadUrl(string? url)
		{
			if (!_handler.NavigatingCanceled(url))
			{
				_handler?.CurrentNavigationEvent = WebNavigationEvent.NewPage;

				if (url is not null && !url.StartsWith('/') && !Uri.TryCreate(url, UriKind.Absolute, out _))
				{
					// URLs like "index.html" can't possibly load, so try "file:///android_asset/index.html"
					url = AssetBaseUrl + url;
				}

				LoadUrl(url ?? string.Empty);
			}
		}
	}
}