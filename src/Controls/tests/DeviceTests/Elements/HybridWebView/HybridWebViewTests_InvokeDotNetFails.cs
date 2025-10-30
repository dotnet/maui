#nullable enable
using System;
using System.Collections.Generic;
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
		RunJavaScriptTest(async (hybridWebView, target) =>
		{
			// Test that GET requests to the __hwvInvokeDotNet endpoint are blocked
			// This should return a 400 Bad Request response because it is ALSO missing the token header.
			Assert.Equal("400", target.TestResult);
		});

	[Fact]
	public Task GetRequestWithHeaderIsBlocked() =>
		RunJavaScriptTest(async (hybridWebView, target) =>
		{
			// Test that GET requests to the __hwvInvokeDotNet endpoint are blocked
			// This should return a 405 Method Not Allowed response
			Assert.Equal("405", target.TestResult);
		});

	[Fact]
	public Task MissingTokenHeaderIsBlocked() =>
		RunJavaScriptTest(async (hybridWebView, target) =>
		{
			// Test that POST requests without the X-Maui-Invoke-Token header are blocked
			// This should return a 400 Bad Request response
			Assert.Equal("400", target.TestResult);
		});

	[Fact]
	public Task InvalidTokenHeaderIsBlocked() =>
		RunJavaScriptTest(async (hybridWebView, target) =>
		{
			// Test that POST requests with an incorrect X-Maui-Invoke-Token header value are blocked
			// This should return a 400 Bad Request response
			Assert.Equal("400", target.TestResult);
		});

#if ANDROID
	[Fact]
	public Task MissingBodyHeaderIsBlocked() =>
		RunJavaScriptTest(async (hybridWebView, target) =>
		{
			// Test that POST requests without the X-Maui-Request-Body header are blocked on Android
			// This should return a 400 Bad Request response
			Assert.Equal("400", target.TestResult);
		});
#endif

	[Fact]
	public Task EmptyBodyIsBlocked() =>
		RunJavaScriptTest(async (hybridWebView, target) =>
		{
			// Test that POST requests with an empty body are blocked
			// This should return a 400 Bad Request response
			Assert.Equal("400", target.TestResult);
		});

	[Fact]
	public Task ValidRequestIsNotBlocked() =>
		RunJavaScriptTest(async (hybridWebView, target) =>
		{
			// Test that valid POST requests with proper headers and body are not blocked
			// Valid requests should get a 200 status or possibly 404 if the method doesn't exist
			// but should not get blocked with 400/403/405
			Assert.True(target.TestResult != "400" && target.TestResult != "403" && target.TestResult != "405",
				$"Valid request was unexpectedly blocked with status: {target.TestResult}");
		});

	[Fact]
	public Task IframeRequestIsBlocked() =>
		RunJavaScriptTest(async (hybridWebView, target) =>
		{
			// Test that requests from iframe contexts are blocked
			// iframe requests should be blocked or result in an error
#if IOS || MACCATALYST
			var error = "iframe:error:app://0.0.0.1:Load failed";
#else
			var error = "iframe:error:https://0.0.0.1:Failed to fetch";
#endif
			Assert.Equal(error, target.TestResult);
		});

	private Task RunJavaScriptTest(Func<HybridWebView, InvokeTarget, Task> validateResult, [CallerMemberName] string? jsMethodName = null) =>
		RunTest("invokedotnetfails.html", async (hybridWebView) =>
		{
			var target = new InvokeTarget();

			hybridWebView.SetInvokeJavaScriptTarget(target);

			// Execute the JavaScript test method
			await hybridWebView.EvaluateJavaScriptAsync($"Test{jsMethodName}()");

			// Wait for the test to complete
			await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

			// Get the result and let the caller validate it
			var result = await hybridWebView.EvaluateJavaScriptAsync("GetTestResult()");

			target.TestResult = result;

			await validateResult(hybridWebView, target);
		});

	private class InvokeTarget
	{
		public List<string> ParamValues { get; private set; } = new();

		public string? TestResult { get; set; }

        public void TestMethod(string param)
        {
			ParamValues.Add(param);
        }
    }
}
