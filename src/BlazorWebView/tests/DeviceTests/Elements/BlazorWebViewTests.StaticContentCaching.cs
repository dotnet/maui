using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
	const string CacheControlTestFilePath = "cache-control-test.txt";
	const string CacheControlTestFileContents = "static asset used by the cache-control tests";
	const string CacheControlTestImagePath = "cache-control-test.svg";
	const string CacheControlTestImageContents = """
		<svg xmlns="http://www.w3.org/2000/svg" width="800" height="500" viewBox="0 0 800 500">
			<defs>
				<linearGradient id="background" x1="0" y1="0" x2="1" y2="1">
					<stop offset="0" stop-color="#512bd4"/>
					<stop offset="1" stop-color="#00a4ef"/>
				</linearGradient>
			</defs>
			<rect width="800" height="500" rx="48" fill="url(#background)"/>
			<circle cx="180" cy="250" r="105" fill="#ffffff" fill-opacity=".94"/>
			<path d="M145 190h70v120h-70zM110 225h140v50H110z" fill="#512bd4"/>
			<text x="330" y="235" font-family="sans-serif" font-size="54" font-weight="700" fill="#ffffff">.NET MAUI</text>
			<text x="330" y="300" font-family="sans-serif" font-size="30" fill="#ffffff">cached static image</text>
		</svg>
		""";

	// Each test fetches a unique URL (path + query): the WebView HTTP cache is shared for the app origin across
	// BlazorWebView instances, so a response cached by one test must not be able to satisfy another test's fetch and
	// skip its provider invocation. The cacheable override test additionally uses a per-run nonce because that cache
	// also persists across runs on a device, so a fixed URL could be served from a prior run's max-age=3600 response.

	[Fact]
	public async Task StaticContentCacheControlProviderCanOverrideCacheControlHeader()
	{
		var nonce = Guid.NewGuid().ToString("N");
		var providerInvokedForTarget = false;

		var cacheControl = await GetServedCacheControlHeaderAsync(
			request =>
			{
				if (request.Uri.AbsolutePath.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					providerInvokedForTarget = true;
					return "max-age=3600";
				}
				return null;
			},
			fetchQueryString: $"?test=override&nonce={nonce}");

		// The nonce makes this a guaranteed cache miss, so a passing header assertion cannot come from a stale cached
		// response: the provider must have run for the requested resource.
		Assert.True(providerInvokedForTarget, "The provider was not invoked for the requested resource - the response was likely served from the WebView cache.");
		Assert.Equal("max-age=3600", cacheControl);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderAllowsRepeatedRequestToUseWebViewCache()
	{
		var nonce = Guid.NewGuid().ToString("N");
		var providerInvocationCount = 0;
		var fileReadCount = 0;

		var cacheControl = await GetServedCacheControlHeaderAsync(
			request =>
			{
				if (request.Uri.AbsolutePath.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					Interlocked.Increment(ref providerInvocationCount);
					return "public, max-age=3600";
				}
				return null;
			},
			fetchQueryString: $"?test=repeated-request&nonce={nonce}",
			fetchCount: 2,
			fileOpened: path =>
			{
				if (path.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					Interlocked.Increment(ref fileReadCount);
				}
			});

		Assert.Equal("public, max-age=3600", cacheControl);
		Assert.Equal(1, providerInvocationCount);
		Assert.Equal(1, fileReadCount);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderDoesNotCacheNoStoreResponse()
	{
		var providerInvocationCount = 0;
		var fileReadCount = 0;

		var cacheControl = await GetServedCacheControlHeaderAsync(
			request =>
			{
				if (request.Uri.AbsolutePath.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					Interlocked.Increment(ref providerInvocationCount);
					return "no-store";
				}
				return null;
			},
			fetchQueryString: "?test=repeated-no-store",
			fetchCount: 2,
			fileOpened: path =>
			{
				if (path.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					Interlocked.Increment(ref fileReadCount);
				}
			});

		Assert.Equal("no-store", cacheControl);
		Assert.Equal(2, providerInvocationCount);
		Assert.Equal(2, fileReadCount);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderExpiresResponse()
	{
		var providerInvocationCount = 0;
		var fileReadCount = 0;

		await GetServedCacheControlHeaderAsync(
			request =>
			{
				if (request.Uri.AbsolutePath.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					Interlocked.Increment(ref providerInvocationCount);
					return "public, max-age=1";
				}
				return null;
			},
			fetchQueryString: "?test=expired",
			fetchCount: 2,
			delayBetweenFetchesMilliseconds: 1200,
			fileOpened: path =>
			{
				if (path.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					Interlocked.Increment(ref fileReadCount);
				}
			});

		Assert.Equal(2, providerInvocationCount);
		Assert.Equal(2, fileReadCount);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderDoesNotCacheNoCacheResponse()
	{
		var providerInvocationCount = 0;
		var fileReadCount = 0;

		await GetServedCacheControlHeaderAsync(
			request =>
			{
				if (request.Uri.AbsolutePath.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					Interlocked.Increment(ref providerInvocationCount);
					return "no-cache, max-age=3600";
				}
				return null;
			},
			fetchQueryString: "?test=repeated-no-cache",
			fetchCount: 2,
			fileOpened: path =>
			{
				if (path.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					Interlocked.Increment(ref fileReadCount);
				}
			});

		Assert.Equal(2, providerInvocationCount);
		Assert.Equal(2, fileReadCount);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderHonorsRequestNoCache()
	{
		var nonce = Guid.NewGuid().ToString("N");
		var providerInvocationCount = 0;
		var fileReadCount = 0;

		await GetServedCacheControlHeaderAsync(
			request =>
			{
				if (request.Uri.AbsolutePath.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					Interlocked.Increment(ref providerInvocationCount);
					return "public, max-age=3600";
				}
				return null;
			},
			fetchQueryString: $"?test=request-no-cache&nonce={nonce}",
			fetchCount: 3,
			noCacheRequestIndex: 1,
			fileOpened: path =>
			{
				if (path.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					Interlocked.Increment(ref fileReadCount);
				}
			});

		Assert.Equal(2, providerInvocationCount);
		Assert.Equal(2, fileReadCount);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderRefreshRemovesStaleResponse()
	{
		var nonce = Guid.NewGuid().ToString("N");
		var providerInvocationCount = 0;
		var fileReadCount = 0;

		await GetServedCacheControlHeaderAsync(
			request =>
			{
				if (request.Uri.AbsolutePath.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					return Interlocked.Increment(ref providerInvocationCount) == 1
						? "public, max-age=3600"
						: "no-store";
				}
				return null;
			},
			fetchQueryString: $"?test=refresh-no-store&nonce={nonce}",
			fetchCount: 3,
			noCacheRequestIndex: 1,
			fileOpened: path =>
			{
				if (path.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					Interlocked.Increment(ref fileReadCount);
				}
			});

		Assert.Equal(3, providerInvocationCount);
		Assert.Equal(3, fileReadCount);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderAuthorizationDisablesRefreshRegardlessOfHeaderOrder()
	{
		var nonce = Guid.NewGuid().ToString("N");
		var providerInvocationCount = 0;
		var fileReadCount = 0;

		EnsureHandlerCreated(builder =>
		{
			builder.Services.AddMauiBlazorWebView();
		});

		var blazorWebView = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
				{ CacheControlTestFilePath, CacheControlTestFileContents },
			},
			StaticContentCacheControlProvider = request =>
			{
				if (request.Uri.AbsolutePath.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					return Interlocked.Increment(ref providerInvocationCount) == 2
						? "no-store"
						: "public, max-age=3600";
				}
				return null;
			},
			FileContentsOverride = path =>
			{
				if (path.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					return $"content-{Interlocked.Increment(ref fileReadCount)}";
				}
				return null;
			},
		};

		blazorWebView.RootComponents.Add(new RootComponent
		{
			ComponentType = typeof(NoOpComponent),
			Selector = "#app"
		});

		string results = null;

		await AttachAndRun(blazorWebView, async handler =>
		{
			var platformWebView = ((BlazorWebViewHandler)handler).PlatformView;

			await WebViewHelpers.WaitForWebViewReady(platformWebView);
			await WebViewHelpers.WaitForControlDiv(platformWebView, controlValueToWaitFor: "Static");

			results = await WebViewHelpers.ExecuteAsyncScriptAndWaitForResult<string>(platformWebView,
				$$"""
					const requestUrl = '/{{CacheControlTestFilePath}}?test=request-header-order&nonce={{nonce}}';
					const first = await (await fetch(requestUrl)).text();
					const authorizedRefresh = await (await fetch(requestUrl, {
						headers: {
							'Cache-Control': 'no-cache',
							'Authorization': 'Bearer cache-test'
						}
					})).text();
					const cached = await (await fetch(requestUrl)).text();
					return [first, authorizedRefresh, cached].join('|');
				""");
		});

		Assert.Equal("content-1|content-2|content-1", results);
		Assert.Equal(2, providerInvocationCount);
		Assert.Equal(2, fileReadCount);
	}

#if ANDROID
	[Fact]
	public async Task StaticContentCacheControlProviderReusesAndroidImageAfterDecodedCachePressure()
	{
		var nonce = Guid.NewGuid().ToString("N");
		var providerInvocationCount = 0;
		var fileReadCount = 0;

		EnsureHandlerCreated(builder =>
		{
			builder.Services.AddMauiBlazorWebView();
		});

		var blazorWebView = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			WidthRequest = 320,
			HeightRequest = 440,
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
				{ CacheControlTestImagePath, CacheControlTestImageContents },
			},
			FileOpened = path =>
			{
				if (path.EndsWith(CacheControlTestImagePath, StringComparison.Ordinal))
				{
					Interlocked.Increment(ref fileReadCount);
				}
			},
			StaticContentCacheControlProvider = request =>
			{
				if (request.Uri.AbsolutePath.EndsWith(CacheControlTestImagePath, StringComparison.Ordinal))
				{
					Interlocked.Increment(ref providerInvocationCount);
					Thread.Sleep(500);
					return "public, max-age=3600";
				}
				return null;
			},
		};

		blazorWebView.RootComponents.Add(new RootComponent
		{
			ComponentType = typeof(NoOpComponent),
			Selector = "#app"
		});

		ImageLoadTimings timings = null;

		await AttachAndRun(blazorWebView, async handler =>
		{
			var platformWebView = ((BlazorWebViewHandler)handler).PlatformView;

			await WebViewHelpers.WaitForWebViewReady(platformWebView);
			await WebViewHelpers.WaitForControlDiv(platformWebView, controlValueToWaitFor: "Static");

			timings = await WebViewHelpers.ExecuteAsyncScriptAndWaitForResult<ImageLoadTimings>(platformWebView,
				$$"""
					const imageUrl = '/{{CacheControlTestImagePath}}?test=image-reinsert&nonce={{nonce}}';
					const container = document.createElement('div');
					container.style.width = '320px';
					container.style.height = '200px';
					document.body.appendChild(container);

					async function loadImage() {
						const image = new Image();
						image.style.width = '320px';
						image.style.height = '200px';
						const loaded = new Promise((resolve, reject) => {
							image.addEventListener('load', resolve, { once: true });
							image.addEventListener('error', () => reject(new Error('Image failed to load')), { once: true });
						});
						const started = performance.now();
						image.src = imageUrl;
						container.replaceChildren(image);
						await loaded;
						if (image.decode) {
							await image.decode();
						}
						return performance.now() - started;
					}

					async function loadChurnImage(url, host) {
						const image = new Image();
						image.style.width = '800px';
						image.style.height = '500px';
						const loaded = new Promise((resolve, reject) => {
							image.addEventListener('load', resolve, { once: true });
							image.addEventListener('error', () => reject(new Error('Churn image failed to load')), { once: true });
						});
						image.src = url;
						host.appendChild(image);
						await loaded;
						if (image.decode) {
							await image.decode();
						}
					}

					function createChurnImage(index) {
						const canvas = document.createElement('canvas');
						canvas.width = 800;
						canvas.height = 500;
						const context = canvas.getContext('2d');
						const gradient = context.createLinearGradient(0, 0, 800, 500);
						gradient.addColorStop(0, 'hsl(' + index * 47 % 360 + ',80%,45%)');
						gradient.addColorStop(1, 'hsl(' + index * 83 % 360 + ',80%,65%)');
						context.fillStyle = gradient;
						context.fillRect(0, 0, 800, 500);
						context.fillStyle = 'white';
						context.font = 'bold 120px sans-serif';
						context.fillText(String(index), 280, 300);
						return canvas.toDataURL('image/png');
					}

					const firstLoadMilliseconds = await loadImage();
					container.replaceChildren();
					const churnHost = document.createElement('div');
					churnHost.style.cssText = 'position:fixed;left:-10000px;top:0;width:800px;height:500px';
					document.body.appendChild(churnHost);
					for (let churnIndex = 0; churnIndex < 64; churnIndex++) {
						await loadChurnImage(createChurnImage(churnIndex), churnHost);
					}
					await new Promise(resolve => setTimeout(resolve, 300));
					churnHost.remove();
					await new Promise(resolve => setTimeout(resolve, 300));
					const secondLoadMilliseconds = await loadImage();
					return { firstLoadMilliseconds, secondLoadMilliseconds };
				""");
		});

		Assert.NotNull(timings);
		Output.WriteLine($"Image load timings: first={timings.firstLoadMilliseconds:F1}ms, cached={timings.secondLoadMilliseconds:F1}ms");
		Assert.Equal(1, providerInvocationCount);
		Assert.Equal(1, fileReadCount);
		Assert.True(
			timings.firstLoadMilliseconds - timings.secondLoadMilliseconds >= 250,
			$"Expected the cached image load to avoid the simulated 500ms source delay. First: {timings.firstLoadMilliseconds:F1}ms; second: {timings.secondLoadMilliseconds:F1}ms.");
	}

	sealed class ImageLoadTimings
	{
		public double firstLoadMilliseconds { get; set; }
		public double secondLoadMilliseconds { get; set; }
	}

#endif

	[Fact]
	public async Task StaticContentCacheControlProviderReturningNullKeepsDefaultNoStore()
	{
		// Returning null from the provider must preserve the historical default so that the change is non-breaking.
		var cacheControl = await GetServedCacheControlHeaderAsync(_ => null, fetchQueryString: "?test=null-provider");

		Assert.Contains("no-store", cacheControl, StringComparison.Ordinal);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderReturningEmptyStringKeepsDefaultNoStore()
	{
		// An empty string is treated the same as null: an empty Cache-Control header value is non-standard and
		// more likely accidental than an intentional opt-in, so the safe default is preserved.
		var cacheControl = await GetServedCacheControlHeaderAsync(_ => string.Empty, fetchQueryString: "?test=empty-provider");

		Assert.Contains("no-store", cacheControl, StringComparison.Ordinal);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderReturningWhitespaceKeepsDefaultNoStore()
	{
		// A whitespace-only value is treated the same as null/empty: it is a non-standard, meaningless Cache-Control
		// value that is far more likely an accidental result of string manipulation than an intentional opt-in.
		var cacheControl = await GetServedCacheControlHeaderAsync(_ => "   ", fetchQueryString: "?test=whitespace-provider");

		Assert.Contains("no-store", cacheControl, StringComparison.Ordinal);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderReturningValueWithNewlinesKeepsDefaultNoStore()
	{
		// Values containing CR/LF are rejected in favor of the default: some platforms concatenate the value into
		// a raw response header block, where a newline would produce a malformed response or allow header injection.
		var cacheControl = await GetServedCacheControlHeaderAsync(_ => "max-age=3600\r\nX-Injected: 1", fetchQueryString: "?test=newline-provider");

		Assert.Contains("no-store", cacheControl, StringComparison.Ordinal);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderThrowingKeepsDefaultNoStore()
	{
		// A provider that throws must not crash or hang static asset serving: the exception is caught and logged,
		// and the request falls back to the historical default header.
		var cacheControl = await GetServedCacheControlHeaderAsync(
			_ => throw new InvalidOperationException("provider failure"),
			fetchQueryString: "?test=throwing-provider");

		Assert.Contains("no-store", cacheControl, StringComparison.Ordinal);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderReceivesResolvedContentType()
	{
		string observedContentType = null;

		await GetServedCacheControlHeaderAsync(
			request =>
			{
				if (request.Uri.AbsolutePath.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					observedContentType = request.ContentType;
				}
				return null;
			},
			fetchQueryString: "?test=content-type");

		Assert.Equal("text/plain", observedContentType);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderReceivesQueryString()
	{
		// The provider must receive the original request URI including the query string so that apps can make
		// cache-busting decisions based on versioned URLs (e.g. img.png?v=2). The query is only stripped when
		// resolving the file on disk. See https://github.com/dotnet/maui/issues/8279
		Uri observedUri = null;

		await GetServedCacheControlHeaderAsync(
			request =>
			{
				if (request.Uri.AbsolutePath.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
				{
					observedUri = request.Uri;
				}
				return null;
			},
			fetchQueryString: "?v=2");

		Assert.NotNull(observedUri);
		Assert.Contains("v=2", observedUri.Query, StringComparison.Ordinal);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderReceivesQueryStringForFolderServedContent()
	{
		// On WinUI, _framework/blazor.modules.json is served through the folder-serving path
		// (WinUIWebViewManager.TryServeFromFolderAsync) rather than the in-memory file provider that backs the
		// other static assets in these tests. That path must also pass the original request URI (including the
		// query string) to the provider, otherwise apps cannot make cache-busting decisions for folder-served
		// content. See https://github.com/dotnet/maui/issues/8279
		Uri observedUri = null;

		await GetServedCacheControlHeaderAsync(
			request =>
			{
				if (request.Uri.AbsolutePath.EndsWith("blazor.modules.json", StringComparison.Ordinal))
				{
					observedUri = request.Uri;
				}
				return null;
			},
			fetchPath: "_framework/blazor.modules.json",
			fetchQueryString: "?v=2");

		Assert.NotNull(observedUri);
		Assert.Contains("v=2", observedUri.Query, StringComparison.Ordinal);
	}

	private async Task<string> GetServedCacheControlHeaderAsync(
		Func<BlazorWebViewStaticContentRequest, string> provider,
		string fetchPath = CacheControlTestFilePath,
		string fetchQueryString = "",
		int fetchCount = 1,
		int delayBetweenFetchesMilliseconds = 0,
		int noCacheRequestIndex = -1,
		Action<string> fileOpened = null)
	{
		EnsureHandlerCreated(builder =>
		{
			builder.Services.AddMauiBlazorWebView();
		});

		var blazorWebView = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
				{ CacheControlTestFilePath, CacheControlTestFileContents },
			},
			StaticContentCacheControlProvider = provider,
			FileOpened = fileOpened,
		};

		blazorWebView.RootComponents.Add(new RootComponent
		{
			ComponentType = typeof(NoOpComponent),
			Selector = "#app"
		});

		string cacheControl = null;

		await AttachAndRun(blazorWebView, async handler =>
		{
			var blazorWebViewHandler = handler as BlazorWebViewHandler;
			var platformWebView = blazorWebViewHandler.PlatformView;

			await WebViewHelpers.WaitForWebViewReady(platformWebView);
			await WebViewHelpers.WaitForControlDiv(platformWebView, controlValueToWaitFor: "Static");

			cacheControl = await WebViewHelpers.ExecuteAsyncScriptAndWaitForResult<string>(platformWebView,
				$$"""
				let cacheControl = null;
				for (let requestIndex = 0; requestIndex < {{fetchCount}}; requestIndex++) {
					const requestOptions = requestIndex === {{noCacheRequestIndex}}
						? { headers: { 'Cache-Control': 'no-cache' } }
						: undefined;
					const response = await fetch('/{{fetchPath}}{{fetchQueryString}}', requestOptions);
					cacheControl = response.headers.get('cache-control');
					await response.text();
					if (requestIndex + 1 < {{fetchCount}} && {{delayBetweenFetchesMilliseconds}} > 0) {
						await new Promise(resolve => setTimeout(resolve, {{delayBetweenFetchesMilliseconds}}));
					}
				}
				return cacheControl;
				""");
		});

		return cacheControl;
	}
}
