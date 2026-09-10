using System;
using System.Diagnostics.CodeAnalysis;
using Android.Content;
using Android.OS;
using Android.Webkit;
using AUri = Android.Net.Uri;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Platform
{
	[RequiresUnreferencedCode(HybridWebViewHandler.DynamicFeatures)]
#if !NETSTANDARD
	[RequiresDynamicCode(HybridWebViewHandler.DynamicFeatures)]
#endif
	public class MauiHybridWebView : AWebView, IHybridPlatformWebView
	{
		private readonly WeakReference<HybridWebViewHandler> _handler;
		private static readonly AUri AndroidAppOriginUri = AUri.Parse(HybridWebViewHandler.AppOrigin)!;
		volatile bool _detachPending;

		public MauiHybridWebView(HybridWebViewHandler handler, Context context) : base(context)
		{
			ArgumentNullException.ThrowIfNull(handler, nameof(handler));
			_handler = new WeakReference<HybridWebViewHandler>(handler);

			// Pre-register the JS bridge BEFORE any page loads.
			// Android WebView only exposes addJavascriptInterface bindings for pages that
			// start loading AFTER the call is made. If Attach is deferred to
			// OnAttachedToWindow, cold-start apps load their page before the view enters
			// the window hierarchy, so the bridge is invisible to JS.
			// Attach is idempotent, so later calls from OnAttachedToWindow are safe no-ops.
			RefreshViewWebViewScrollCapture.Attach(this);
		}

		protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
		{
			base.OnSizeChanged(width, height, oldWidth, oldHeight);
		}

		// OnAttachedToWindow — calls Attach(this) when inside a SwipeRefreshLayout.
		protected override void OnAttachedToWindow()
		{
			_detachPending = false;

			base.OnAttachedToWindow();

			if (RefreshViewWebViewScrollCapture.IsInsideMauiSwipeRefreshLayout(this))
			{
				RefreshViewWebViewScrollCapture.Attach(this);
				// If a page has already loaded before this HybridWebView was placed inside a
				// RefreshView (late-attach), the observer was never injected. Re-inject now.
				if (!string.IsNullOrEmpty(Url))
				{
					RefreshViewWebViewScrollCapture.InjectObserver(this);
				}
			}
			else
			{
				// Not inside a RefreshView — remove the bridge that was pre-registered
				// in the constructor so it is not exposed to untrusted page content
				// loaded in standalone HybridWebViews.
				RefreshViewWebViewScrollCapture.Detach(this);
			}
		}

		// OnDetachedFromWindow — calls Detach().
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
		}

		public void SendRawMessage(string rawMessage)
		{
#pragma warning disable CA1416 // Validate platform compatibility
			PostWebMessage(new WebMessage(rawMessage), AndroidAppOriginUri);
#pragma warning restore CA1416 // Validate platform compatibility
		}

		// Dispose(bool) — calls Detach() on cleanup.
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
