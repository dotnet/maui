#nullable enable
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.HybridWebView)]
#if WINDOWS
[Collection(WebViewsCollection)]
#endif
public partial class HybridWebViewTests_EvaluateJavaScriptAsync : HybridWebViewTestsBase
{
	[Fact]
	public Task EvaluateJavaScriptAsync_WithStringParameters() =>
		RunTest(async (hybridWebView) =>
		{
			// Run some JavaScript to call a method and get result
			var result1 = await hybridWebView.EvaluateJavaScriptAsync("EvaluateMeWithParamsAndReturn('abc', 'def')");
			Assert.Equal("abcdef", result1);
		});

	[Fact]
	public Task EvaluateJavaScriptAsync_WithNumberParameters() =>
		RunTest(async (hybridWebView) =>
		{
			// Run some JavaScript to call a method and get result
			var result1 = await hybridWebView.EvaluateJavaScriptAsync("EvaluateMeWithParamsAndReturn(1, 2)");
			Assert.Equal("3", result1);
		});

	[Fact]
	public Task EvaluateJavaScriptAsync_GetsProperty() =>
		RunTest(async (hybridWebView) =>
		{
			// Run some JavaScript to get an arbitrary result by running JavaScript
			var result2 = await hybridWebView.EvaluateJavaScriptAsync("window.TestKey");
			Assert.Equal("test_value", result2);
		});

	[Fact]
	public Task EvaluateJavaScriptAsync_HandlesDoubleQuotes() =>
		RunTest(async (hybridWebView) =>
		{
			// Run some JavaScript to call a method and get result
			var result1 = await hybridWebView.EvaluateJavaScriptAsync("EvaluateMeWithParamsAndReturn('\"Hel', 'lo!\"')");
			Assert.Equal("\"Hello!\"", result1);
		});

	[Fact]
	public Task EvaluateJavaScriptAsync_HandlesSingleQuotes() =>
		RunTest(async (hybridWebView) =>
		{
			// Run some JavaScript to call a method and get result
			var result1 = await hybridWebView.EvaluateJavaScriptAsync("EvaluateMeWithParamsAndReturn('\\'Hel', 'lo!\\'')");
			Assert.Equal("'Hello!'", result1);
		});

	[Fact]
	public Task EvaluateJavaScriptAsync_HandlesDoubleAndSingleQuotes() =>
		RunTest(async (hybridWebView) =>
		{
			// Run some JavaScript to call a method and get result
			var result1 = await hybridWebView.EvaluateJavaScriptAsync("EvaluateMeWithParamsAndReturn('\"Hel', 'lo!\\'')");
			Assert.Equal("\"Hello!'", result1);
		});
}
