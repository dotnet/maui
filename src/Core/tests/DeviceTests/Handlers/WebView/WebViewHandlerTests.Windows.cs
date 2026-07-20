using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WebViewHandlerTests
	{
		[Theory(DisplayName = "IsUriWithLocalScheme classifies URLs by parsed host")]
		[InlineData("https://appdir/index.html", true)]        // exact appdir host
		[InlineData("https://appdir/", true)]                   // appdir root
		[InlineData("https://APPDIR/index.html", true)]         // host match is case-insensitive
		[InlineData("http://appdir/index.html", false)]         // wrong scheme
		[InlineData("https://appdir.example.com/page.html", false)] // host merely starts with "appdir"
		[InlineData("https://appdir@host.example.com/page.html", false)] // "appdir" is userinfo, not host
		[InlineData("https://user:pass@appdir/index.html", false)] // host is appdir but carries credentials (no auth on a virtual host)
		[InlineData("https://example.com/index.html", false)]   // unrelated host
		[InlineData("index.html", false)]                        // relative URL is not an absolute appdir URL
		public void IsUriWithLocalSchemeClassifiesByParsedHost(string url, bool expected) =>
			Assert.Equal(expected, MauiWebView.IsUriWithLocalScheme(url));

		[Theory(DisplayName = "IsLocalAppDirUrl treats relative and appdir URLs as local")]
		[InlineData("index.html", true)]                         // relative URLs stay local
		[InlineData("assets/page.html", true)]                   // nested relative URLs stay local
		[InlineData("https://appdir/index.html", true)]          // true appdir URLs are local
		[InlineData("https://appdir.example.com/page.html", false)] // different host
		[InlineData("https://appdir@host.example.com/page.html", false)] // appdir as userinfo
		[InlineData("https://user:pass@appdir/index.html", false)] // appdir host but with credentials
		[InlineData("https://example.com/index.html", false)]    // external host
		public void IsLocalAppDirUrlClassifiesCorrectly(string url, bool expected) =>
			Assert.Equal(expected, MauiWebView.IsLocalAppDirUrl(url));

		[Fact(DisplayName = "Local sub-resource from appdir host loads")]
		public async Task LocalSubResourceFromAppDirLoads()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var webView = new WebViewStub();
				var handler = CreateHandler(webView);
				var platformView = (MauiWebView)handler.PlatformView;

				await AttachAndRun(webView, async (_) =>
				{
					await platformView.EnsureCoreWebView2Async();

					var navigated = new TaskCompletionSource();
					platformView.CoreWebView2.NavigationCompleted += (_, _) => navigated.TrySetResult();

					// The page references appdir-subresource-test.js relative to the appdir
					// host; the script sets document.title when it successfully loads.
					((IWebViewDelegate)platformView).LoadUrl("https://appdir/appdir-subresource-test.html");

					await navigated.Task.WaitAsync(TimeSpan.FromSeconds(10));

					var title = await platformView.CoreWebView2.ExecuteScriptAsync("document.title");

					// ExecuteScriptAsync returns a JSON-encoded string (quotes included).
					Assert.Equal("\"subresource-loaded\"", title);
				});
			});
		}

		[Fact(DisplayName = "Non-appdir navigation does not receive local mapping")]
		public async Task NonAppDirNavigationDoesNotReceiveLocalMapping()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var webView = new WebViewStub();
				var handler = CreateHandler(webView);
				var platformView = (MauiWebView)handler.PlatformView;

				await AttachAndRun(webView, async (_) =>
				{
					await platformView.EnsureCoreWebView2Async();

					// 1. Load a local appdir page so the mapping is applied and the
					//    sub-resource loads.
					var appDirNavigated = new TaskCompletionSource();
					platformView.CoreWebView2.NavigationCompleted += (_, _) => appDirNavigated.TrySetResult();

					((IWebViewDelegate)platformView).LoadUrl("https://appdir/appdir-subresource-test.html");
					await appDirNavigated.Task.WaitAsync(TimeSpan.FromSeconds(10));

					Assert.Equal(
						"\"subresource-loaded\"",
						await platformView.CoreWebView2.ExecuteScriptAsync("document.title"));

					// 2. Navigate to a non-appdir page that references the appdir
					//    sub-resource. Because the parsed host is not "appdir", the
					//    NavigationStarting handler clears the mapping, so the script
					//    must NOT load and must not override the title. A cache-busting
					//    query ensures the request is not served from the cache populated
					//    in step 1 - only a live mapping could serve it.
					var otherNavigated = new TaskCompletionSource();
					platformView.CoreWebView2.NavigationCompleted += (_, _) => otherNavigated.TrySetResult();

					platformView.NavigateToString(
						"<html><head><title>non-appdir</title>" +
						"<script src=\"https://appdir/appdir-subresource-test.js?nocache=1\"></script>" +
						"</head><body><p>non-appdir</p></body></html>");
					await otherNavigated.Task.WaitAsync(TimeSpan.FromSeconds(10));

					// The mapping was cleared, so the appdir sub-resource failed to load
					// and the title kept its original value.
					Assert.Equal(
						"\"non-appdir\"",
						await platformView.CoreWebView2.ExecuteScriptAsync("document.title"));
				});
			});
		}

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

				var platformView = handler.PlatformView;

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

		WebView2 GetNativeWebView(WebViewHandler webViewHandler) =>
			webViewHandler.PlatformView;

		string GetNativeSource(WebViewHandler webViewHandler)
		{
			var plaformWebView = GetNativeWebView(webViewHandler);
			return plaformWebView.Source.AbsoluteUri;
		}
	}
}
