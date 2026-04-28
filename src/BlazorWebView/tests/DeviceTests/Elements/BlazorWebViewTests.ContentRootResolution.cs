using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

/// <summary>
/// Tests for URL-to-file path resolution in BlazorWebView.
/// Path.Combine ignores the first argument when the second starts with a path
/// separator, which can cause incorrect file resolution. These tests verify that
/// file resolution correctly handles various URL path formats.
/// </summary>
public partial class BlazorWebViewTests
{
	public class UrlResolutionResult
	{
		public int status { get; set; }
		public int bodyLength { get; set; }
		public string bodyPreview { get; set; } = "";
		public string url { get; set; } = "";
	}

	[JsonSerializable(typeof(UrlResolutionResult))]
	internal partial class UrlResolutionJsonContext : JsonSerializerContext
	{
	}

	private async Task RunUrlResolutionTest(string path, string mode, Action<UrlResolutionResult> assertion)
	{
		await RunTest(async (blazorWebView, handler) =>
		{
			var jsPath = path.Replace("'", "\\'", StringComparison.Ordinal);
			var result = await WebViewHelpers.ExecuteAsyncScriptAndWaitForResult<UrlResolutionResult>(
				handler.PlatformView,
				"try {" +
				"  let url;" +
				"  if ('" + mode + "' === 'origin') {" +
				"    url = window.location.origin + '" + jsPath + "';" +
				"  } else {" +
				"    url = '" + jsPath + "';" +
				"  }" +
				"  const response = await fetch(url);" +
				"  const body = await response.text();" +
				"  return {" +
				"    status: response.status," +
				"    bodyLength: body.length," +
				"    bodyPreview: body.substring(0, 200)," +
				"    url: url" +
				"  };" +
				"} catch (e) {" +
				"  let url = ('" + mode + "' === 'origin') ? window.location.origin + '" + jsPath + "' : '" + jsPath + "';" +
				"  return {" +
				"    status: -1," +
				"    bodyLength: 0," +
				"    bodyPreview: e.toString()," +
				"    url: url" +
				"  };" +
				"}");

			Assert.NotNull(result);
			assertion(result);
		});
	}

	/// <summary>
	/// Checks if the response is the host page (SPA fallback) rather than actual file content.
	/// BlazorWebView returns the host page for extensionless paths as part of SPA routing.
	/// </summary>
	static bool IsSpaFallback(UrlResolutionResult result) =>
		result.bodyPreview.Contains("blazor.webview.js", StringComparison.Ordinal) ||
		result.bodyPreview.Contains("testhtmlloaded", StringComparison.Ordinal) ||
		result.bodyPreview.Contains("There is no content at", StringComparison.Ordinal);

	// NOTE: No HostPage_LoadsSuccessfully test here because the Blazor host page
	// is only served for navigation requests (ResourceContext.Document), not for
	// fetch() requests. RunTest already verifies the host page loads by waiting
	// for the Blazor component to render before running the test lambda.

	// ============================================================
	// Positive test — a known-good asset must still load after the
	// path-hardening changes so we don't accidentally block legit
	// requests
	// ============================================================

	[Fact]
	public Task Blazor_KnownFrameworkAsset_LoadsSuccessfully() =>
		RunUrlResolutionTest("_framework/blazor.webview.js", "relative", result =>
		{
			Assert.Equal(200, result.status);
			Assert.True(result.bodyLength > 0, "Framework script should return content");
		});

	// ============================================================
	// Rooted paths — Path.Combine drops the root when the second
	// argument starts with a separator, so these should not resolve
	// ============================================================

	[Theory]
	[InlineData("//images/logo.png")]
	[InlineData("//data/readme.txt")]
	[InlineData("//content/page.html")]
	public Task Blazor_RootedPath_DoesNotResolve(string path) =>
		RunUrlResolutionTest(path, "origin", result =>
		{
			Assert.True(
				result.status != 200 || IsSpaFallback(result),
				$"Path '{path}' unexpectedly returned content (status={result.status}, length={result.bodyLength})");
		});

	// ============================================================
	// Dot-dot segments — should not resolve above content root
	// ============================================================

	[Theory]
	[InlineData("../readme.txt")]
	[InlineData("../../data/config.txt")]
	[InlineData("subfolder/../../readme.txt")]
	public Task Blazor_DotDotSegments_DoNotResolveAboveRoot(string path) =>
		RunUrlResolutionTest(path, "relative", result =>
		{
			Assert.True(
				result.status != 200 || IsSpaFallback(result),
				$"Path '{path}' unexpectedly returned content (status={result.status}, length={result.bodyLength})");
		});

	// ============================================================
	// Encoded separators — should not affect path resolution
	// ============================================================

	[Theory]
	[InlineData("%2F%2Fimages%2Flogo.png")]
	[InlineData("%2e%2e/readme.txt")]
	public Task Blazor_EncodedSeparators_DoNotAffectResolution(string path) =>
		RunUrlResolutionTest(path, "relative", result =>
		{
			Assert.True(
				result.status != 200 || IsSpaFallback(result),
				$"Path '{path}' unexpectedly returned content (status={result.status}, length={result.bodyLength})");
		});
}
