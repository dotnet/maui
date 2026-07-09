using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.WebView)]
	public partial class WebViewAllowedDomainsTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<WebView, WebViewHandler>();
				});
			});
		}

		[Fact]
		public async Task AllowedDomainsAllowsPermittedNavigation()
		{
			SetupBuilder();

			var navigatedTcs = new TaskCompletionSource<WebNavigationResult>();
			var webView = new WebView
			{
				WidthRequest = 100,
				HeightRequest = 100,
				AllowedDomains = new List<string> { "example.com" },
			};

			webView.Navigated += (s, e) => navigatedTcs.TrySetResult(e.Result);

			await AttachAndRun(webView, async handler =>
			{
				webView.Source = new UrlWebViewSource { Url = "https://example.com" };
				handler.UpdateValue(nameof(IWebView.Source));

				using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
				cts.Token.Register(() => navigatedTcs.TrySetCanceled());

				var result = await navigatedTcs.Task;
				Assert.Equal(WebNavigationResult.Success, result);
			});
		}

		[Fact]
		public async Task AllowedDomainsBlocksDisallowedNavigation()
		{
			SetupBuilder();

			var webView = new WebView
			{
				WidthRequest = 100,
				HeightRequest = 100,
				AllowedDomains = new List<string> { "example.com" },
			};

			await AttachAndRun(webView, async handler =>
			{
				// First load an allowed page
				var loadedTcs = new TaskCompletionSource<bool>();
				webView.Navigated += (s, e) => loadedTcs.TrySetResult(true);
				webView.Source = new UrlWebViewSource { Url = "https://example.com" };
				handler.UpdateValue(nameof(IWebView.Source));

				using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
				cts.Token.Register(() => loadedTcs.TrySetCanceled());
				await loadedTcs.Task;

				// Now try to navigate to a blocked domain
				var blockedNavigatedTcs = new TaskCompletionSource<bool>();
				webView.Navigated += (s, e) =>
				{
					// If we get navigated to evil.com, the test should fail
					if (e.Url?.Contains("evil.com", StringComparison.Ordinal) == true)
						blockedNavigatedTcs.TrySetResult(false);
				};

				webView.Source = new UrlWebViewSource { Url = "https://evil.com" };
				handler.UpdateValue(nameof(IWebView.Source));

				// Wait a bit for the navigation to be attempted and blocked
				await Task.Delay(3000);

				// The blocked navigation should not have completed
				Assert.False(blockedNavigatedTcs.Task.IsCompletedSuccessfully,
					"Navigation to blocked domain should not have succeeded");
			});
		}

		[Fact]
		public async Task NullAllowedDomainsAllowsAllNavigation()
		{
			SetupBuilder();

			var navigatedTcs = new TaskCompletionSource<WebNavigationResult>();
			var webView = new WebView
			{
				WidthRequest = 100,
				HeightRequest = 100,
				// AllowedDomains is null by default
			};

			webView.Navigated += (s, e) => navigatedTcs.TrySetResult(e.Result);

			await AttachAndRun(webView, async handler =>
			{
				webView.Source = new UrlWebViewSource { Url = "https://example.com" };
				handler.UpdateValue(nameof(IWebView.Source));

				using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
				cts.Token.Register(() => navigatedTcs.TrySetCanceled());

				var result = await navigatedTcs.Task;
				Assert.Equal(WebNavigationResult.Success, result);
			});
		}
	}
}
