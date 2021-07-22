using Microsoft.Maui.Handlers;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WebViewHandlerTests
	{
		AWebView GetNativeWebView(WebViewHandler webViewHandler) =>
			webViewHandler.NativeView;

		string GetNativeSource(WebViewHandler webViewHandler) =>
			GetNativeWebView(webViewHandler).Url;
	}
}