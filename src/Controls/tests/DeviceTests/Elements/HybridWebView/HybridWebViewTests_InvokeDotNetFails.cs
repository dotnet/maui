#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
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
		RunJavaScriptTest(async (hybridWebView, result) =>
		{
			// Test that GET requests to the __hwvInvokeDotNet endpoint are blocked
			// This should return a 400 Bad Request response because it is ALSO missing the token header.
			Assert.Equal("400", result);
		});

	[Fact]
	public Task GetRequestWithHeaderIsBlocked() =>
		RunJavaScriptTest(async (hybridWebView, result) =>
		{
			// Test that GET requests to the __hwvInvokeDotNet endpoint are blocked
			// This should return a 405 Method Not Allowed response
			Assert.Equal("405", result);
		});

	[Fact]
	public Task MissingTokenHeaderIsBlocked() =>
		RunJavaScriptTest(async (hybridWebView, result) =>
		{
			// Test that POST requests without the X-Maui-Invoke-Token header are blocked
			// This should return a 400 Bad Request response
			Assert.Equal("400", result);
		});

	[Fact]
	public Task InvalidTokenHeaderIsBlocked() =>
		RunJavaScriptTest(async (hybridWebView, result) =>
		{
			// Test that POST requests with an incorrect X-Maui-Invoke-Token header value are blocked
			// This should return a 400 Bad Request response
			Assert.Equal("400", result);
		});

#if ANDROID
	[Fact]
	public Task MissingBodyHeaderIsBlocked() =>
		RunJavaScriptTest(async (hybridWebView, result) =>
		{
			// Test that POST requests without the X-Maui-Request-Body header are blocked on Android
			// This should return a 400 Bad Request response
			Assert.Equal("400", result);
		});
#endif

	[Fact]
	public Task EmptyBodyIsBlocked() =>
		RunJavaScriptTest(async (hybridWebView, result) =>
		{
			// Test that POST requests with an empty body are blocked
			// This should return a 400 Bad Request response
			Assert.Equal("400", result);
		});

	[Fact]
	public Task ValidRequestIsNotBlocked() =>
		RunJavaScriptTest(async (hybridWebView, result) =>
		{
			// Test that valid POST requests with proper headers and body are not blocked
			// Valid requests should get a 200 status or possibly 404 if the method doesn't exist
			// but should not get blocked with 400/403/405
			Assert.True(result != "400" && result != "403" && result != "405",
				$"Valid request was unexpectedly blocked with status: {result}");
		});

	[Fact]
	public Task IframeRequestIsBlocked() =>
		RunJavaScriptTest(async (hybridWebView, result) =>
		{
			// Test that requests from iframe contexts are blocked
			// Iframe requests should be blocked or result in an error
			Assert.True(result == "400" || result == "403" || result == "405" || (result?.StartsWith("error:") == true),
				$"Iframe request was not properly blocked, got status: {result}");
		});

	protected Task RunJavaScriptTest(Func<HybridWebView, string?, Task> validateResult, [CallerMemberName] string? jsMethodName = null) =>
		RunTest("invokedotnetfails.html", async (hybridWebView) =>
		{
			// Execute the JavaScript test method
			await hybridWebView.EvaluateJavaScriptAsync($"Test{jsMethodName}()");

			// Wait for the test to complete
			await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

			// Get the result and let the caller validate it
			var result = await hybridWebView.EvaluateJavaScriptAsync("GetTestResult()");

			await validateResult(hybridWebView, result);
		});
}
