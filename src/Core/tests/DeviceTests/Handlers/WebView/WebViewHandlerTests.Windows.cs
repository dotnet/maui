using System;
using System.Threading;
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
		[Theory(DisplayName = "UrlSource Updates Correctly")]
		[InlineData("<h1>Old Source</h1><br>", "<p>New Source</p>\"")]
		[InlineData("<p>Old Source</p><br>", "<h1>New Source</h1>\"")]
		public async Task HtmlSourceUpdatesCorrectly(string oldSource, string newSource)
		{
			var pageLoadTimeout = TimeSpan.FromSeconds(2);

			await InvokeOnMainThreadAsync(async () =>
			{
				var webView = new WebViewStub()
				{
					Width = 100,
					Height = 100,
					Source = new HtmlWebViewSourceStub { Html = oldSource }
				};

				var handler = CreateHandler(webView);

				// Setup the view to be displayed/parented and run our tests on it
				await AttachAndRun(webView, async (handler) =>
				{
					// Wait for the page to load
					var tcsLoaded = new TaskCompletionSource<bool>();
					var ctsTimeout = new CancellationTokenSource(pageLoadTimeout);
					ctsTimeout.Token.Register(() => tcsLoaded.TrySetException(new TimeoutException($"Failed to load HTML")));

					webView.NavigatedDelegate = (evnt, url, result) =>
					{
						// Set success when we have a successful nav result
						if (result == WebNavigationResult.Success)
							tcsLoaded.TrySetResult(result == WebNavigationResult.Success);
					};

					// Load the new Source
					webView.Source = new HtmlWebViewSourceStub { Html = newSource };

					handler.UpdateValue(nameof(IWebView.Source));

					// If the new source is loaded without exceptions, the test has passed
					Assert.True(await tcsLoaded.Task);
				});
			});
		}

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
				};

				await AttachAndRun(webView, async (handler) =>
				{
					await handler.PlatformView.OnLoadedAsync();
					await navigationComplete.Task;
					navigationComplete = null;
				});
			});
		}

		[Fact(DisplayName = "CanGoBack Is False After Navigating Back To The First Page (Issue #37534)")]
		public async Task CanGoBackIsFalseAfterReturningToFirstPage()
		{
			var pageLoadTimeout = TimeSpan.FromSeconds(5);

			await InvokeOnMainThreadAsync(async () =>
			{
				var webView = new WebViewStub()
				{
					Width = 100,
					Height = 100,
					Source = new HtmlWebViewSourceStub { Html = "<h1>Page 1</h1>" }
				};

				var handler = CreateHandler(webView);

				var platformView = handler.PlatformView;

				// Setup the view to be displayed/parented and run our tests on it
				await AttachAndRun(webView, async (handler) =>
				{
					async Task WaitForNavigationAsync(Action navigate)
					{
						var tcsNavigated = new TaskCompletionSource<bool>();
						using var ctsTimeout = new CancellationTokenSource(pageLoadTimeout);
						ctsTimeout.Token.Register(() => tcsNavigated.TrySetException(new TimeoutException("Failed to navigate")));

						webView.NavigatedDelegate = (evnt, url, result) =>
						{
							if (result == WebNavigationResult.Success)
								tcsNavigated.TrySetResult(true);
						};

						navigate();

						Assert.True(await tcsNavigated.Task);
					}

					// Wait for the first page to finish loading
					await WaitForNavigationAsync(() => { });

					// Navigate to a second page, creating back history
					await WaitForNavigationAsync(() =>
					{
						webView.Source = new HtmlWebViewSourceStub { Html = "<h1>Page 2</h1>" };
						handler.UpdateValue(nameof(IWebView.Source));
					});

					Assert.True(webView.CanGoBack, "CanGoBack should be true after navigating to a second page.");

					// Navigate back to the first page
					await WaitForNavigationAsync(() =>
					{
						handler.Invoke(nameof(IWebView.GoBack), null);
					});

					// Regression test for https://github.com/dotnet/maui/issues/37534:
					// CanGoBack should become false once there is no more back history left,
					// instead of remaining stale as true (which caused GoBack() to repeatedly
					// reload the same page instead of falling back to app-level navigation).
					Assert.False(webView.CanGoBack, "CanGoBack should be false once there is no more back history.");
				});
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
