using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
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

				async Task KillAndAssertRestored()
				{
					terminated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
					navigated = new TaskCompletionSource<WebNavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
					KillWebContentProcess(handler.PlatformView);

					await terminated.Task.WaitAsync(TimeSpan.FromSeconds(5));
					Assert.Equal(WebNavigationResult.Success, await navigated.Task.WaitAsync(TimeSpan.FromSeconds(5)));
					Assert.Equal("Issue39973", await webView.EvaluateJavaScriptAsync("document.getElementById('content').textContent"));
				}

				await KillAndAssertRestored();

				// Once the 30 second cooldown has passed, the next termination is recovered again
				var navigationDelegate = (MauiWebViewNavigationDelegate)handler.PlatformView.NavigationDelegate;
				navigationDelegate.LastProcessTerminationReload -= 30_000;
				await KillAndAssertRestored();

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

		[Fact(DisplayName = "Evaluating JavaScript returns null when navigating to another site destroys the page running it")]
		public async Task EvaluateJavaScriptReturnsNullWhenNavigationDestroysPage()
		{
			SetupBuilder();

			var webView = new WebView
			{
				WidthRequest = 100,
				HeightRequest = 100,
				Source = new HtmlWebViewSource { Html = "<html><body>First</body></html>", BaseUrl = "https://first.example/" }
			};

			var navigated = new TaskCompletionSource<WebNavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
			webView.Navigated += (_, e) => navigated.TrySetResult(e.Result);

			await AttachAndRun<WebViewHandler>(webView, async handler =>
			{
				Assert.Equal(WebNavigationResult.Success, await navigated.Task.WaitAsync(TimeSpan.FromSeconds(5)));

				// The other site loads in a new web content process. The first page is still busy running the
				// script, so it cannot suspend, and WebKit destroys it after about 10 seconds, before the script
				// can return its result.
				webView.Source = new HtmlWebViewSource { Html = "<html><body>Second</body></html>", BaseUrl = "https://second.example/" };
				var result = await webView.EvaluateJavaScriptAsync("(function(){var start=Date.now();while(Date.now()-start<30000){}return 1;})()")
					.WaitAsync(TimeSpan.FromSeconds(60));

				Assert.Null(result);
			});
		}

		[Theory(DisplayName = "Evaluating JavaScript returns null only for a lost WebKit result")]
		[InlineData("WKErrorDomain", 5, true)]
		[InlineData("WKErrorDomain", 4, false)]
		[InlineData("WKErrorDomain", 1, false)]
		[InlineData("NSCocoaErrorDomain", 5, false)]
		public async Task EvaluateJavaScriptReturnsNullOnlyForLostResult(string domain, int code, bool resultLost)
		{
			using var error = new NSError(new NSString(domain), code);

			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<WebView>(_ => new FailingEvaluationWebViewHandler(error));
				});
			});

			var webView = new WebView
			{
				WidthRequest = 100,
				HeightRequest = 100,
			};

			await AttachAndRun(webView, async handler =>
			{
				Assert.IsType<FailingEvaluationWebViewHandler>(handler);

				var evaluation = webView.EvaluateJavaScriptAsync("1+1").WaitAsync(TimeSpan.FromSeconds(5));

				if (resultLost)
				{
					Assert.Null(await evaluation);
				}
				else
				{
					var exception = await Assert.ThrowsAsync<NSErrorException>(() => evaluation);
					Assert.Equal(domain, exception.Error.Domain);
					Assert.Equal((nint)code, exception.Error.Code);
				}
			});
		}

		sealed class FailingEvaluationWebViewHandler : WebViewHandler
		{
			readonly NSError _error;

			public FailingEvaluationWebViewHandler(NSError error)
			{
				_error = error;
			}

			protected override WebKit.WKWebView CreatePlatformView() => new FailingEvaluationWKWebView(this, _error);
		}

		sealed class FailingEvaluationWKWebView : MauiWKWebView
		{
			readonly NSError _error;

			public FailingEvaluationWKWebView(WebViewHandler handler, NSError error) : base(handler)
			{
				_error = error;
			}

			public override void EvaluateJavaScript(NSString javascript, WebKit.WKJavascriptEvaluationResult completionHandler)
			{
				completionHandler?.Invoke(null, _error);
			}
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
