using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WebViewTests
	{
		[Fact(DisplayName = "HTML content is restored after the web content process terminates")]
		public async Task HtmlContentIsRestoredAfterWebContentProcessTerminates()
		{
			SetupBuilder();

			var webView = new WebView
			{
				WidthRequest = 100,
				HeightRequest = 100,
				Source = new HtmlWebViewSource { Html = "<html><body><p id='content'>Issue39973</p></body></html>" }
			};

			var navigated = new TaskCompletionSource<WebNavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
			var terminated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			webView.Navigated += (_, e) => navigated.TrySetResult(e.Result);
			webView.ProcessTerminated += (_, _) => terminated.TrySetResult();

			await AttachAndRun<WebViewHandler>(webView, async handler =>
			{
				Assert.Equal(WebNavigationResult.Success, await navigated.Task.WaitAsync(TimeSpan.FromSeconds(5)));

				navigated = new TaskCompletionSource<WebNavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
				KillWebContentProcess(handler.PlatformView);

				await terminated.Task.WaitAsync(TimeSpan.FromSeconds(5));
				Assert.Equal(WebNavigationResult.Success, await navigated.Task.WaitAsync(TimeSpan.FromSeconds(5)));
				Assert.Equal("Issue39973", await webView.EvaluateJavaScriptAsync("document.getElementById('content').textContent"));

				// A second termination right after the reload must not reload again, so a page that keeps crashing can't loop
				terminated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				navigated = new TaskCompletionSource<WebNavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
				KillWebContentProcess(handler.PlatformView);

				await terminated.Task.WaitAsync(TimeSpan.FromSeconds(5));
				await Task.Delay(1000);
				Assert.False(navigated.Task.IsCompleted);
			});
		}

		[Fact(DisplayName = "HTML source keeps a page it navigated to after the web content process terminates")]
		public async Task HtmlSourceKeepsNavigatedPageAfterWebContentProcessTerminates()
		{
			SetupBuilder();

			var webView = new WebView
			{
				WidthRequest = 100,
				HeightRequest = 100,
				Source = new HtmlWebViewSource { Html = "<html><body><p id='content'>Issue39973</p></body></html>" }
			};

			var navigated = new TaskCompletionSource<WebNavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
			webView.Navigated += (_, e) => navigated.TrySetResult(e.Result);

			await AttachAndRun<WebViewHandler>(webView, async handler =>
			{
				Assert.Equal(WebNavigationResult.Success, await navigated.Task.WaitAsync(TimeSpan.FromSeconds(5)));

				// Leave the HTML document, as following a link does, while Source stays the HTML
				var page = NSBundle.MainBundle.GetUrlForResource("index", "html", "HybridTestRoot");
				navigated = new TaskCompletionSource<WebNavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
				handler.PlatformView.LoadFileUrl(page, page);
				Assert.Equal(WebNavigationResult.Success, await navigated.Task.WaitAsync(TimeSpan.FromSeconds(5)));

				navigated = new TaskCompletionSource<WebNavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
				KillWebContentProcess(handler.PlatformView);

				Assert.Equal(WebNavigationResult.Success, await navigated.Task.WaitAsync(TimeSpan.FromSeconds(5)));
				Assert.Equal(page.AbsoluteString, handler.PlatformView.Url?.AbsoluteString);
			});
		}

		[Fact(DisplayName = "URL source keeps its current page after the web content process terminates")]
		public async Task UrlSourceKeepsCurrentPageAfterWebContentProcessTerminates()
		{
			SetupBuilder();

			var webView = new WebView
			{
				WidthRequest = 100,
				HeightRequest = 100,
				Source = new UrlWebViewSource { Url = "HybridTestRoot/index.html" }
			};

			var navigated = new TaskCompletionSource<WebNavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
			webView.Navigated += (_, e) => navigated.TrySetResult(e.Result);

			await AttachAndRun<WebViewHandler>(webView, async handler =>
			{
				Assert.Equal(WebNavigationResult.Success, await navigated.Task.WaitAsync(TimeSpan.FromSeconds(5)));

				// Move to a new history entry, so reloading the source would lose it but reloading the current page keeps it
				await webView.EvaluateJavaScriptAsync("location.hash = 'second'");
				await AssertHelpers.AssertEventually(() => handler.PlatformView.Url?.Fragment == "second");

				navigated = new TaskCompletionSource<WebNavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
				KillWebContentProcess(handler.PlatformView);

				Assert.Equal(WebNavigationResult.Success, await navigated.Task.WaitAsync(TimeSpan.FromSeconds(5)));
				Assert.Equal("second", handler.PlatformView.Url?.Fragment);
			});
		}

		// WebKit SPI that ends the web content process, as iOS does when it reclaims memory from an idle app.
		// The shorter _killWebContentProcess does nothing on macOS 26, so Mac Catalyst needs this variant.
		static void KillWebContentProcess(WebKit.WKWebView webView)
		{
			var kill = new Selector("_killWebContentProcessAndResetState");
			Assert.True(webView.RespondsToSelector(kill));
			webView.PerformSelector(kill, null, 0);
		}
	}
}
