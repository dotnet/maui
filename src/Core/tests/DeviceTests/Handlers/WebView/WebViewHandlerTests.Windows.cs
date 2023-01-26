using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WebViewHandlerTests
	{
		[Fact(DisplayName = "Disconnect Handler close the Platform WebView2")]
		public async Task DisconnectHandlerCloseWebView()
		{
			var webView = new WebViewStub()
			{
				Source = new UrlWebViewSourceStub { Url = "https://dotnet.microsoft.com/" }
			};

			var handler = await CreateHandlerAsync(webView);

			var coreWebView2 = await InvokeOnMainThreadAsync(() =>
			{
				var platformView = (WebView2)webView.Handler.PlatformView;
				webView.Handler.DisconnectHandler();
				return platformView.CoreWebView2;
			});

			Assert.Null(coreWebView2);
		}

		WebView2 GetNativeWebView(WebViewHandler webViewHandler) =>
			webViewHandler.PlatformView;

		string GetNativeSource(WebViewHandler webViewHandler)
		{
			var plaformWebView = GetNativeWebView(webViewHandler);
			return plaformWebView.Source.AbsoluteUri;
		}
	}
}
