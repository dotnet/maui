using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Webkit;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WebViewHandlerTests
	{
		[Fact]
		public Task EnsureSupportForCustomWebViewClients() =>
			InvokeOnMainThreadAsync(async () =>
			{
				// create the cross-platform view
				var webView = new WebViewStub
				{
					Width = 300,
					Height = 200,
					Background = new SolidPaint(Colors.Red),
				};

				// create the platform view
				await AttachAndRun<CustomWebViewHandler>(webView, async webViewHandler =>
				{
					var platformWebView = webViewHandler.PlatformView;

					var tcsLoaded = new TaskCompletionSource<bool>();
					var tcsNavigating = new TaskCompletionSource();
					var tcsRequested = new TaskCompletionSource();

					// if the timeout happens, cancel everything
					var pageLoadTimeout = TimeSpan.FromSeconds(30);
					var ctsTimeout = new CancellationTokenSource(pageLoadTimeout);
					ctsTimeout.Token.Register(() =>
					{
						tcsLoaded.TrySetException(new TimeoutException($"Failed to load HTML"));
						tcsNavigating.TrySetException(new TimeoutException($"Failed to navigate to the loaded page"));
						tcsRequested.TrySetException(new TimeoutException($"Failed to request the image"));
					});

					// attach some event handlers to track things
					var navigatingCount = 0;
					webView.NavigatingDelegate = new((evnt, url) =>
					{
						navigatingCount++;

						if (url == "file:///android_asset/extracontent.html")
							tcsNavigating.TrySetResult();

						return false; // do not cancel the navigation
					});
					var shouldRequestCount = 0;
					webViewHandler.ShouldInterceptRequestDelegate = new((view, request) =>
					{
						shouldRequestCount++;

						if (request.Url.ToString().StartsWith("https://raw.githubusercontent.com/dotnet/maui/4c096c1f17e9a23bf3961ba5778d3936039ad881/Assets/icon.png"))
							tcsRequested.TrySetResult();
					});

					// set up a task to wait for the page to load
					webView.NavigatedDelegate = (evnt, url, result) =>
					{
						// Set success when we have a successful nav result
						if (result == WebNavigationResult.Success && url == "file:///android_asset/extracontent.html")
							tcsLoaded.TrySetResult(result == WebNavigationResult.Success);
					};

					// load the page
					webView.Source = new UrlWebViewSourceStub { Url = "extracontent.html" };
					webViewHandler.UpdateValue(nameof(IWebView.Source));

					// wait for the loaded event
					Assert.True(await tcsLoaded.Task, "HTML Source Failed to Load");

					// make sure the mapper override fired at least once
					Assert.IsType<CustomWebClient>(webViewHandler.CustomWebClient);

					// wait for the navigation to complete
					await tcsNavigating.Task;
					Assert.True(navigatingCount > 1); // at least 1 navigation, Android seems to do a few

					// wait for the image to be requested
					await tcsRequested.Task;
					Assert.Equal(1, shouldRequestCount); // only 1 request for the image to load
				});
			});

		AWebView GetNativeWebView(WebViewHandler webViewHandler) =>
			webViewHandler.PlatformView;

		string GetNativeSource(WebViewHandler webViewHandler) =>
			GetNativeWebView(webViewHandler).Url;

		class CustomWebViewHandler : WebViewHandler
		{
			// make a copy of the Core mappers because we don't want any Controls changes or to override us
			static IPropertyMapper<IWebView, IWebViewHandler> TestMapper =
				new PropertyMapper<IWebView, IWebViewHandler>(WebViewHandler.Mapper);
			static CommandMapper<IWebView, IWebViewHandler> TestCommandMapper =
				new(WebViewHandler.CommandMapper);

			static CustomWebViewHandler()
			{
				// this is part of the test: testing the modify/replace the existing mapper
				TestMapper.ModifyMapping(
					nameof(WebViewClient),
					(handler, view, setter) =>
					{
						if (handler is not CustomWebViewHandler custom)
							throw new Exception("The CustomWebViewHandler.TestMapper is only meant to be used with the CustomWebViewHandler tests.");

						if (custom.CustomWebClient is not null)
							throw new Exception("The [WebViewClient] mapper method is only supposed to be called once.");

						custom.CustomWebClient = new CustomWebClient((CustomWebViewHandler)handler);

						handler.PlatformView.SetWebViewClient(custom.CustomWebClient);
					});
			}

			// make sure to use the Core mappers
			public CustomWebViewHandler()
				: base(TestMapper, TestCommandMapper)
			{
			}

			public CustomWebClient CustomWebClient { get; private set; }

			public Action<AWebView, IWebResourceRequest> ShouldInterceptRequestDelegate { get; set; }
		}

		class CustomWebClient : MauiWebViewClient
		{
			CustomWebViewHandler _handler;

			public CustomWebClient(CustomWebViewHandler handler)
				: base(handler)
			{
				_handler = handler;
			}

			public override WebResourceResponse ShouldInterceptRequest(AWebView view, IWebResourceRequest request)
			{
				_handler.ShouldInterceptRequestDelegate(view, request);

				return base.ShouldInterceptRequest(view, request);
			}
		}
	}
}