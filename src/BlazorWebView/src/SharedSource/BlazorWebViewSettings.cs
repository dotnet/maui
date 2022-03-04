using System;

#if WEBVIEW2_WINFORMS
namespace Microsoft.AspNetCore.Components.WebView.WindowsForms
#elif WEBVIEW2_WPF
namespace Microsoft.AspNetCore.Components.WebView.Wpf
#elif WEBVIEW2_MAUI
namespace Microsoft.AspNetCore.Components.WebView.Maui
#else
#error Must define WEBVIEW2_WINFORMS, WEBVIEW2_WPF, WEBVIEW2_MAUI
#endif
{
	/// <summary>
	/// Settings related to the underlying BlazorWebView configuration.
	/// </summary>
	public class BlazorWebViewSettings
	{
		/// <summary>
		/// Gets or sets whether the underlying WebView needs to be initialized in Development mode and
		/// enable features specific to development scenarios like the ability to open developer tools.
		/// </summary>
		public bool DevelopmentMode { get; set; }
	}
}
