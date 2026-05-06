using System;
using System.Diagnostics.CodeAnalysis;
using Android.Content;
using Android.Graphics;
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
		readonly Rect _clipRect;

		public MauiHybridWebView(HybridWebViewHandler handler, Context context) : base(context)
		{
			ArgumentNullException.ThrowIfNull(handler, nameof(handler));
			_handler = new WeakReference<HybridWebViewHandler>(handler);

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

		// OnAttachedToWindow — calls Attach(this) when inside a SwipeRefreshLayout.
		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			// Re-evaluate ClipBounds when re-parented (e.g., wrapped in WrapperView for shadow)
			UpdateClipBounds(Width, Height);

			if (IsInsideSwipeRefreshLayout())
			{
				RefreshViewWebViewScrollCapture.Attach(this);
				// If a page has already loaded before this HybridWebView was placed inside a
				// RefreshView (late-attach), the observer was never injected. Re-inject now.
				if (!string.IsNullOrEmpty(Url))
				{
					RefreshViewWebViewScrollCapture.InjectObserver(this);
				}
			}
		}

		// OnDetachedFromWindow — calls Detach().
		protected override void OnDetachedFromWindow()
		{
			RefreshViewWebViewScrollCapture.Detach(this);
			base.OnDetachedFromWindow();
		}

		// IsInsideSwipeRefreshLayout — walks parent view tree.
		bool IsInsideSwipeRefreshLayout()
		{
			var parent = Parent;
			while (parent is not null)
			{
				if (parent is MauiSwipeRefreshLayout)
				{
					return true;
				}
				parent = parent.Parent;
			}
			return false;
		}

		void UpdateClipBounds(int width, int height)
		{
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
				// Re-apply empty clip bounds when the view becomes zero-sized or hidden.
				_clipRect.Set(0, 0, 0, 0);
				ClipBounds = _clipRect;
			}
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
				RefreshViewWebViewScrollCapture.Detach(this);
			}

			base.Dispose(disposing);
		}
	}
}
