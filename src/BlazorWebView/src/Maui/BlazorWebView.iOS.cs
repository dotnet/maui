#nullable disable
using Microsoft.Maui.Handlers;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebView
	{
#if IOS
		/// <summary>
		/// Maps the IsScrollBounceEnabled property to the platform view.
		/// </summary>
		/// <param name="handler">The BlazorWebViewHandler.</param>
		/// <param name="blazorWebView">The BlazorWebView.</param>
		public static void MapIsScrollBounceEnabled(BlazorWebViewHandler handler, BlazorWebView blazorWebView)
		{
			Microsoft.Maui.Controls.Platform.BlazorWebViewExtensions.UpdateIsScrollBounceEnabled(handler.PlatformView, blazorWebView);
		}
#endif
	}
}