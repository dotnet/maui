using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.Maui.Platform
{
	[RequiresUnreferencedCode(HybridWebViewHandler.DynamicFeatures)]
#if !NETSTANDARD
	[RequiresDynamicCode(HybridWebViewHandler.DynamicFeatures)]
#endif
	public partial class MauiHybridWebView : WebView2, IHybridPlatformWebView
	{
		private readonly WeakReference<HybridWebViewHandler> _handler;
		internal Task<bool>? WebViewReadyTask { get; set; }

		public MauiHybridWebView(HybridWebViewHandler handler)
		{
			ArgumentNullException.ThrowIfNull(handler, nameof(handler));
			_handler = new WeakReference<HybridWebViewHandler>(handler);
		}

		public void SendRawMessage(string rawMessage)
		{
			// WebView2's PostWebMessageAsString marshals to a null-terminated LPCWSTR, so any embedded
			// NUL character would truncate the message. URL-encode the payload so it survives; the JS
			// transport decodes it in the WebView2 'message' event listener in hybridwebview.js.
			CoreWebView2.PostWebMessageAsString(Uri.EscapeDataString(rawMessage));
		}

		public async void RunAfterInitialize(Action action)
		{
			var isWebViewInitialized = await WebViewReadyTask!;

			if (isWebViewInitialized)
			{
				action();
			}
		}
	}
}
