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

	[Fact]
	public async Task StaticContentCacheControlProviderCanOverrideCacheControlHeader()
	{
		var cacheControl = await GetServedCacheControlHeaderAsync(request =>
			request.Uri.AbsolutePath.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal)
				? "max-age=3600"
				: null);

		Assert.Equal("max-age=3600", cacheControl);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderReturningNullKeepsDefaultNoStore()
	{
		// Returning null from the provider must preserve the historical default so that the change is non-breaking.
		var cacheControl = await GetServedCacheControlHeaderAsync(_ => null);

		Assert.Contains("no-store", cacheControl, StringComparison.Ordinal);
	}

	[Fact]
	public async Task StaticContentCacheControlProviderReceivesResolvedContentType()
	{
		string observedContentType = null;

		await GetServedCacheControlHeaderAsync(request =>
		{
			if (request.Uri.AbsolutePath.EndsWith(CacheControlTestFilePath, StringComparison.Ordinal))
			{
				observedContentType = request.ContentType;
			}
			return null;
		});

		Assert.Equal("text/plain", observedContentType);
	}

	private async Task<string> GetServedCacheControlHeaderAsync(Func<BlazorWebViewStaticContentRequest, string> provider)
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
				const response = await fetch('/{{CacheControlTestFilePath}}');
				return response.headers.get('cache-control');
				""");
		});

		return cacheControl;
	}
}
