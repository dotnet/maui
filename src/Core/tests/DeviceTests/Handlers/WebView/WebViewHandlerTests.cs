using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.WebView)]
	public partial class WebViewHandlerTests : CoreHandlerTestBase<WebViewHandler, WebViewStub>
	{
		[Theory(DisplayName = "UrlSource Initializes Correctly")]
		[InlineData("https://dotnet.microsoft.com/")]
		[InlineData("https://devblogs.microsoft.com/dotnet/")]
		[InlineData("https://xamarin.com/")]
		public async Task UrlSourceInitializesCorrectly(string urlSource)
		{
			var webView = new WebViewStub()
			{
				Source = new UrlWebViewSourceStub { Url = urlSource }
			};

			var url = ((UrlWebViewSourceStub)webView.Source).Url;

			await InvokeOnMainThreadAsync(() => ValidatePropertyInitValue(webView, () => url, GetNativeSource, url));
		}
	}
}