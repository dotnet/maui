#nullable enable
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

/// <summary>
/// Tests for URL-to-file path resolution in HybridWebView.
/// Path.Combine ignores the first argument when the second starts with a path
/// separator, which can cause incorrect file resolution. These tests verify that
/// file resolution correctly handles various URL path formats.
/// </summary>
[Category(TestCategory.HybridWebView)]
#if WINDOWS
[Collection(WebViewsCollection)]
#endif
public partial class HybridWebViewTests_ContentRootResolution : HybridWebViewTestsBase
{
	public class UrlResolutionResult
	{
		public int status { get; set; }
		public int bodyLength { get; set; }
		public string bodyPreview { get; set; } = "";
		public string url { get; set; } = "";
	}

	[JsonSourceGenerationOptions(WriteIndented = true)]
	[JsonSerializable(typeof(UrlResolutionResult))]
	[JsonSerializable(typeof(string))]
	internal partial class UrlResolutionJsonContext : JsonSerializerContext
	{
	}

	private Task RunUrlResolutionTest(string path, string mode, Action<UrlResolutionResult> assertion) =>
		RunTest("urlresolution.html", async (hybridWebView) =>
		{
			var result = await hybridWebView.InvokeJavaScriptAsync<UrlResolutionResult>(
				"TestUrlResolution",
				UrlResolutionJsonContext.Default.UrlResolutionResult,
				[path, mode],
				[UrlResolutionJsonContext.Default.String, UrlResolutionJsonContext.Default.String]);

			Assert.NotNull(result);
			assertion(result);
		});

	// ============================================================
	// Relative paths — files inside content root resolve correctly
	// ============================================================

	[Theory]
	[InlineData("index.html")]
	[InlineData("urlresolution.html")]
	[InlineData("safe-file.txt")]
	public Task RelativePaths_ResolveToContent(string path) =>
		RunUrlResolutionTest(path, "relative", result =>
		{
			Assert.Equal(200, result.status);
			Assert.True(result.bodyLength > 0, $"Expected content for '{path}' but got empty response.");
		});

	[Fact]
	public Task KnownFile_ReturnsExpectedContent() =>
		RunUrlResolutionTest("safe-file.txt", "relative", result =>
		{
			Assert.Equal(200, result.status);
			Assert.Contains("content directory", result.bodyPreview, StringComparison.Ordinal);
		});

	// ============================================================
	// Rooted paths — Path.Combine drops the root when the second
	// argument starts with a separator, so these should not resolve
	// ============================================================

	[Theory]
	[InlineData("//images/logo.png")]
	[InlineData("//data/readme.txt")]
	[InlineData("//content/page.html")]
	public Task RootedPath_DoesNotResolve(string path) =>
		RunUrlResolutionTest(path, "origin", result =>
		{
			Assert.NotEqual(200, result.status);
		});

	// ============================================================
	// Dot-dot segments — should not resolve above content root
	// ============================================================

	[Theory]
	[InlineData("../readme.txt")]
	[InlineData("../../data/config.txt")]
	[InlineData("subfolder/../../readme.txt")]
	public Task DotDotSegments_DoNotResolveAboveRoot(string path) =>
		RunUrlResolutionTest(path, "relative", result =>
		{
			Assert.NotEqual(200, result.status);
		});

	// ============================================================
	// Encoded separators — should not affect path resolution
	// ============================================================

	[Theory]
	[InlineData("%2F%2Fimages%2Flogo.png")]
	[InlineData("%2e%2e/readme.txt")]
	public Task EncodedSeparators_DoNotAffectResolution(string path) =>
		RunUrlResolutionTest(path, "relative", result =>
		{
			Assert.NotEqual(200, result.status);
		});
}
