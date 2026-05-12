#nullable enable
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.HybridWebView)]
#if WINDOWS
[Collection(WebViewsCollection)]
#endif
public partial class HybridWebViewTests_ExceptionHandling : HybridWebViewTestsBase
{
	[Fact]
	public Task CSharpMethodThatThrowsException_ShouldPropagateToJavaScript() =>
		RunTest("exception-tests.html", async (hybridWebView) =>
		{
			var invokeJavaScriptTarget = new TestExceptionMethods();
			hybridWebView.SetInvokeJavaScriptTarget(invokeJavaScriptTarget);

			// Tell JavaScript to invoke the method that throws an exception
			hybridWebView.SendRawMessage("ThrowException");

			// Wait for JavaScript to handle the exception
			await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

			// Check that JavaScript caught the exception
			var caughtError = await hybridWebView.EvaluateJavaScriptAsync("GetLastError()");
			Assert.Equal("Test exception message", caughtError);

			// Check that the method was called before throwing
			Assert.Equal("ThrowException", invokeJavaScriptTarget.LastMethodCalled);
		});

	[Fact]
	public Task CSharpAsyncMethodThatThrowsException_ShouldPropagateToJavaScript() =>
		RunTest("exception-tests.html", async (hybridWebView) =>
		{
			var invokeJavaScriptTarget = new TestExceptionMethods();
			hybridWebView.SetInvokeJavaScriptTarget(invokeJavaScriptTarget);

			// Tell JavaScript to invoke the async method that throws an exception
			hybridWebView.SendRawMessage("ThrowExceptionAsync");

			// Wait for JavaScript to handle the exception
			await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

			// Check that JavaScript caught the async exception
			var caughtError = await hybridWebView.EvaluateJavaScriptAsync("GetLastError()");
			Assert.Equal("Async test exception", caughtError);

			// Check that the method was called before throwing
			Assert.Equal("ThrowExceptionAsync", invokeJavaScriptTarget.LastMethodCalled);
		});

	[Fact]
	public Task CSharpMethodThatThrowsCustomException_ShouldIncludeExceptionDetails() =>
		RunTest("exception-tests.html", async (hybridWebView) =>
		{
			var invokeJavaScriptTarget = new TestExceptionMethods();
			hybridWebView.SetInvokeJavaScriptTarget(invokeJavaScriptTarget);

			// Tell JavaScript to invoke the method that throws a custom exception
			hybridWebView.SendRawMessage("ThrowCustomException");

			// Wait for JavaScript to handle the exception
			await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

			// Check that JavaScript caught the exception with custom details
			var errorType = await hybridWebView.EvaluateJavaScriptAsync("GetLastErrorType()");
			var errorMessage = await hybridWebView.EvaluateJavaScriptAsync("GetLastErrorMessage()");

			Assert.Equal("ArgumentException", errorType);
			Assert.Equal("Custom argument exception", errorMessage);
		});

	[Fact]
	public Task CSharpMethodThatSucceeds_ShouldStillWorkNormally() =>
		RunTest("exception-tests.html", async (hybridWebView) =>
		{
			var invokeJavaScriptTarget = new TestExceptionMethods();
			hybridWebView.SetInvokeJavaScriptTarget(invokeJavaScriptTarget);

			// Tell JavaScript to invoke a normal method that doesn't throw
			hybridWebView.SendRawMessage("SuccessMethod");

			// Wait for JavaScript to complete
			await WebViewHelpers.WaitForHtmlStatusSet(hybridWebView);

			// Check that JavaScript got the normal result
			var result = await hybridWebView.EvaluateJavaScriptAsync("GetLastResult()");
			Assert.Contains("Success!", result, StringComparison.Ordinal);

			// Check that no error was thrown
			var hasError = await hybridWebView.EvaluateJavaScriptAsync("HasError()");
			Assert.Contains("false", hasError, StringComparison.Ordinal);
		});

	private class TestExceptionMethods
	{
		public string? LastMethodCalled { get; private set; }

		public void ThrowException()
		{
			LastMethodCalled = nameof(ThrowException);
			throw new InvalidOperationException("Test exception message");
		}

		public void ThrowCustomException()
		{
			LastMethodCalled = nameof(ThrowCustomException);
			throw new ArgumentException("Custom argument exception");
		}

		public async Task ThrowExceptionAsync()
		{
			LastMethodCalled = nameof(ThrowExceptionAsync);
			await Task.Delay(10); // Make it actually async
			throw new InvalidOperationException("Async test exception");
		}

		public string SuccessMethod()
		{
			LastMethodCalled = nameof(SuccessMethod);
			return "Success!";
		}
	}
}