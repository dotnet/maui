using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
#if IOS || MACCATALYST
	[Fact]
	public async Task BlazorWebViewScrollBounceDisabledByDefault()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(NoOpComponent), Selector = "#app", });

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);

			// Verify that bounce scrolling is disabled by default to make apps feel more native
			Assert.False(platformWebView.ScrollView.Bounces);
			Assert.False(platformWebView.ScrollView.AlwaysBounceVertical);
			Assert.False(platformWebView.ScrollView.AlwaysBounceHorizontal);
		});
	}
#endif

#if ANDROID
	const string SmallFontSpanId = "smallFontSpan";
	const string SmallFontCssValue = "4.87761px";

	static class SmallFontTestStaticFilesContents
	{
		public const string SmallFontIndexHtmlContent = @"<!DOCTYPE html>
<html>

<head testhtmlloaded=""true"">
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"" />
    <title>Blazor app</title>
    <base href=""/"" />
</head>

<body>
    <span id=""" + SmallFontSpanId + @""" style=""font-size:" + SmallFontCssValue + @";"">tiny span text</span>
    <div id=""app""></div>

    <div id=""blazor-error-ui"">
        An unhandled error has occurred.
        <a href="""" class=""reload"">Reload</a>
        <a class=""dismiss"">🗙</a>
    </div>
    <script src=""_framework/blazor.webview.js"" autostart=""false""></script>

</body>

</html>
";
	}

	/// <summary>
	/// Regression test for https://github.com/dotnet/maui/issues/26924 - verifies that
	/// BlazorWebViewHandler.Android.cs sets MinimumFontSize/MinimumLogicalFontSize to 1 so
	/// small CSS font sizes (below the Android WebView's default 8px minimum) aren't clamped up.
	/// </summary>
	[Fact]
	public async Task BlazorWebViewDoesNotClampSmallCssFontSizes()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", SmallFontTestStaticFilesContents.SmallFontIndexHtmlContent },
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(NoOpComponent), Selector = "#app", });

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);
			var computedFontSizeJson = await WebViewHelpers.ExecuteScriptAsync(
				platformWebView,
				$"parseFloat(window.getComputedStyle(document.getElementById('{SmallFontSpanId}')).fontSize)");
			var computedFontSize = double.Parse(computedFontSizeJson, CultureInfo.InvariantCulture);

			Assert.True(computedFontSize < 8, $"Expected computed font size to be under the 8px clamp, but was {computedFontSize}px.");
		});
	}
#endif
}
