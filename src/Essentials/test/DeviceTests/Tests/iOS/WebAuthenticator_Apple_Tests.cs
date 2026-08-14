#if IOS || MACCATALYST
#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Authentication;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[CollectionDefinition(CollectionName, DisableParallelization = true)]
	public sealed class AppleWebAuthenticatorCollection
	{
		public const string CollectionName = "Apple WebAuthenticator";
	}

	[Collection(AppleWebAuthenticatorCollection.CollectionName)]
	[Category("WebAuthenticator")]
	public class WebAuthenticator_Apple_Tests : IDisposable
	{
		readonly List<WebAuthenticatorRequest> requests = new();

		[Fact]
		public void ModernSessionRejectsNonWebAuthenticationUrl()
		{
			Assert.Throws<InvalidOperationException>(() =>
				WebAuthenticatorImplementation.ValidateModernSessionUrls(
					new Uri("maui-auth:/authorize"),
					new Uri("maui-auth:/callback")));
		}

		[Fact]
		public void ModernSessionRejectsHttpCallback()
		{
			Assert.Throws<InvalidOperationException>(() =>
				WebAuthenticatorImplementation.ValidateModernSessionUrls(
					new Uri("https://example.com/authorize"),
					new Uri("http://example.com/callback")));
		}

		[Fact]
		public void ModernSessionRejectsNonDefaultHttpsCallbackPort()
		{
			Assert.Throws<InvalidOperationException>(() =>
				WebAuthenticatorImplementation.ValidateModernSessionUrls(
					new Uri("https://example.com/authorize"),
					new Uri("https://example.com:8443/callback")));
		}

		[Fact]
		public void ModernSessionAcceptsCustomSchemeWithRoute()
		{
			WebAuthenticatorImplementation.ValidateModernSessionUrls(
				new Uri("https://example.com/authorize"),
				new Uri("maui-auth://callback/complete"));
		}

		[Fact]
		public void HttpsCallbackAvailabilityMatchesTheRunningPlatform()
		{
			var validate = () => WebAuthenticatorImplementation.ValidateModernSessionUrls(
				new Uri("https://example.com/authorize"),
				new Uri("https://login.example.com/callback/complete"));

#if IOS
			if (OperatingSystem.IsIOSVersionAtLeast(17, 4))
#elif MACCATALYST
			if (OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
#endif
				validate();
			else
				Assert.Throws<FeatureNotSupportedException>(validate);
		}

		[Fact]
		public async Task NativeSuccessCompletesTheBoundRequest()
		{
			var request = Begin();

			WebAuthenticatorImplementation.CompleteNativeSession(
				request,
				"maui-auth://callback?code=sample-code",
				wasCanceled: false,
				failed: false);

			Assert.Equal("sample-code", (await request.Task).Properties["code"]);
		}

		[Fact]
		public async Task NativeCancellationCancelsTheBoundRequest()
		{
			var request = Begin();

			WebAuthenticatorImplementation.CompleteNativeSession(
				request,
				absoluteCallbackUrl: null,
				wasCanceled: true,
				failed: false);

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request.Task);
		}

		[Fact]
		public async Task NativeFailureFaultsTheBoundRequest()
		{
			var request = Begin();

			WebAuthenticatorImplementation.CompleteNativeSession(
				request,
				absoluteCallbackUrl: null,
				wasCanceled: false,
				failed: true);

			await Assert.ThrowsAsync<InvalidOperationException>(() => request.Task);
		}

		[Fact]
		public Task NativeMissingCallbackCannotLeaveTheBoundRequestPending() =>
			AssertNativeTerminalFailure(null, "did not include");

		[Fact]
		public Task NativeMismatchCannotLeaveTheBoundRequestPending() =>
			AssertNativeTerminalFailure("maui-auth://wrong?code=sample-code", "did not match");

		async Task AssertNativeTerminalFailure(string? callback, string expectedMessage)
		{
			var request = Begin();

			WebAuthenticatorImplementation.CompleteNativeSession(
				request,
				callback,
				wasCanceled: false,
				failed: false);

			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => request.Task);
			Assert.Contains(expectedMessage, exception.Message, StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public async Task LateNativeCallbackCannotCompleteASuccessor()
		{
			var first = Begin();
			WebAuthenticatorImplementation.CompleteNativeSession(first, null, wasCanceled: true, failed: false);
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first.Task);
			Assert.True(WebAuthenticatorRequestManager.End(first));

			var successor = Begin();
			WebAuthenticatorImplementation.CompleteNativeSession(
				first,
				"maui-auth://callback?code=stale",
				wasCanceled: false,
				failed: false);
			Assert.False(successor.Task.IsCompleted);

			WebAuthenticatorImplementation.CompleteNativeSession(
				successor,
				"maui-auth://callback?code=current",
				wasCanceled: false,
				failed: false);
			Assert.Equal("current", (await successor.Task).Properties["code"]);
		}

		[Fact]
		public async Task NativePostingDoesNotCompleteInline()
		{
			var callerThread = Environment.CurrentManagedThreadId;
			var insideNativeStack = true;
			var completedInline = false;
			var request = Begin(beforeCallbackCompletion: () =>
				completedInline = insideNativeStack && Environment.CurrentManagedThreadId == callerThread);
			using var callback = new NSUrl("maui-auth://callback?code=sample-code");

			WebAuthenticatorImplementation.PostNativeCompletion(request, callback, error: null);
			insideNativeStack = false;

			await request.Task;
			Assert.False(completedInline);
		}

		public void Dispose()
		{
			for (var index = requests.Count - 1; index >= 0; index--)
				WebAuthenticatorRequestManager.End(requests[index]);
		}

		WebAuthenticatorRequest Begin(Action? beforeCallbackCompletion = null)
		{
			var request = WebAuthenticatorRequestManager.Begin(
				new Uri("https://example.com/authorize"),
				new Uri("maui-auth://callback"),
				responseDecoder: null,
				prefersEphemeralWebBrowserSession: false,
				cancellationToken: default,
				beforeCallbackCompletion);

			requests.Add(request);
			return request;
		}
	}
}
#endif
