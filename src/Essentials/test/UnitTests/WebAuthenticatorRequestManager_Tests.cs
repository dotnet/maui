#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using Xunit;

namespace Tests
{
	[CollectionDefinition(CollectionName, DisableParallelization = true)]
	public sealed class WebAuthenticatorRequestManagerCollection
	{
		public const string CollectionName = "WebAuthenticator request manager";
	}

	[Collection(WebAuthenticatorRequestManagerCollection.CollectionName)]
	public class WebAuthenticatorRequestManager_Tests : IDisposable
	{
		readonly List<WebAuthenticatorRequest> requests = new();

		[Fact]
		public async Task FakeTransportRejectsNullOptions()
		{
			var transport = CreateTransport();

			await Assert.ThrowsAsync<ArgumentNullException>(() => transport.AuthenticateAsync(null!));
			Assert.Equal(0, transport.LaunchCount);
		}

		[Fact]
		public async Task FakeTransportRejectsNullUrlsInOrder()
		{
			var transport = CreateTransport();
			var options = new WebAuthenticatorOptions();

			var urlException = await Assert.ThrowsAsync<ArgumentNullException>(() => transport.AuthenticateAsync(options));
			Assert.Equal("Url", urlException.ParamName);

			options.Url = new Uri("https://example.com/authorize");
			var callbackException = await Assert.ThrowsAsync<ArgumentNullException>(() => transport.AuthenticateAsync(options));
			Assert.Equal("CallbackUrl", callbackException.ParamName);
			Assert.Equal(0, transport.LaunchCount);
		}

		[Fact]
		public async Task FakeTransportRejectsRelativeUrisBeforePublishing()
		{
			var transport = CreateTransport();

			await Assert.ThrowsAsync<ArgumentException>(() => transport.AuthenticateAsync(new WebAuthenticatorOptions
			{
				Url = new Uri("authorize", UriKind.Relative),
				CallbackUrl = new Uri("maui-auth://callback"),
			}));

			await Assert.ThrowsAsync<ArgumentException>(() => transport.AuthenticateAsync(new WebAuthenticatorOptions
			{
				Url = new Uri("https://example.com/authorize"),
				CallbackUrl = new Uri("callback", UriKind.Relative),
			}));

			Assert.Equal(0, transport.LaunchCount);
		}

		[Fact]
		public async Task PreCanceledTokenIsPreservedAndDoesNotLaunch()
		{
			var transport = CreateTransport();
			using var cancellationSource = new CancellationTokenSource();
			cancellationSource.Cancel();

			var task = transport.AuthenticateAsync(CreateOptions(), cancellationSource.Token);
			var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);

			Assert.Equal(TaskStatus.Canceled, task.Status);
			Assert.Equal(cancellationSource.Token, exception.CancellationToken);
			Assert.Equal(0, transport.LaunchCount);
			Assert.Null(transport.Request);
		}

		[Fact]
		public async Task OptionsAreSnapshottedBeforeConcurrentMutation()
		{
			var originalDecoder = new CallbackDecoder(_ => new Dictionary<string, string> { ["snapshot"] = "original" });
			var replacementDecoder = new CallbackDecoder(_ => new Dictionary<string, string> { ["snapshot"] = "replacement" });
			var options = CreateOptions();
			options.ResponseDecoder = originalDecoder;
			options.PrefersEphemeralWebBrowserSession = true;

			var transport = CreateTransport();
			transport.AfterSnapshot = () =>
			{
				options.Url = new Uri("https://mutated.example/authorize");
				options.CallbackUrl = new Uri("mutated-auth://callback");
				options.ResponseDecoder = replacementDecoder;
				options.PrefersEphemeralWebBrowserSession = false;
			};

			var task = transport.AuthenticateAsync(options);
			var request = Assert.IsType<WebAuthenticatorRequest>(transport.Request);

			Assert.Equal("example.com", request.Url.Host);
			Assert.Equal("maui-auth", request.CallbackUrl.Scheme);
			Assert.Same(originalDecoder, request.ResponseDecoder);
			Assert.True(request.PrefersEphemeralWebBrowserSession);

			Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback?code=ok")));
			var result = await task;

			Assert.Equal("original", result.Properties["snapshot"]);
			Assert.Equal(1, originalDecoder.CallCount);
			Assert.Equal(0, replacementDecoder.CallCount);
		}

