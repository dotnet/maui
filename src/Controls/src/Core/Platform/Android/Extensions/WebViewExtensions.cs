#nullable disable
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using AMixedContentHandling = Android.Webkit.MixedContentHandling;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Controls.Platform
{
	public static class WebViewExtensions
	{
		public static void UpdateMixedContentMode(this AWebView platformView, WebView webView)
		{
			platformView.Settings.MixedContentMode = (AMixedContentHandling)webView.OnThisPlatform().MixedContentMode();
		}

		public static void UpdateEnableZoomControls(this AWebView platformView, WebView webView)
		{
			var value = webView.OnThisPlatform().ZoomControlsEnabled();
			platformView.Settings.SetSupportZoom(value);
			platformView.Settings.BuiltInZoomControls = value;
		}

		public static void UpdateDisplayZoomControls(this AWebView platformView, WebView webView)
		{
			platformView.Settings.DisplayZoomControls = webView.OnThisPlatform().ZoomControlsDisplayed();
		}

		public static void UpdateJavaScriptEnabled(this AWebView platformView, WebView webView)
		{
			platformView.Settings.JavaScriptEnabled = webView.OnThisPlatform().IsJavaScriptEnabled();
		}
	}
}
