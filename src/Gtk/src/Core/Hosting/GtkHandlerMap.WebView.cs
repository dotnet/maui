using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Gtk.Hosting
{
	public static partial class GtkHandlerMap
	{
		// WebView and HybridWebView (WebKitGTK). BlazorWebView registers its own pair from Microsoft.AspNetCore.Components.WebView.Maui.Gtk.
		static void AddWebViewEntries()
		{
			Add<WebViewHandler, GtkWebViewHandler>();
			Add<HybridWebViewHandler, GtkHybridWebViewHandler>();

			AddDefault(typeof(IWebView), typeof(GtkWebViewHandler));
			AddDefault(typeof(IHybridWebView), typeof(GtkHybridWebViewHandler));
		}
	}
}
