using System;
#if IOS || MACCATALYST
using PlatformSettings = WebKit.WKWebViewConfiguration;
#elif ANDROID
using PlatformSettings = Android.Webkit.WebSettings;
#elif WINDOWS
using PlatformSettings = Microsoft.Web.WebView2.Core.CoreWebView2Settings;
#else
using PlatformSettings = System.Object;
#endif

namespace Microsoft.Maui;

public class WebViewInitializingEventArgs : EventArgs
{
	public Action<PlatformSettings>? AdditionalSettings { get; set; }

	public WebViewInitializingEventArgs()
	{
	}
}
