using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Webkit;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WebViewHandlerTests
	{
		AWebView GetNativeWebView(WebViewHandler webViewHandler) =>
			webViewHandler.PlatformView;

		string GetNativeSource(WebViewHandler webViewHandler) =>
			GetNativeWebView(webViewHandler).Url;
	}
}