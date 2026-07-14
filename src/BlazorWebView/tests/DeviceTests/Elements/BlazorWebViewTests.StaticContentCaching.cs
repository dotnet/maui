using System;
using System.Collections.Generic;
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

	// Each test fetches a unique URL (path + query): the WebView HTTP cache is shared for the app origin across
	// BlazorWebView instances (and persists across runs on a device), so a response cached by one test (e.g. with
	// max-age=3600) must not be able to satisfy another test's fetch and skip its provider invocation.

	[Fact]
	public async Task StaticContentCacheControlProviderCanOverrideCacheControlHeader()
	{
		var cacheControl = await GetServedCacheControlHeaderAsync(
			request => request.Uri.AbsolutePath.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal)
				? "max-age=3600"
				: null,
			fetchQueryString: "?test=override");

		Assert.Equal("max-age=3600", cacheControl);
	}

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
	public async Task StaticContentCacheControlProviderReturningValueWithNewlinesKeepsDefaultNoStore()
	{
		// Values containing CR/LF are rejected in favor of the default: some platforms concatenate the value into
		// a raw response header block, where a newline would produce a malformed response or allow header injection.
		var cacheControl = await GetServedCacheControlHeaderAsync(_ => "max-age=3600\r\nX-Injected: 1", fetchQueryString: "?test=newline-provider");

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

	private async Task<string> GetServedCacheControlHeaderAsync(Func<BlazorWebViewStaticContentRequest, string> provider, string fetchPath = CacheControlTestFilePath, string fetchQueryString = "")
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
				const response = await fetch('/{{fetchPath}}{{fetchQueryString}}');
				return response.headers.get('cache-control');
				""");
		});

		return cacheControl;
	}
}
