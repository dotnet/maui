using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Webkit;

namespace Microsoft.Maui.Platform
{
	public class MauiWebView : WebView, IWebViewDelegate
	{
		public const string AssetBaseUrl = "file:///android_asset/";

		readonly WebViewHandler _handler;
		readonly Rect _clipRect;

		// True after the first layout pass where exactly one dimension is positive and the other is zero.
		// Auto-sizing layouts produce this intermediate state; a zero-area ClipBounds here
		// causes RenderThread to crash on an incomplete Skia canvas (SIGSEGV).
		// https://github.com/dotnet/maui/issues/35771
		bool _isAutoSizing;

		public MauiWebView(WebViewHandler handler, Context context) : base(context)
		{
			_handler = handler ?? throw new ArgumentNullException(nameof(handler));

			// Initialize with empty clip bounds to prevent the WebView from briefly
			// rendering at full screen size before layout is complete.
			// https://github.com/dotnet/maui/issues/31475
			_clipRect = new Rect(0, 0, 0, 0);
			ClipBounds = _clipRect;
		}

		protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
		{
			base.OnSizeChanged(width, height, oldWidth, oldHeight);
			UpdateClipBounds(width, height);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			// Re-evaluate ClipBounds when re-parented (e.g., wrapped in WrapperView for shadow)
			UpdateClipBounds(Width, Height);
		}

		void UpdateClipBounds(int width, int height)
		{
			// Auto-sizing layouts produce an intermediate layout pass where exactly one dimension
			// is positive and the other is zero: vertical layouts give (w>0, h=0) first; horizontal
			// layouts give (w=0, h>0) first. A zero-area ClipBounds in either state causes
			// RenderThread to crash (SIGSEGV). Null disables clipping; the latch prevents later
			// layout passes from re-enabling it before both dimensions are stable.
			// https://github.com/dotnet/maui/issues/35771
			if (_isAutoSizing || (width > 0 && height == 0) || (width == 0 && height > 0))
			{
				_isAutoSizing = true;
				ClipBounds = null;
				return;
			}

			// Normal (non-auto-sizing) WebView: apply flash prevention from issue #31475.
			if (width > 0 && height > 0)
			{
				if (Parent is WrapperView)
				{
					// Parent is WrapperView (shadow/border/clip applied).
					// Remove ClipBounds to allow visual effects like shadows
					// to render outside the view area.
					ClipBounds = null;
				}
				else
				{
					// No WrapperView — apply exact bounds to prevent the WebView
					// from briefly rendering at full screen size before layout.
					_clipRect.Set(0, 0, width, height);
					ClipBounds = _clipRect;
				}
			}
			else
			{
				// View has no area yet or is fully collapsed — keep a zero clip rect.
				_clipRect.Set(0, 0, 0, 0);
				ClipBounds = _clipRect;
			}
		}

		public override bool OnTouchEvent(MotionEvent? e)
		{
			if (e == null)
				return false;

			switch (e.Action)
			{
				case MotionEventActions.Down:
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