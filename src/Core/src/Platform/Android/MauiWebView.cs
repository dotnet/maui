using System;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Webkit;

namespace Microsoft.Maui.Platform
{
	public class MauiWebView : WebView, IWebViewDelegate
	{
		public const string AssetBaseUrl = "file:///android_asset/";

		readonly WebViewHandler _handler;
		bool _hasSwipeViewParent;
		volatile bool _detachPending;

		// Tracks whether about:blank was loaded synthetically for layout (null source).
		// MauiWebViewClient clears this entry from the native back stack once the real URL loads,
		// preventing CanGoBack() returning true unexpectedly. Fixes #35788.
		internal bool IsLoadingForLayout { get; set; }

		public MauiWebView(WebViewHandler handler, Context context) : base(context)
		{
			_handler = handler ?? throw new ArgumentNullException(nameof(handler));

			// Pre-register the JS bridge BEFORE any page loads.
			// Android WebView only exposes addJavascriptInterface bindings for pages that
			// start loading AFTER the call is made.  If Attach is deferred to
			// OnAttachedToWindow, cold-start apps (e.g. the Sandbox) load their page before
			// the view enters the window hierarchy, so the bridge is invisible to JS.
			// Attach is idempotent, so later calls from OnAttachedToWindow are safe no-ops.
			RefreshViewWebViewScrollCapture.Attach(this);
		}

		protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
		{
			base.OnSizeChanged(width, height, oldWidth, oldHeight);
		}

		protected override void OnAttachedToWindow()
		{
			_detachPending = false;

			base.OnAttachedToWindow();

			_hasSwipeViewParent = ((View)this).GetParentOfType<MauiSwipeView>() is not null;

			if (RefreshViewWebViewScrollCapture.IsInsideMauiSwipeRefreshLayout(this))
			{
				RefreshViewWebViewScrollCapture.Attach(this);
				// If a page has already loaded before this WebView was placed inside a
				// RefreshView (late-attach), OnPageFinished already fired with IsAttached=false
				// and the observer was never injected. Re-inject it now so inner-scroll can
				// correctly prevent pull-to-refresh.
				if (!string.IsNullOrEmpty(Url))
				{
					RefreshViewWebViewScrollCapture.InjectObserver(this);
				}
			}
			else
			{
				// Not inside a RefreshView — remove the bridge that was pre-registered
				// in the constructor so it is not exposed to untrusted page content
				// loaded in standalone WebViews.
				RefreshViewWebViewScrollCapture.Detach(this);
			}
		}

		protected override void OnDetachedFromWindow()
		{
			if (RefreshViewWebViewScrollCapture.IsAttached(this))
			{
				_detachPending = true;
#pragma warning disable CA1422 // Validate platform compatibility
				new Handler(Looper.MainLooper!).Post(() =>
#pragma warning restore CA1422 // Validate platform compatibility
				{
					if (_detachPending)
					{
						_detachPending = false;
						RefreshViewWebViewScrollCapture.Detach(this);
					}
				});
			}

			base.OnDetachedFromWindow();
			_hasSwipeViewParent = false;
		}

		public override bool OnTouchEvent(MotionEvent? e)
		{
			if (e == null)
				return false;

			switch (e.Action)
			{
				case MotionEventActions.Down:
				case MotionEventActions.Move:
					// Do not request disallow intercept when inside a SwipeView — that would set
					// FLAG_DISALLOW_INTERCEPT on the SwipeView and prevent it from detecting
					// swipe gestures
					if (!_hasSwipeViewParent)
					{
						Parent?.RequestDisallowInterceptTouchEvent(true);
					}
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

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_detachPending = false;
				RefreshViewWebViewScrollCapture.Detach(this);
			}

			base.Dispose(disposing);
		}
	}
}