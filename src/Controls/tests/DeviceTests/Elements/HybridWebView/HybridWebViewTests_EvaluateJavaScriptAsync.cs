#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.HybridWebView)]
#if WINDOWS
[Collection(WebViewsCollection)]
#endif
public partial class HybridWebViewTests_EvaluateJavaScriptAsync : HybridWebViewTestsBase
{
	[Fact]
	public async Task EvaluationWithoutPlatformViewIsCanceled()
	{
		var handler = new HybridWebViewHandler();
		var hybridWebView = new HybridWebView();
		var request = new EvaluateJavaScriptAsyncRequest("1+1");

		Assert.Null(((IElementHandler)handler).PlatformView);

		HybridWebViewHandler.MapEvaluateJavaScriptAsync(handler, hybridWebView, request);

		await Assert.ThrowsAnyAsync<OperationCanceledException>(
			() => request.Task.WaitAsync(TimeSpan.FromSeconds(5)));

		Assert.True(request.Task.IsCanceled);
	}

	[Fact]
	public Task EvaluateJavaScriptAndGetResult() =>
		RunTest(async (hybridWebView) =>
		{
			// Run some JavaScript to call a method and get result
			var result1 = await hybridWebView.EvaluateJavaScriptAsync("EvaluateMeWithParamsAndReturn('abc', 'def')");
			Assert.Equal("abcdef", result1);

			// Run some JavaScript to get an arbitrary result by running JavaScript
			var result2 = await hybridWebView.EvaluateJavaScriptAsync("window.TestKey");
			Assert.Equal("test_value", result2);
		});
}
