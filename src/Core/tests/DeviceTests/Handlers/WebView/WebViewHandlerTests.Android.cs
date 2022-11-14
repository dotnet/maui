using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WebViewHandlerTests
	{
		AWebView GetNativeWebView(WebViewHandler webViewHandler) =>
			webViewHandler.PlatformView;

		string GetNativeSource(WebViewHandler webViewHandler) =>
			GetNativeWebView(webViewHandler).Url;

		[Fact(DisplayName = "Control meets basic accessibility requirements")]
		[Category(TestCategory.Accessibility)]
		public async Task PlatformViewIsAccessible()
		{
			var view = new WebViewStub();
			await AssertPlatformViewIsAccessible(view);
		}
	}
}