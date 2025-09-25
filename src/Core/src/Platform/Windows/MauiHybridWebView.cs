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
			CoreWebView2.PostWebMessageAsString(rawMessage);
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
