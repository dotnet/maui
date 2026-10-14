using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	public class HybridWebViewBinaryTests
	{
		[Theory]
		[InlineData(0)]
		[InlineData(256)]
		[InlineData(150000)]
		[InlineData(HybridWebViewBinaryInvocationManager.MaximumPayloadLength)]
		public async Task RawAndFragmentedInvocationsPreserveEveryByte(int length)
		{
			var bytes = Enumerable.Range(0, length).Select(i => (byte)i).ToArray();
			foreach (var fragmented in new[] { false, true })
			{
				var invoker = new Invoker((request, token) =>
				{
					Assert.Equal("echo.bytes", request.MethodName);
					Assert.Equal(bytes, request.Payload);
					Assert.Equal("value", request.Headers["test"]);
					return Task.FromResult(request.Payload);
				});
				var manager = new HybridWebViewBinaryInvocationManager(invoker);
				var session = manager.Session;
				var headers = new Dictionary<string, string> { ["test"] = "value" };
				manager.Begin(session, 1, "echo.bytes", length, headers, fragmented);
				headers["test"] = "mutated";
				if (fragmented)
					Upload(manager, session, 1, bytes);
				using var body = fragmented ? null : new MemoryStream(bytes);
				Assert.Equal(bytes, await manager.InvokeAsync(session, 1, body, default));
				Assert.Throws<InvalidOperationException>(() => manager.Begin(session, 1, "echo", 0, null, fragmented));
				manager.Reset(null);
			}
		}

		[Fact]
		public async Task SuccessfulInvocationDoesNotCancelTheHostToken()
		{
			var cancelled = false;
			CancellationToken hostToken = default;
			CancellationTokenRegistration registration = default;
			var invoker = new Invoker((request, token) =>
			{
				hostToken = token;
				registration = token.Register(() => cancelled = true);
				return Task.FromResult(request.Payload);
			});
			var manager = new HybridWebViewBinaryInvocationManager(invoker);
			var session = manager.Session;
			manager.Begin(session, 1, "echo", 1, null, false);
			try
			{
				using var body = new MemoryStream(new byte[] { 42 });
				Assert.Equal(new byte[] { 42 }, await manager.InvokeAsync(
					session, 1, body, default));
				Assert.False(cancelled);
				Assert.False(hostToken.IsCancellationRequested);
				manager.Reset(null);
				Assert.False(cancelled);
			}
			finally
			{
				registration.Dispose();
				manager.Reset(null);
			}
		}

		[Fact]
		public async Task LimitsAndMalformedUploadsNeverInvokeHost()
		{
			var invoker = new Invoker();
			var manager = new HybridWebViewBinaryInvocationManager(invoker);
			var session = manager.Session;
			Assert.Throws<ArgumentOutOfRangeException>(() => manager.Begin(session, 1, "echo", -1, null, true));
			Assert.Throws<ArgumentOutOfRangeException>(() => manager.Begin(session, 1, "echo", 4194305, null, true));
			manager.Begin(session, 1, "echo", 1, null, true);
			Assert.Throws<InvalidOperationException>(() => manager.Append(session, 1, 1, "AA=="));
			manager.Begin(session, 2, "echo", 1, null, true);
			Assert.Throws<FormatException>(() => manager.Append(session, 2, 0, "???"));
			manager.Begin(session, 3, "echo", 1, null, true);
			Assert.Throws<InvalidOperationException>(() => manager.Append(session, 3, 0, "AAA="));
			manager.Begin(session, 4, "echo", 1, null, true);
			await Assert.ThrowsAsync<InvalidOperationException>(() => manager.InvokeAsync(session, 4, null, default));
			manager.Begin(session, 5, "echo", 1, null, true);
			manager.Append(session, 5, 0, "AA==");
			Assert.Throws<InvalidOperationException>(() => manager.Append(session, 5, 0, "AA=="));
			manager.Begin(session, 6, "echo", 1, null, true);
			Assert.Throws<InvalidOperationException>(() => manager.Append(session, 6, 0, new string('A', 16385)));
			Assert.Equal(0, invoker.Calls);
			manager.Reset(null);
		}

		[Fact]
		public async Task RawBodiesMustExactlyMatchDeclaredLengthAndResultsAreBounded()
		{
			var invoker = new Invoker();
			var manager = new HybridWebViewBinaryInvocationManager(invoker);
			var session = manager.Session;
			manager.Begin(session, 1, "echo", 2, null, false);
			await Assert.ThrowsAsync<InvalidOperationException>(() => manager.InvokeAsync(session, 1, new MemoryStream(new byte[1]), default));
			manager.Begin(session, 2, "echo", 1, null, false);
			await Assert.ThrowsAsync<InvalidOperationException>(() => manager.InvokeAsync(session, 2, new MemoryStream(new byte[2]), default));
			Assert.Equal(0, invoker.Calls);
			manager.Reset(new Invoker((request, token) => Task.FromResult(new byte[4194305])));
			session = manager.Session;
			manager.Begin(session, 1, "echo", 0, null, false);
			await Assert.ThrowsAsync<InvalidOperationException>(() => manager.InvokeAsync(session, 1, null, default));
			manager.Reset(null);
		}

		[Theory]
		[InlineData("invalid name", "good", "valid")]
		[InlineData("good", "bad:name", "valid")]
		[InlineData("good", "good", "invalid\nvalue")]
		[InlineData("good", "good", "😀")]
		public void MetadataIsValidated(string method, string name, string value) =>
			Assert.Throws<ArgumentException>(() => HybridWebViewBinaryInvocationManager.ValidateMetadata(method,
				new Dictionary<string, string> { [name] = value }));

		[Fact]
		public void MetadataCountSizeAndCaseInsensitiveDuplicatesAreRejected()
		{
			Assert.Throws<ArgumentException>(() => HybridWebViewBinaryInvocationManager.ValidateMetadata(new string('a', 129), null));
			Assert.Throws<ArgumentException>(() => HybridWebViewBinaryInvocationManager.ValidateMetadata("ok",
				Enumerable.Range(0, 17).ToDictionary(i => "header" + i, i => "value")));
			Assert.Throws<ArgumentException>(() => HybridWebViewBinaryInvocationManager.ValidateMetadata("ok",
				new Dictionary<string, string> { ["key"] = new string('a', 257) }));
			Assert.Throws<ArgumentException>(() => HybridWebViewBinaryInvocationManager.ValidateMetadata("ok",
				new Dictionary<string, string> { ["key"] = "", ["KEY"] = "" }));
		}

		[Fact]
		public void UnknownIdsStaleSessionsAndDisconnectedManagersFail()
		{
			var invoker = new Invoker();
			var manager = new HybridWebViewBinaryInvocationManager(invoker);
			var other = new HybridWebViewBinaryInvocationManager(invoker);
			var old = manager.Session;
			manager.Begin(old, 1, "echo", 0, null, true);
			Assert.Throws<InvalidOperationException>(() => manager.Cancel(other.Session, 1));
			Assert.Throws<InvalidOperationException>(() => manager.Cancel(old, 2));
			manager.Reset(invoker);
			Assert.Throws<InvalidOperationException>(() => manager.Begin(old, 2, "echo", 0, null, true));
			Assert.Throws<InvalidOperationException>(() => manager.Cancel(manager.Session, 1));
			manager.Reset(null);
			Assert.Throws<InvalidOperationException>(() => manager.Begin(old, 2, "echo", 0, null, true));
			other.Reset(null);
		}

		[Fact]
		public async Task CancellationAndReloadSuppressLateResultsAndRetainRunningSlots()
		{
			var completed = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
			var tokens = new List<CancellationToken>();
			var invoker = new Invoker((request, token) =>
			{
				tokens.Add(token);
				return completed.Task; // Intentionally ignore cancellation to test the concurrency bound.
			});
			var manager = new HybridWebViewBinaryInvocationManager(invoker);
			var session = manager.Session;
			var calls = new List<Task<byte[]>>();
			for (var id = 1; id <= 4; id++)
			{
				manager.Begin(session, id, "echo", 0, null, true);
				calls.Add(manager.InvokeAsync(session, id, null, default));
			}
			manager.Cancel(session, 1);
			Assert.True(tokens[0].IsCancellationRequested);
			Assert.Throws<InvalidOperationException>(() => manager.Begin(session, 5, "echo", 0, null, true));
			manager.Reset(invoker);
			Assert.All(tokens, token => Assert.True(token.IsCancellationRequested));
			Assert.Throws<InvalidOperationException>(() => manager.Begin(manager.Session, 1, "echo", 0, null, true));
			completed.SetResult(new byte[] { 42 });
			foreach (var call in calls)
				await Assert.ThrowsAnyAsync<OperationCanceledException>(() => call);
			manager.Begin(manager.Session, 1, "echo", 0, null, true);
			manager.Cancel(manager.Session, 1);
			manager.Reset(null);
		}

		[Fact]
		public async Task NativeRequestCancellationAndTimeoutCancelTheHost()
		{
			var entered = new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
			var invoker = new Invoker(async (request, token) =>
			{
				entered.TrySetResult(token);
				await Task.Delay(Timeout.Infinite, token);
				return Array.Empty<byte>();
			});
			var manager = new HybridWebViewBinaryInvocationManager(invoker, timeoutMilliseconds: 100);
			var session = manager.Session;
			manager.Begin(session, 1, "echo", 0, null, true);
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => manager.InvokeAsync(session, 1, null, default));
			Assert.True((await entered.Task).IsCancellationRequested);
			manager.Begin(session, 2, "echo", 0, null, true);
			using var cancel = new CancellationTokenSource();
			var pending = manager.InvokeAsync(session, 2, null, cancel.Token);
			cancel.Cancel();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
			manager.Reset(null);
		}

		[Fact]
		public void CanceledUploadsReleaseSlotsAndDoNotDispatch()
		{
			var invoker = new Invoker();
			var manager = new HybridWebViewBinaryInvocationManager(invoker);
			var session = manager.Session;
			for (var id = 1; id <= 4; id++)
				manager.Begin(session, id, "echo", 1, null, true);
			Assert.Throws<InvalidOperationException>(() => manager.Begin(session, 5, "echo", 1, null, true));
			manager.Cancel(session, 1);
			Assert.Throws<InvalidOperationException>(() => manager.Cancel(session, 1));
			manager.Begin(session, 5, "echo", 1, null, true);
			Assert.Equal(0, invoker.Calls);
			manager.Reset(null);
		}

		[Fact]
		public async Task AbandonedUploadsExpireWithoutDispatchAndReleaseAllSlots()
		{
			var invoker = new Invoker();
			var manager = new HybridWebViewBinaryInvocationManager(invoker, timeoutMilliseconds: 50);
			var session = manager.Session;
			for (var id = 1; id <= 4; id++)
				manager.Begin(session, id, "echo", 1, null, true);
			await Task.Delay(200);
			Assert.Throws<InvalidOperationException>(() => manager.Append(session, 1, 0, "AA=="));
			manager.Begin(session, 5, "echo", 0, null, true);
			Assert.Equal(0, invoker.Calls);
			manager.Reset(null);
		}

		[Fact]
		public async Task InvokerChangesImmediatelyInvalidateTheOldBootstrapSession()
		{
			var original = new Invoker();
			var replacement = new Invoker();
			var view = new HybridWebView { BinaryInvoker = original };
			var handler = new TestHandler();
			handler.SetVirtualView(view);
			using var script = handler.GetHybridScriptStream();
			var old = handler.BinaryDocumentSession;
			view.BinaryInvoker = replacement;
			Assert.NotEqual(old, handler.BinaryDocumentSession);
			var headers = Headers(old, 1, "begin");
			headers["X-Maui-Binary-Metadata"] = Uri.EscapeDataString("{\"MethodName\":\"echo\",\"Length\":0}");
			Assert.Equal(1, (await handler.InvokeDotNetBinaryAsync(headers, "POST", true, true))[0]);
			view.BinaryInvoker = null;
			Assert.Equal("", handler.BinaryDocumentSession);
			Assert.Equal(0, original.Calls);
			Assert.Equal(0, replacement.Calls);
		}

		[Fact]
		public void BinaryConfigurationDoesNotConfigureOrRequireTheJsonInvoker()
		{
			var view = new HybridWebView();
			Assert.Null(((IHybridWebViewBinary)view).BinaryInvoker);
			var invoker = new Invoker();
			view.BinaryInvoker = invoker;
			Assert.Same(invoker, ((IHybridWebViewBinary)view).BinaryInvoker);
			Assert.Throws<InvalidOperationException>(() => view.Invoker);
			view.BinaryInvoker = null;
			Assert.Null(((IHybridWebViewBinary)view).BinaryInvoker);
		}

		[Fact]
		public async Task EndpointUsesRawSuccessBytesStructuredFailuresAndLeavesJsonIntact()
		{
			var view = new HybridWebView { BinaryInvoker = new Invoker() };
			view.Invoker = new JsonInvoker();
			var handler = new TestHandler();
			handler.SetVirtualView(view);
			using (var script = handler.GetHybridScriptStream())
			using (var reader = new StreamReader(script))
			{
				Assert.DoesNotContain(HybridWebViewHandler.BinarySessionPlaceholder, reader.ReadToEnd(), StringComparison.Ordinal);
			}
			var headers = Headers(handler.BinaryDocumentSession, 1, "begin");
			headers["X-Maui-Binary-Metadata"] = Uri.EscapeDataString("{\"MethodName\":\"echo\",\"Length\":3}");
			Assert.Equal(new byte[] { 0 }, await handler.InvokeDotNetBinaryAsync(headers, "POST", true, false));
			headers["X-Maui-Binary-Operation"] = "invoke";
			Assert.Equal(new byte[] { 0, 0, 128, 255 }, await handler.InvokeDotNetBinaryAsync(headers, "POST", true, false,
				new MemoryStream(new byte[] { 0, 128, 255 })));
			var failure = await handler.InvokeDotNetBinaryAsync(headers, "POST", true, false);
			Assert.Equal(1, failure[0]);
			Assert.Contains("\"IsError\":true", Encoding.UTF8.GetString(failure, 1, failure.Length - 1), StringComparison.Ordinal);
			Assert.DoesNotContain("\"Result\"", Encoding.UTF8.GetString(failure), StringComparison.Ordinal);
			var json = Encoding.UTF8.GetString(await handler.InvokeDotNetAsync(stringBody: "{\"MethodName\":\"echo\",\"ParamValues\":[\"123\"]}"));
			Assert.Contains("\"IsJson\":true", json, StringComparison.Ordinal);
			Assert.Contains("\"IsError\":false", json, StringComparison.Ordinal);
			Assert.Contains("\"Result\":\"123\"", json, StringComparison.Ordinal);
			handler.ResetBinaryInvocation(true);
		}

		[Theory]
		[InlineData("GET", true, "https://0.0.0.1", "empty", "HybridWebView/1")]
		[InlineData("POST", false, "https://0.0.0.1", "empty", "HybridWebView/1")]
		[InlineData("POST", true, "https://remote.invalid", "empty", "HybridWebView/1")]
		[InlineData("POST", true, "https://0.0.0.1", "iframe", "HybridWebView/1")]
		[InlineData("POST", true, "https://0.0.0.1", "empty", "wrong")]
		public async Task EndpointRejectsUntrustedRequests(string verb, bool trustedDocument, string origin, string destination, string marker)
		{
			var handler = new TestHandler();
			var headers = Headers(new string('a', 32), 1, "begin");
			headers["Origin"] = origin;
			headers["Sec-Fetch-Dest"] = destination;
			headers[HybridWebViewHandler.BinaryTokenHeader] = marker;
			Assert.Equal(1, (await handler.InvokeDotNetBinaryAsync(headers, verb, trustedDocument, true))[0]);
		}

		[Theory]
		[InlineData("{")]
		[InlineData("{\"MethodName\":\"echo\"}")]
		[InlineData("{\"MethodName\":\"echo\",\"Length\":4194305}")]
		[InlineData("{\"MethodName\":\"echo\",\"Length\":0,\"Headers\":{\"tag\":\"a\",\"TAG\":\"b\"}}")]
		public async Task MalformedBeginMetadataFailsBeforeHostDispatch(string metadata)
		{
			var invoker = new Invoker();
			var handler = new TestHandler();
			handler.SetVirtualView(new HybridWebView { BinaryInvoker = invoker });
			using var script = handler.GetHybridScriptStream();
			var headers = Headers(handler.BinaryDocumentSession, 1, "begin");
			headers["X-Maui-Binary-Metadata"] = Uri.EscapeDataString(metadata);
			Assert.Equal(1, (await handler.InvokeDotNetBinaryAsync(headers, "POST", true, true))[0]);
			Assert.Equal(0, invoker.Calls);
			handler.ResetBinaryInvocation(true);
		}

		[Fact]
		public async Task HostDenialIsAStructuredFailureAndIsNeverRetried()
		{
			var invoker = new Invoker((request, token) => throw new UnauthorizedAccessException("Command denied."));
			var handler = new TestHandler();
			handler.SetVirtualView(new HybridWebView { BinaryInvoker = invoker });
			using var script = handler.GetHybridScriptStream();
			var headers = Headers(handler.BinaryDocumentSession, 1, "begin");
			headers["X-Maui-Binary-Metadata"] = Uri.EscapeDataString("{\"MethodName\":\"denied\",\"Length\":0}");
			Assert.Equal(new byte[] { 0 }, await handler.InvokeDotNetBinaryAsync(headers, "POST", true, false));
			headers["X-Maui-Binary-Operation"] = "invoke";
			var response = await handler.InvokeDotNetBinaryAsync(headers, "POST", true, false);
			Assert.Equal(1, response[0]);
			Assert.Contains("\"ErrorType\":\"UnauthorizedAccessException\"", Encoding.UTF8.GetString(response, 1, response.Length - 1), StringComparison.Ordinal);
			Assert.Equal(1, invoker.Calls);
			handler.ResetBinaryInvocation(true);
		}

		[Fact]
		public void AndroidResponseStreamDropsLateBytesAndClearsDisposedResponses()
		{
			var bytes = new byte[] { 0, 42 };
			var current = true;
			using var cancellation = new CancellationTokenSource();
			var stream = new HybridWebViewBinaryResponseStream(Task.FromResult(bytes), cancellation, () => current);
			Assert.Equal(1, stream.Read(new byte[1], 0, 1));
			current = false;
			Assert.Throws<OperationCanceledException>(() => stream.Read(new byte[1], 0, 1));
			Assert.Equal(new byte[2], bytes);
			stream.Dispose();
		}

		static void Upload(HybridWebViewBinaryInvocationManager manager, string session, int id, byte[] bytes)
		{
			for (int offset = 0, index = 0; offset < bytes.Length; offset += HybridWebViewBinaryInvocationManager.FragmentLength, index++)
				manager.Append(session, id, index, Convert.ToBase64String(bytes, offset,
					Math.Min(HybridWebViewBinaryInvocationManager.FragmentLength, bytes.Length - offset)));
		}

		static Dictionary<string, string> Headers(string session, int id, string operation) => new()
		{
			["Origin"] = HybridWebViewHandler.AppOrigin.TrimEnd('/'),
			[HybridWebViewHandler.BinaryTokenHeader] = HybridWebViewHandler.BinaryTokenValue,
			["X-Maui-Binary-Session"] = session,
			["X-Maui-Binary-Id"] = id.ToString(System.Globalization.CultureInfo.InvariantCulture),
			["X-Maui-Binary-Operation"] = operation
		};

		sealed class TestHandler : HybridWebViewHandler
		{
			internal TestHandler() : base(new PropertyMapper<IHybridWebView, IHybridWebViewHandler>
			{
				[nameof(IHybridWebViewBinary.BinaryInvoker)] = MapBinaryInvoker
			})
			{ }
			protected override object CreatePlatformView() => new object();
		}

		sealed class Invoker : HybridWebViewBinaryInvoker
		{
			readonly Func<HybridWebViewBinaryRequest, CancellationToken, Task<byte[]>> _invoke;
			internal int Calls;
			internal Invoker(Func<HybridWebViewBinaryRequest, CancellationToken, Task<byte[]>> invoke = null) =>
				_invoke = invoke ?? ((request, token) => Task.FromResult(request.Payload));
			public override Task<byte[]> InvokeAsync(HybridWebViewBinaryRequest request, CancellationToken cancellationToken)
			{
				Calls++;
				return _invoke(request, cancellationToken);
			}
		}

		sealed class JsonInvoker : HybridWebViewInvoker
		{
			internal JsonInvoker() : base(null, null) { }
			public override Task<string> InvokeMethodAsync(string methodName, string[] paramJsonValues)
			{
				Assert.Equal("echo", methodName);
				Assert.Equal(new[] { "123" }, paramJsonValues);
				return Task.FromResult("123");
			}
		}
	}
}
