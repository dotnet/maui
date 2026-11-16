using System;
using System.Threading.Tasks;
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

				// WebKit SPI that ends the web content process, as iOS does when it reclaims memory from an idle app
				var kill = new Selector("_killWebContentProcess");
				Assert.True(handler.PlatformView.RespondsToSelector(kill));

				navigated = new TaskCompletionSource<WebNavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
				handler.PlatformView.PerformSelector(kill, null, 0);

				await terminated.Task.WaitAsync(TimeSpan.FromSeconds(5));
				Assert.Equal(WebNavigationResult.Success, await navigated.Task.WaitAsync(TimeSpan.FromSeconds(5)));
				Assert.Equal("Issue39973", await webView.EvaluateJavaScriptAsync("document.getElementById('content').textContent"));

				// A second termination right after the reload must not reload again, so a page that keeps crashing can't loop
				terminated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				navigated = new TaskCompletionSource<WebNavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
				handler.PlatformView.PerformSelector(kill, null, 0);

				await terminated.Task.WaitAsync(TimeSpan.FromSeconds(5));
				await Task.Delay(1000);
				Assert.False(navigated.Task.IsCompleted);
			});
		}
	}
}
