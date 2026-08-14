#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using AndroidX.Browser.Auth;
using Microsoft.Maui.Authentication;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[CollectionDefinition(CollectionName, DisableParallelization = true)]
	public sealed class AndroidWebAuthenticatorCollection
	{
		public const string CollectionName = "Android WebAuthenticator";
	}

	[Collection(AndroidWebAuthenticatorCollection.CollectionName)]
	public class WebAuthenticator_Android_Tests : IDisposable
	{
		readonly List<WebAuthenticatorRequest> requests = new();

		[Theory]
		[InlineData("https://example.com/authorize", "maui-auth://callback", true)]
		[InlineData("http://example.com/authorize", "https://app.example/callback", true)]
		[InlineData("https://example.com/authorize", "https://app.example:8443/callback", false)]
		[InlineData("https://example.com/authorize", "http://app.example/callback", false)]
		[InlineData("maui-auth://authorize", "maui-auth://callback", false)]
		public void AuthTabEligibilityRequiresAWebAuthorizationUrlAndNonHttpCallback(
			string authorizationUrl,
			string callbackUrl,
			bool expected)
		{
			Assert.Equal(
				expected,
				WebAuthenticatorImplementation.CanUseAuthTab(new Uri(authorizationUrl), new Uri(callbackUrl)));
		}

		[Fact]
		public async Task BuiltInCallbackPrecedesCustomFacade()
		{
			var request = Begin();
			var custom = new CallbackAuthenticator();
			using var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse("maui-auth://callback?code=sample-code"));

			Assert.True(custom.OnResume(intent));
			Assert.Equal(0, custom.CallbackCount);
			Assert.Equal("sample-code", (await request.Task).Properties["code"]);
		}

		[Fact]
		public void UnhandledCallbackFlowsToCustomFacade()
		{
			var custom = new CallbackAuthenticator();
			using var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse("other-auth://callback"));

			Assert.True(custom.OnResume(intent));
			Assert.Equal(1, custom.CallbackCount);
		}

		[Fact]
		public async Task RouteMismatchFlowsToCustomFacadeAndLeavesBuiltInRequestPending()
		{
			var request = Begin();
			var custom = new CallbackAuthenticator();
			using var mismatchIntent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse("maui-auth://other?code=ignored"));
			using var callbackIntent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse("maui-auth://callback?code=sample-code"));

			Assert.True(custom.OnResume(mismatchIntent));
			Assert.Equal(1, custom.CallbackCount);
			Assert.False(request.Task.IsCompleted);

			Assert.True(custom.OnResume(callbackIntent));
			Assert.Equal(1, custom.CallbackCount);
			Assert.Equal("sample-code", (await request.Task).Properties["code"]);
		}

		[Fact]
		public async Task SuccessfulAuthTabResultCompletesTheBoundRequest()
		{
			var request = Begin();

			WebAuthenticatorIntermediateActivity.HandleAuthTabResult(
				request.Id,
				AuthTabIntent.ResultOk,
				"maui-auth://callback?code=sample-code");

			Assert.Equal("sample-code", (await request.Task).Properties["code"]);
		}

		[Fact]
		public async Task AuthTabBackOrCloseCancelsTheBoundRequest()
		{
			var request = Begin();

			WebAuthenticatorIntermediateActivity.HandleAuthTabResult(
				request.Id,
				AuthTabIntent.ResultCanceled,
				null);

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request.Task);
		}

		[Theory]
		[InlineData(AuthTabIntent.ResultVerificationFailed, "verify")]
		[InlineData(AuthTabIntent.ResultVerificationTimedOut, "timed out")]
		[InlineData(-42, "unknown")]
		public async Task AuthTabFailuresAreTerminalAndRedacted(int resultCode, string expectedMessage)
		{
			var request = Begin();

			WebAuthenticatorIntermediateActivity.HandleAuthTabResult(request.Id, resultCode, null);

			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => request.Task);
			Assert.Contains(expectedMessage, exception.Message, StringComparison.OrdinalIgnoreCase);
			Assert.DoesNotContain("?", exception.Message, StringComparison.Ordinal);
		}

		[Fact]
		public async Task SuccessfulAuthTabResultWithoutCallbackUriFaultsTheBoundRequest()
		{
			var request = Begin();

			WebAuthenticatorIntermediateActivity.HandleAuthTabResult(request.Id, AuthTabIntent.ResultOk, null);

			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => request.Task);
			Assert.Contains("did not include", exception.Message, StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public async Task AuthTabTerminalMismatchFaultsTheBoundRequest()
		{
			var request = Begin();

			WebAuthenticatorIntermediateActivity.HandleAuthTabResult(
				request.Id,
				AuthTabIntent.ResultOk,
				"maui-auth://wrong-path?code=sample-code");

			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => request.Task);
			Assert.Contains("did not match", exception.Message, StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public async Task StaleAuthTabResultCannotCompleteASuccessor()
		{
			var first = Begin();
			WebAuthenticatorIntermediateActivity.HandleAuthTabResult(
				first.Id,
				AuthTabIntent.ResultCanceled,
				null);
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first.Task);
			Assert.True(WebAuthenticatorRequestManager.End(first));

			var successor = Begin();
			WebAuthenticatorIntermediateActivity.HandleAuthTabResult(
				first.Id,
				AuthTabIntent.ResultOk,
				"maui-auth://callback?code=stale");

			Assert.False(successor.Task.IsCompleted);

			WebAuthenticatorIntermediateActivity.HandleAuthTabResult(
				successor.Id,
				AuthTabIntent.ResultOk,
				"maui-auth://callback?code=current");
			Assert.Equal("current", (await successor.Task).Properties["code"]);
		}

		[Fact]
		public async Task MissingOrUnknownRequestIdDoesNotAffectTheCurrentRequest()
		{
			var request = Begin();

			WebAuthenticatorIntermediateActivity.HandleAuthTabResult(0, AuthTabIntent.ResultOk, null);
			WebAuthenticatorIntermediateActivity.HandleAuthTabResult(request.Id + 1, AuthTabIntent.ResultOk, null);

			Assert.False(request.Task.IsCompleted);

			WebAuthenticatorIntermediateActivity.HandleAuthTabResult(
				request.Id,
				AuthTabIntent.ResultOk,
				"maui-auth://callback?code=current");
			await request.Task;
		}

		public void Dispose()
		{
			for (var index = requests.Count - 1; index >= 0; index--)
				WebAuthenticatorRequestManager.End(requests[index]);
		}

		WebAuthenticatorRequest Begin()
		{
			var request = WebAuthenticatorRequestManager.Begin(
				new Uri("https://example.com/authorize"),
				new Uri("maui-auth://callback"),
				responseDecoder: null,
				prefersEphemeralWebBrowserSession: false,
				cancellationToken: default);

			requests.Add(request);
			return request;
		}

		sealed class CallbackAuthenticator : IWebAuthenticator, IPlatformWebAuthenticatorCallback
		{
			internal int CallbackCount { get; private set; }

			public bool OnResumeCallback(Intent intent)
			{
				CallbackCount++;
				return true;
			}

			public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions) =>
				throw new NotSupportedException();

			public Task<WebAuthenticatorResult> AuthenticateAsync(
				WebAuthenticatorOptions webAuthenticatorOptions,
				CancellationToken cancellationToken) =>
				throw new NotSupportedException();
		}
	}
}
