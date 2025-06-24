using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.AspNetCore.Components.WebView.Maui.PlatformConfiguration.iOSSpecific;
using WebKit;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Extensions for configuring BlazorWebView on iOS.
	/// </summary>
	public static class BlazorWebViewExtensions
	{
		/// <summary>
		/// Updates the scroll bounce property of the WKWebView.
		/// </summary>
		/// <param name="platformView">The platform WKWebView.</param>
		/// <param name="blazorWebView">The BlazorWebView.</param>
		public static void UpdateIsScrollBounceEnabled(this WKWebView platformView, BlazorWebView blazorWebView)
		{
			var isScrollBounceEnabled = blazorWebView.OnThisPlatform().IsScrollBounceEnabled();
			
			// WKWebView has a scrollView property that exposes the underlying UIScrollView
			if (platformView.ScrollView != null)
			{
				// Control the bounce/elastic scrolling behavior
				platformView.ScrollView.Bounces = isScrollBounceEnabled;
				platformView.ScrollView.AlwaysBounceVertical = isScrollBounceEnabled;
				platformView.ScrollView.AlwaysBounceHorizontal = isScrollBounceEnabled;
			}
		}
	}
}