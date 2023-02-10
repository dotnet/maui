using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WebViewHandlerTests
	{
		[Fact(DisplayName = "Closing Window With WebView Doesnt Crash")]
		public async Task ClosingWindowWithWebViewDoesntCrash()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.Services.AddSingleton(typeof(UI.Xaml.Window), (services) => new UI.Xaml.Window());
			});

			var webView = new WebViewStub()
			{
				Source = new UrlWebViewSourceStub { Url = "https://dotnet.microsoft.com/" }
			};

			var handler = await CreateHandlerAsync(webView);

			await InvokeOnMainThreadAsync(async () =>
			{
				TaskCompletionSource navigationComplete = new TaskCompletionSource();
				handler.PlatformView.NavigationCompleted += (_, _) =>
				{
					navigationComplete?.SetResult();
					navigationComplete = null;
				};

				await handler.PlatformView.AttachAndRun(async () =>
				{
					await handler.PlatformView.OnLoadedAsync();
					await navigationComplete.Task;
				}, MauiContext);
			});
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