		[Fact]
		public void BeginRejectsASecondValidRequest()
		{
			_ = Begin();

			Assert.Throws<InvalidOperationException>(() => Begin());
		}

		[Fact]
		public async Task InvalidSecondCallKeepsValidationPrecedence()
		{
			var firstTransport = CreateTransport();
			var firstTask = firstTransport.AuthenticateAsync(CreateOptions());
			var secondTransport = CreateTransport();

			var nullUrl = await Assert.ThrowsAsync<ArgumentNullException>(() =>
				secondTransport.AuthenticateAsync(new WebAuthenticatorOptions { CallbackUrl = new Uri("maui-auth://callback") }));
			Assert.Equal("Url", nullUrl.ParamName);

			await Assert.ThrowsAsync<InvalidOperationException>(() => secondTransport.AuthenticateAsync(CreateOptions()));

			Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback?code=ok")));
			await firstTask;
		}

		[Fact]
		public async Task RouteOnlyCallbackCompletesMatchingRequest()
		{
			var request = Begin();

			Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("MAUI-AUTH://CALLBACK?code=ok#state=kept")));

			var result = await request.Task;
			Assert.Equal("ok", result.Properties["code"]);
		}

		[Fact]
		public async Task RouteOnlyMismatchIsUnhandledAndRemainsPending()
		{
			var request = Begin(callbackUrl: new Uri("maui-auth://callback/complete"));

			Assert.False(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback/wrong?code=bad")));
			Assert.False(request.Task.IsCompleted);
			Assert.False(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("another-auth://callback/complete")));
			Assert.False(request.Task.IsCompleted);

			Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback/complete?code=ok")));
			Assert.Equal("ok", (await request.Task).Properties["code"]);
		}

		[Fact]
		public async Task TerminalCallbackCompletesOnlyMatchingRequest()
		{
			var request = Begin(callbackUrl: new Uri("maui-auth://callback:4567/complete"));

			Assert.True(WebAuthenticatorRequestManager.TryHandleTerminalCallback(
				request,
				new Uri("MAUI-AUTH://CALLBACK:4567/complete?code=ok")));

			Assert.Equal("ok", (await request.Task).Properties["code"]);
		}

		[Fact]
		public async Task TerminalMismatchFaultsTheBoundRequestWithoutSensitiveUris()
		{
			var request = Begin(callbackUrl: new Uri("maui-auth://callback/complete"));

			Assert.True(WebAuthenticatorRequestManager.TryHandleTerminalCallback(
				request,
				new Uri("maui-auth://callback/wrong?code=secret")));

			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => request.Task);
			Assert.DoesNotContain("secret", exception.Message, StringComparison.Ordinal);
			Assert.DoesNotContain("maui-auth", exception.Message, StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public async Task TerminalCallbackWithoutUriFaultsTheBoundRequest()
		{
			var request = Begin();

			Assert.True(WebAuthenticatorRequestManager.TryHandleTerminalCallback(request, null));

			await Assert.ThrowsAsync<InvalidOperationException>(() => request.Task);
		}

		[Fact]
		public async Task DuplicateRouteCallbackIsConsumedWithoutRepeatingCompletion()
		{
			var decoder = new CallbackDecoder(_ => new Dictionary<string, string>());
			var beforeCompletionCalls = 0;
			var request = Begin(decoder: decoder, beforeCallbackCompletion: () => beforeCompletionCalls++);

			Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback?code=first")));
			Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback?code=second")));
			await request.Task;

			Assert.Equal(1, decoder.CallCount);
			Assert.Equal(1, beforeCompletionCalls);
		}

		[Fact]
		public void CallbackWithoutRequestIsNotHandled()
		{
			Assert.False(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback")));
		}

		[Fact]
		public async Task LateReferenceAndIdCannotCompleteSuccessor()
		{
			var first = Begin();
			Assert.True(WebAuthenticatorRequestManager.TryCancelFromPlatform(first));
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first.Task);
			Assert.True(WebAuthenticatorRequestManager.End(first));

			var second = Begin();
			Assert.False(WebAuthenticatorRequestManager.TryHandleTerminalCallback(first, new Uri("maui-auth://callback?code=late")));
			Assert.False(WebAuthenticatorRequestManager.TryHandleTerminalCallback(first.Id, new Uri("maui-auth://callback?code=late")));
			Assert.False(WebAuthenticatorRequestManager.TryCancelFromPlatform(first.Id));
			Assert.False(WebAuthenticatorRequestManager.TryFail(first.Id, new InvalidOperationException("late")));
			Assert.False(second.Task.IsCompleted);

			Assert.True(WebAuthenticatorRequestManager.TryHandleTerminalCallback(second.Id, new Uri("maui-auth://callback?code=current")));
			Assert.Equal("current", (await second.Task).Properties["code"]);
		}

		[Fact]
		public async Task CancelByIdIsIdentitySafe()
		{
			var request = Begin();

			Assert.False(WebAuthenticatorRequestManager.TryCancelFromPlatform(request.Id + 1));
			Assert.False(request.Task.IsCompleted);
			Assert.True(WebAuthenticatorRequestManager.TryCancelFromPlatform(request.Id));
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request.Task);
		}

		[Fact]
		public async Task FailByIdIsIdentitySafe()
		{
			var expected = new InvalidOperationException("redacted failure");
			var request = Begin();

			Assert.False(WebAuthenticatorRequestManager.TryFail(request.Id + 1, expected));
			Assert.False(request.Task.IsCompleted);
			Assert.True(WebAuthenticatorRequestManager.TryFail(request.Id, expected));

			var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => request.Task);
			Assert.Same(expected, actual);
		}

		[Fact]
		public async Task CallerCancellationPreservesOriginalToken()
		{
			using var cancellationSource = new CancellationTokenSource();
			var request = Begin(cancellationToken: cancellationSource.Token);

			Assert.True(WebAuthenticatorRequestManager.TryCancelFromCaller(request));
			var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request.Task);

			Assert.Equal(cancellationSource.Token, exception.CancellationToken);
			Assert.Equal(TaskStatus.Canceled, request.Task.Status);
		}

		[Fact]
		public async Task CallerCancellationConsumesOnlyMatchingDuplicateCallback()
		{
			using var cancellationSource = new CancellationTokenSource();
			var decoder = new CallbackDecoder(_ => new Dictionary<string, string>());
			var request = Begin(decoder: decoder, cancellationToken: cancellationSource.Token);

			Assert.True(WebAuthenticatorRequestManager.TryCancelFromCaller(request));
			Assert.False(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://other?code=ignored")));
			Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback?code=duplicate")));

			var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request.Task);
			Assert.Equal(cancellationSource.Token, exception.CancellationToken);
			Assert.Equal(0, decoder.CallCount);
		}

		[Fact]
		public async Task FailureConsumesOnlyMatchingDuplicateCallback()
		{
			var expected = new InvalidOperationException("redacted failure");
			var decoder = new CallbackDecoder(_ => new Dictionary<string, string>());
			var request = Begin(decoder: decoder);

			Assert.True(WebAuthenticatorRequestManager.TryFail(request, expected));
			Assert.False(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://other?code=ignored")));
			Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback?code=duplicate")));

			var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => request.Task);
			Assert.Same(expected, actual);
			Assert.Equal(0, decoder.CallCount);
		}

		[Fact]
		public async Task DecoderRunsOnceAndItsExceptionIsPropagated()
		{
			var expected = new DecoderException();
			var decoder = new CallbackDecoder(_ => throw expected);
			var request = Begin(decoder: decoder);

			Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback?code=sensitive")));
			var actual = await Assert.ThrowsAsync<DecoderException>(() => request.Task);

			Assert.Same(expected, actual);
			Assert.Equal(1, decoder.CallCount);
			Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback?code=duplicate")));
			Assert.Equal(1, decoder.CallCount);
		}

		[Fact]
		public async Task PlatformDelegateRunsBeforeDecoderAndOutsideTheManagerLock()
		{
			var order = new List<string>();
			var lockWasAvailable = false;
			WebAuthenticatorRequest? request = null;
			var decoder = new CallbackDecoder(_ =>
			{
				order.Add("decoder");
				return new Dictionary<string, string>();
			});

			request = Begin(
				decoder: decoder,
				beforeCallbackCompletion: () =>
				{
					order.Add("platform");
					lockWasAvailable = Task.Run(() => WebAuthenticatorRequestManager.End(request!)).Wait(TimeSpan.FromSeconds(5));
				});

			Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback")));
			await request.Task;

			Assert.True(lockWasAvailable);
			Assert.Equal(new[] { "platform", "decoder" }, order);
		}

		[Fact]
		public async Task DecoderRunsOutsideTheManagerLock()
		{
			var lockWasAvailable = false;
			WebAuthenticatorRequest? request = null;
			var decoder = new CallbackDecoder(_ =>
			{
				lockWasAvailable = Task.Run(() => WebAuthenticatorRequestManager.End(request!)).Wait(TimeSpan.FromSeconds(5));
				return new Dictionary<string, string>();
			});
			request = Begin(decoder: decoder);

			Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback")));
			await request.Task;

			Assert.True(lockWasAvailable);
		}

		[Fact]
		public async Task ContinuationsDoNotRunInlineWithCompletion()
		{
			var request = Begin();
			using var continuationStarted = new ManualResetEventSlim();
			using var releaseContinuation = new ManualResetEventSlim();
			var continuation = request.Task.ContinueWith(
				_ =>
				{
					continuationStarted.Set();
					releaseContinuation.Wait(TimeSpan.FromSeconds(5));
				},
				CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default);

			try
			{
				var completion = Task.Run(() => WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback")));
				Assert.True(await completion.WaitAsync(TimeSpan.FromSeconds(5)));
				Assert.True(continuationStarted.Wait(TimeSpan.FromSeconds(5)));
			}
			finally
			{
				releaseContinuation.Set();
			}

			await continuation.WaitAsync(TimeSpan.FromSeconds(5));
		}

		[Fact]
		public async Task CallbackCancelAndFailRacesHaveOneTerminalOutcome()
		{
			for (var iteration = 0; iteration < 100; iteration++)
			{
				var decoder = new CallbackDecoder(_ => new Dictionary<string, string>());
				var request = Begin(decoder: decoder);

				await Task.WhenAll(
					Task.Run(() => WebAuthenticatorRequestManager.TryHandleTerminalCallback(request, new Uri("maui-auth://callback"))),
					Task.Run(() => WebAuthenticatorRequestManager.TryCancelFromPlatform(request)),
					Task.Run(() => WebAuthenticatorRequestManager.TryFail(request, new InvalidOperationException("race"))));

				try
				{
					await request.Task;
				}
				catch (OperationCanceledException)
				{
				}
				catch (InvalidOperationException)
				{
				}

				Assert.True(request.Task.IsCompleted);
				Assert.InRange(decoder.CallCount, 0, 1);
				Assert.True(WebAuthenticatorRequestManager.End(request));
			}
		}

		[Fact]
		public async Task EndIsIdentitySafeAndKeepsConcurrencyClosedUntilCleanup()
		{
			var first = Begin();
			Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback")));
			await first.Task;

			Assert.Throws<InvalidOperationException>(() => Begin());
			Assert.True(WebAuthenticatorRequestManager.End(first));

			var second = Begin();
			Assert.False(WebAuthenticatorRequestManager.End(first));
			Assert.True(WebAuthenticatorRequestManager.TryCancelFromPlatform(second));
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => second.Task);
		}

		[Fact]
		public async Task ActiveBuiltInRequestSurvivesFacadeReplacement()
		{
			var request = Begin();
			var custom = CreateTransport();

			try
			{
				WebAuthenticator.SetDefault(custom);
				Assert.Same(custom, WebAuthenticator.Default);
				Assert.True(WebAuthenticatorRequestManager.TryHandleCallback(new Uri("maui-auth://callback")));
				await request.Task;
			}
			finally
			{
				WebAuthenticator.SetDefault(null);
			}

			Assert.True(request.Task.IsCompletedSuccessfully);
		}

		public void Dispose()
		{
			for (var index = requests.Count - 1; index >= 0; index--)
				WebAuthenticatorRequestManager.End(requests[index]);
		}

		WebAuthenticatorRequest Begin(
			Uri? callbackUrl = null,
			IWebAuthenticatorResponseDecoder? decoder = null,
			CancellationToken cancellationToken = default,
			Action? beforeCallbackCompletion = null)
		{
			var request = WebAuthenticatorRequestManager.Begin(
				new Uri("https://example.com/authorize"),
				callbackUrl ?? new Uri("maui-auth://callback"),
				decoder,
				false,
				cancellationToken,
				beforeCallbackCompletion);

			Track(request);
			return request;
		}

		FakeWebAuthenticator CreateTransport() => new(Track);

		void Track(WebAuthenticatorRequest request) => requests.Add(request);

		static WebAuthenticatorOptions CreateOptions() => new()
		{
			Url = new Uri("https://example.com/authorize"),
			CallbackUrl = new Uri("maui-auth://callback"),
		};

		sealed class FakeWebAuthenticator : IWebAuthenticator
		{
			readonly Action<WebAuthenticatorRequest> trackRequest;

			internal FakeWebAuthenticator(Action<WebAuthenticatorRequest> trackRequest) =>
				this.trackRequest = trackRequest;

			internal Action? AfterSnapshot { get; set; }

			internal int LaunchCount { get; private set; }

			internal WebAuthenticatorRequest? Request { get; private set; }

			public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions) =>
				AuthenticateAsync(webAuthenticatorOptions, CancellationToken.None);

			public async Task<WebAuthenticatorResult> AuthenticateAsync(
				WebAuthenticatorOptions webAuthenticatorOptions,
				CancellationToken cancellationToken)
			{
				if (webAuthenticatorOptions is null)
					throw new ArgumentNullException(nameof(webAuthenticatorOptions));

				var url = webAuthenticatorOptions.Url;
				var callbackUrl = webAuthenticatorOptions.CallbackUrl;
				var responseDecoder = webAuthenticatorOptions.ResponseDecoder;
				var prefersEphemeral = webAuthenticatorOptions.PrefersEphemeralWebBrowserSession;

				AfterSnapshot?.Invoke();

				if (url is null)
					throw new ArgumentNullException(nameof(webAuthenticatorOptions.Url));
				if (callbackUrl is null)
					throw new ArgumentNullException(nameof(webAuthenticatorOptions.CallbackUrl));
				if (!url.IsAbsoluteUri)
					throw new ArgumentException("The authentication URL must be absolute.", nameof(webAuthenticatorOptions.Url));
				if (!callbackUrl.IsAbsoluteUri)
					throw new ArgumentException("The callback URL must be absolute.", nameof(webAuthenticatorOptions.CallbackUrl));

				cancellationToken.ThrowIfCancellationRequested();

				var request = WebAuthenticatorRequestManager.Begin(
					url,
					callbackUrl,
					responseDecoder,
					prefersEphemeral,
					cancellationToken);
				Request = request;
				trackRequest(request);

				var registration = cancellationToken.Register(
					static state =>
					{
						var request = (WebAuthenticatorRequest)state!;
						WebAuthenticatorRequestManager.TryCancelFromCaller(request);
					},
					request);

				try
				{
					if (!request.Task.IsCompleted)
						LaunchCount++;

					return await request.Task.ConfigureAwait(false);
				}
				finally
				{
					registration.Dispose();
					WebAuthenticatorRequestManager.End(request);
				}
			}
		}

		sealed class CallbackDecoder : IWebAuthenticatorResponseDecoder
		{
			readonly Func<Uri, IDictionary<string, string>> callback;

			internal CallbackDecoder(Func<Uri, IDictionary<string, string>> callback) =>
				this.callback = callback;

			internal int CallCount { get; private set; }

			public IDictionary<string, string> DecodeResponse(Uri uri)
			{
				CallCount++;
				return callback(uri);
			}
		}

		sealed class DecoderException : Exception
		{
		}
	}
}
