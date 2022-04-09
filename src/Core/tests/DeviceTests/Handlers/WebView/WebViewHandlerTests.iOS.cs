using Microsoft.Maui.Handlers;
using WebKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WebViewHandlerTests
	{
		WKWebView GetNativeWebView(WebViewHandler webViewHandler) =>
			webViewHandler.PlatformView;

		string GetNativeSource(WebViewHandler webViewHandler) =>
			GetNativeWebView(webViewHandler).Url.AbsoluteString;
	}
}