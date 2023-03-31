using System;
using Android.Content;
using Android.Views;
using Android.Webkit;
using AndroidX.ViewPager2.Widget;

namespace Microsoft.Maui.Platform
{
	public class MauiWebView : WebView, IWebViewDelegate
	{
		public const string AssetBaseUrl = "file:///android_asset/";

		readonly WebViewHandler _handler;

		public MauiWebView(WebViewHandler handler, Context context) : base(context)
		{
			_handler = handler ?? throw new ArgumentNullException("handler");
		}

		public override bool OnTouchEvent(MotionEvent? e)
		{
			RequestDisallowInterceptParentTouchEvent();

			return base.OnTouchEvent(e);
		}

		void RequestDisallowInterceptParentTouchEvent()
		{
			if (Parent is null)
				return;

			var viewPager2 = Parent.GetParentOfType<ViewPager2>();

			if (viewPager2 is not null)
				Parent?.RequestDisallowInterceptTouchEvent(HorizontalScrollBarEnabled);
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

				if (url != null && !url.StartsWith('/') && !Uri.IsWellFormedUriString(url, UriKind.Absolute))
				{
					// URLs like "index.html" can't possibly load, so try "file:///android_asset/index.html"
					url = AssetBaseUrl + url;
				}

				LoadUrl(url ?? string.Empty);
			}
		}
	}
}