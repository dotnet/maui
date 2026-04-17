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

			if (width > 0 && height > 0)
			{
				// Remove clip bounds once layout is complete and the view has its
				// correct size. Setting null instead of exact bounds allows visual
				// effects like shadows to render outside the view area.
				ClipBounds = null;
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
	}
}
