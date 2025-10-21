#nullable enable
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.HybridWebView)]
#if WINDOWS
[Collection(WebViewsCollection)]
#endif
public class HybridWebViewTests_InvokeDotNet : HybridWebViewTestsBase
{
	[Fact]
	public Task GetRequestsAreBlocked() =>
		RunTest("invokedotnetfails.html", async (hybridWebView) =>
		{
			// Test that GET requests to the __hwvInvokeDotNet endpoint are blocked
			// This should return a 403 Forbidden response because it is ALSO missing
			// the token header.
			await hybridWebView.EvaluateJavaScriptAsync("TestGetRequestIsBlocked()");

			// Wait for the test to complete
			await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

			// Get the result and verify it's a 403 error
			var result = await hybridWebView.EvaluateJavaScriptAsync("GetTestResult()");
			Assert.Equal("403", result);
		});

	[Fact]
	public Task GetRequestWithHeaderIsBlocked() =>
		RunTest("invokedotnetfails.html", async (hybridWebView) =>
		{
			// Test that GET requests to the __hwvInvokeDotNet endpoint are blocked
			// This should return a 405 Method Not Allowed response
			await hybridWebView.EvaluateJavaScriptAsync("TestGetRequestWithHeaderIsBlocked()");

			// Wait for the test to complete
			await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

			// Get the result and verify it's a 405 error
			var result = await hybridWebView.EvaluateJavaScriptAsync("GetTestResult()");
			Assert.Equal("405", result);
		});

	[Fact]
	public Task MissingTokenHeaderIsBlocked() =>
		RunTest("invokedotnetfails.html", async (hybridWebView) =>
		{
			// Test that POST requests without the X-Maui-Invoke-Token header are blocked
			// This should return a 403 Forbidden response
			await hybridWebView.EvaluateJavaScriptAsync("TestMissingTokenHeader()");

			// Wait for the test to complete
			await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

			// Get the result and verify it's a 403 error
			var result = await hybridWebView.EvaluateJavaScriptAsync("GetTestResult()");
			Assert.Equal("403", result);
		});

	[Fact]
	public Task InvalidTokenHeaderIsBlocked() =>
		RunTest("invokedotnetfails.html", async (hybridWebView) =>
		{
			// Test that POST requests with an incorrect X-Maui-Invoke-Token header value are blocked
			// This should return a 403 Forbidden response
			await hybridWebView.EvaluateJavaScriptAsync("TestInvalidTokenHeader()");

			// Wait for the test to complete
			await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

			// Get the result and verify it's a 403 error
			var result = await hybridWebView.EvaluateJavaScriptAsync("GetTestResult()");
			Assert.Equal("403", result);
		});

#if ANDROID
	[Fact]
	public Task MissingBodyHeaderOnAndroidIsBlocked() =>
		RunTest("invokedotnetfails.html", async (hybridWebView) =>
		{
			// Test that POST requests without the X-Maui-Request-Body header are blocked on Android
			// This should return a 400 Bad Request response
			await hybridWebView.EvaluateJavaScriptAsync("TestMissingBodyHeader()");

			// Wait for the test to complete
			await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

			// Get the result and verify it's a 400 error
			var result = await hybridWebView.EvaluateJavaScriptAsync("GetTestResult()");
			Assert.Equal("400", result);
		});
#endif

	[Fact]
	public Task EmptyBodyIsBlocked() =>
		RunTest("invokedotnetfails.html", async (hybridWebView) =>
		{
			// Test that POST requests with an empty body are blocked
			// This should return a 400 Bad Request response
			await hybridWebView.EvaluateJavaScriptAsync("TestEmptyBody()");

			// Wait for the test to complete
			await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

			// Get the result and verify it's a 400 error
			var result = await hybridWebView.EvaluateJavaScriptAsync("GetTestResult()");
			Assert.Equal("400", result);
		});
}
