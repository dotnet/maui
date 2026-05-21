#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.WebView)]
#if WINDOWS
	[Collection(WebViewsCollection)]
#endif
	public partial class WebViewTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task NavigatingEventHasMainFrameTarget()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(async () =>
			{
				var navigatingFired = new TaskCompletionSource<WebNavigatingEventArgs>();

				var webView = new WebView
				{
					WidthRequest = 100,
					HeightRequest = 100,
					Source = new HtmlWebViewSource
					{
						Html = @"<!DOCTYPE html><html><body>
							<script>function nav() { window.location = 'https://example.com/'; }</script>
							<p id='status'>ready</p>
						</body></html>"
					}
				};

				var handler = CreateHandler(webView);

				await AttachAndRun(webView, async (handler) =>
				{
					// Wait for initial load
					var loaded = new TaskCompletionSource<bool>();
					webView.Navigated += (s, e) =>
					{
						if (e.Result == WebNavigationResult.Success)
							loaded.TrySetResult(true);
					};

					using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
					cts.Token.Register(() => loaded.TrySetResult(false));
					await loaded.Task;

					// Now subscribe to Navigating
					webView.Navigating += (s, e) =>
					{
						navigatingFired.TrySetResult(e);
						e.Cancel = true;
					};

					// Trigger a main frame navigation
					await webView.EvaluateJavaScriptAsync("nav()");

					using var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(5));
					cts2.Token.Register(() => navigatingFired.TrySetCanceled());

					var result = await navigatingFired.Task;

					Assert.NotNull(result);
					Assert.Equal(WebNavigationTarget.MainFrame, result.Target);
					Assert.Contains("example.com", result.Url);
				});
			});
		}

		[Fact]
		public async Task NavigatingEventHasPlatformArgs()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(async () =>
			{
				var navigatingFired = new TaskCompletionSource<WebNavigatingEventArgs>();

				var webView = new WebView
				{
					WidthRequest = 100,
					HeightRequest = 100,
					Source = new HtmlWebViewSource
					{
						Html = @"<!DOCTYPE html><html><body>
							<script>function nav() { window.location = 'https://example.com/'; }</script>
							<p>test</p>
						</body></html>"
					}
				};

				var handler = CreateHandler(webView);

				await AttachAndRun(webView, async (handler) =>
				{
					var loaded = new TaskCompletionSource<bool>();
					webView.Navigated += (s, e) =>
					{
						if (e.Result == WebNavigationResult.Success)
							loaded.TrySetResult(true);
					};

					using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
					cts.Token.Register(() => loaded.TrySetResult(false));
					await loaded.Task;

					webView.Navigating += (s, e) =>
					{
						navigatingFired.TrySetResult(e);
						e.Cancel = true;
					};

					await webView.EvaluateJavaScriptAsync("window.location = 'https://example.com/'");

					using var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(5));
					cts2.Token.Register(() => navigatingFired.TrySetCanceled());

					var result = await navigatingFired.Task;

					Assert.NotNull(result);
					Assert.NotNull(result.PlatformArgs);
				});
			});
		}

#if WINDOWS
		[Fact]
		public async Task NavigatingEventHasFrameTarget()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(async () =>
			{
				var navigatingFired = new TaskCompletionSource<WebNavigatingEventArgs>();

				var webView = new WebView
				{
					WidthRequest = 200,
					HeightRequest = 200,
					Source = new HtmlWebViewSource
					{
						Html = @"<!DOCTYPE html><html><body>
							<iframe id='testframe' src='about:blank'></iframe>
							<script>function navFrame() { document.getElementById('testframe').src = 'https://example.com/'; }</script>
						</body></html>"
					}
				};

				var handler = CreateHandler(webView);

				await AttachAndRun(webView, async (handler) =>
				{
					var loaded = new TaskCompletionSource<bool>();
					webView.Navigated += (s, e) =>
					{
						if (e.Result == WebNavigationResult.Success)
							loaded.TrySetResult(true);
					};

					using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
					cts.Token.Register(() => loaded.TrySetResult(false));
					await loaded.Task;

					webView.Navigating += (s, e) =>
					{
						if (e.Target == WebNavigationTarget.Frame)
						{
							navigatingFired.TrySetResult(e);
							e.Cancel = true;
						}
					};

					await webView.EvaluateJavaScriptAsync("navFrame()");

					using var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(5));
					cts2.Token.Register(() => navigatingFired.TrySetCanceled());

					var result = await navigatingFired.Task;

					Assert.NotNull(result);
					Assert.Equal(WebNavigationTarget.Frame, result.Target);
					Assert.Contains("example.com", result.Url);
				});
			});
		}
#endif

		[Fact]
		public async Task NavigatingEventHasNewWindowTarget()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(async () =>
			{
				var navigatingFired = new TaskCompletionSource<WebNavigatingEventArgs>();

				var webView = new WebView
				{
					WidthRequest = 100,
					HeightRequest = 100,
					Source = new HtmlWebViewSource
					{
						Html = @"<!DOCTYPE html><html><body>
							<script>function openWin() { window.open('https://example.com/', '_blank'); }</script>
							<p>test</p>
						</body></html>"
					}
				};

				var handler = CreateHandler(webView);

				await AttachAndRun(webView, async (handler) =>
				{
					var loaded = new TaskCompletionSource<bool>();
					webView.Navigated += (s, e) =>
					{
						if (e.Result == WebNavigationResult.Success)
							loaded.TrySetResult(true);
					};

					using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
					cts.Token.Register(() => loaded.TrySetResult(false));
					await loaded.Task;

					webView.Navigating += (s, e) =>
					{
						if (e.Target == WebNavigationTarget.NewWindow)
						{
							navigatingFired.TrySetResult(e);
							e.Cancel = true;
						}
					};

					await webView.EvaluateJavaScriptAsync("openWin()");

					using var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(5));
					cts2.Token.Register(() => navigatingFired.TrySetCanceled());

					var result = await navigatingFired.Task;

					Assert.NotNull(result);
					Assert.Equal(WebNavigationTarget.NewWindow, result.Target);
					Assert.Contains("example.com", result.Url);
				});
			});
		}
	}
}
