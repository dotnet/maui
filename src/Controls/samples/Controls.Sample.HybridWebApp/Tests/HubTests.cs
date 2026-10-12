using System.Text.Json;
using Maui.Controls.Sample.HybridWebApp.Playground;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class HubTests
{
	readonly string _session = Guid.NewGuid().ToString("D");

	string Wire(string command, object args, string? session = null, string? id = null) =>
		// Test-only anonymous serialization: the runtime always uses source-generated metadata.
		JsonSerializer.Serialize(new { version = 1, session = session ?? _session, id = id ?? Guid.NewGuid().ToString("D"), command, args });

	async Task<Response> Call(HybridAppTargetObject hub, string command, object args, string? session = null, string? id = null) =>
		JsonSerializer.Deserialize(await hub.Invoke(Wire(command, args, session, id)), PlaygroundJsonContext.Default.Response)!;

	async Task Connect(HybridAppTargetObject hub, string? session = null) =>
		Assert.Equal("success", (await Call(hub, "core.connect", new { }, session)).Status);

	static HybridAppTargetObject CreateHub(IPlaygroundPlatform platform, Action<NativeEvent> send, TimeProvider? time = null)
	{
		var registry = new CommandRegistry();
		EssentialsProvider.Register(registry, platform);
		new SampleCommands().Register(registry);
		return new HybridAppTargetObject(registry,
			() => new ReadyResult(registry.Names, EssentialsProvider.PreferenceNamespace, EssentialsProvider.MaximumTextLength),
			send, time);
	}

	[Fact]
	public async Task UnicodeEscapesNullArraysAndAsyncDtoUseGeneratedMetadata()
	{
		using var hub = CreateHub(new FakePlatform(), _ => { });
		await Connect(hub);
		var value = "你好 👋 café\n\"quotes\"\\\0";
		var response = await Call(hub, "sample.echo", new { text = value, number = -4.25, flag = true, items = new[] { "é", "\\", "\n" } });
		Assert.Equal("success", response.Status);
		var dto = response.Result!.Value.Deserialize(PlaygroundJsonContext.Default.EchoResult)!;
		Assert.Equal(value, dto.Text);
		Assert.Equal(-4.25, dto.Number);
		Assert.True(dto.Flag);
		Assert.Equal(new[] { "é", "\\", "\n" }, dto.Items);
		Assert.Equal(1, dto.Counter);
		response = await Call(hub, "sample.echo", new { text = (string?)null, number = 0, flag = false, items = Array.Empty<string>() });
		Assert.Null(response.Result!.Value.Deserialize(PlaygroundJsonContext.Default.EchoResult)!.Text);
	}

	[Theory]
	[InlineData("sample.notRegistered", "unknown_command")]
	[InlineData("sample.disabled", "disabled_command")]
	[InlineData("sample.fail", "intentional_error")]
	[InlineData("sample.echo", "invalid_arguments")]
	public async Task FailuresAreDistinctAndHubRecovers(string command, string code)
	{
		using var hub = CreateHub(new FakePlatform(), _ => { });
		await Connect(hub);
		var response = await Call(hub, command, new { });
		Assert.Equal("error", response.Status);
		Assert.Equal(code, response.Error!.Code);
		Assert.Null(response.Result);
		Assert.Equal("success", (await Call(hub, "sample.counter", new { })).Status);
	}

	[Theory]
	[InlineData("{\"text\":null,\"number\":0,\"flag\":true,\"items\":[null]}")]
	[InlineData("{\"text\":null,\"number\":0,\"flag\":true,\"items\":null}")]
	[InlineData("{\"text\":null,\"number\":0,\"flag\":true,\"items\":[],\"extra\":true}")]
	[InlineData("null")]
	public async Task InvalidDtoArgumentsAreRejected(string arguments)
	{
		using var hub = CreateHub(new FakePlatform(), _ => { });
		await Connect(hub);
		using var document = JsonDocument.Parse(arguments);
		var response = await Call(hub, "sample.echo", document.RootElement);
		Assert.Equal("invalid_arguments", response.Error!.Code);
	}

	[Theory]
	[InlineData("not JSON")]
	[InlineData("{}")]
	[InlineData("{\"version\":1,\"session\":\"bad\",\"id\":\"bad\",\"command\":\"c\",\"args\":{}}")]
	[InlineData("你好")]
	public async Task MalformedProtocolIsRejected(string request)
	{
		using var hub = CreateHub(new FakePlatform(), _ => { });
		var response = JsonSerializer.Deserialize(await hub.Invoke(request), PlaygroundJsonContext.Default.Response)!;
		Assert.Equal("error", response.Status);
		Assert.Equal("invalid_request", response.Error!.Code);
	}

	[Fact]
	public async Task NamespaceAndSizeBoundsAreEnforcedBeforePlatformCalls()
	{
		var platform = new FakePlatform();
		using var hub = CreateHub(platform, _ => { });
		await Connect(hub);
		foreach (var key in new[] { "", "../other", "a.b", new string('a', 49), "你好" })
			Assert.Equal("invalid_arguments", (await Call(hub, "preferences.read", new { key })).Error!.Code);
		Assert.Equal("invalid_arguments", (await Call(hub, "clipboard.write", new { text = new string('x', 4097) })).Error!.Code);
		Assert.Equal(0, platform.Calls);
		Assert.Equal("success", (await Call(hub, "preferences.write", new { key = "example", value = "你好" })).Status);
		Assert.Equal("你好", (await Call(hub, "preferences.read", new { key = "example" })).Result!.Value.GetProperty("value").GetString());
		Assert.Equal("success", (await Call(hub, "preferences.remove", new { key = "example" })).Status);
		Assert.Equal(JsonValueKind.Null, (await Call(hub, "preferences.read", new { key = "example" })).Result!.Value.GetProperty("value").ValueKind);
	}

	[Fact]
	public async Task CancellationHasTerminalCancelledStatusAndCorrelatedProgress()
	{
		var events = new List<NativeEvent>();
		var progressed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		using var hub = CreateHub(new FakePlatform(), value =>
		{
			lock (events)
				events.Add(value);
			progressed.TrySetResult();
		});
		await Connect(hub);
		await Call(hub, "core.subscribe", new { topic = "operation.progress" });
		var id = Guid.NewGuid().ToString("D");
		var running = Call(hub, "sample.work", new { steps = 20, delayMs = 100 }, id: id);
		await progressed.Task.WaitAsync(TimeSpan.FromSeconds(5));
		Assert.True((await Call(hub, "core.cancel", new { requestId = id })).Result!.Value.GetProperty("accepted").GetBoolean());
		var response = await running;
		Assert.Equal("cancelled", response.Status);
		Assert.Null(response.Result);
		lock (events)
		{
			Assert.NotEmpty(events);
			Assert.All(events, value => { Assert.Equal(id, value.Id); Assert.Equal(_session, value.Session); });
		}
	}

	[Fact]
	public async Task NewDocumentCancelsOldWorkAndOldSubscriptionsButPreservesService()
	{
		var events = new List<NativeEvent>();
		using var hub = CreateHub(new FakePlatform(), value => events.Add(value));
		await Connect(hub);
		await Call(hub, "core.subscribe", new { topic = "service.changed" });
		await Call(hub, "sample.echo", new { text = (string?)null, number = 0, flag = false, items = Array.Empty<string>() });
		Assert.Single(events);
		var running = Call(hub, "sample.work", new { steps = 20, delayMs = 100 });
		var next = Guid.NewGuid().ToString("D");
		await Connect(hub, next);
		Assert.Equal("cancelled", (await running).Status);
		Assert.Equal("stale_session", (await Call(hub, "sample.counter", new { })).Error!.Code);
		Assert.Equal(1, (await Call(hub, "sample.counter", new { }, next)).Result!.Value.GetProperty("counter").GetInt32());
		await Call(hub, "sample.echo", new { text = "new", number = 0, flag = false, items = Array.Empty<string>() }, next);
		Assert.Single(events);
		hub.ReceiveDocumentMessage(JsonSerializer.Serialize(new DocumentMessage(_session, "disconnect"), PlaygroundJsonContext.Default.DocumentMessage));
		Assert.True(hub.IsCurrent(next));
	}

	[Fact]
	public async Task ReplayedConnectCannotReplaceANewerDocument()
	{
		using var hub = CreateHub(new FakePlatform(), _ => { });
		var originalConnect = Wire("core.connect", new { });
		Assert.Equal("success", JsonSerializer.Deserialize(await hub.Invoke(originalConnect), PlaygroundJsonContext.Default.Response)!.Status);
		var next = Guid.NewGuid().ToString("D");
		await Connect(hub, next);
		var running = Call(hub, "sample.work", new { steps = 1, delayMs = 100 }, next);
		var replay = JsonSerializer.Deserialize(await hub.Invoke(originalConnect), PlaygroundJsonContext.Default.Response)!;
		Assert.Equal("stale_session", replay.Error!.Code);
		Assert.True(hub.IsCurrent(next));
		Assert.Equal("success", (await running).Status);
		Assert.Equal("stale_session", (await Call(hub, "core.connect", new { })).Error!.Code);
	}

	[Fact]
	public async Task DisconnectedDocumentMustUseANewIdentityToReconnect()
	{
		using var hub = CreateHub(new FakePlatform(), _ => { });
		await Connect(hub);
		hub.Disconnect();
		Assert.Equal("stale_session", (await Call(hub, "core.connect", new { })).Error!.Code);
		await Connect(hub, Guid.NewGuid().ToString("D"));
	}

	[Fact]
	public async Task UnsubscribeAndDisposeReleaseDocumentWork()
	{
		var events = new List<NativeEvent>();
		var hub = CreateHub(new FakePlatform(), value => events.Add(value));
		await Connect(hub);
		await Call(hub, "core.subscribe", new { topic = "service.changed" });
		await Call(hub, "core.unsubscribe", new { topic = "service.changed" });
		await Call(hub, "sample.echo", new { text = "no event", number = 0, flag = false, items = Array.Empty<string>() });
		Assert.Empty(events);
		var running = Call(hub, "sample.work", new { steps = 20, delayMs = 100 });
		hub.Dispose();
		Assert.Equal("cancelled", (await running).Status);
		Assert.False(hub.IsCurrent(_session));
		Assert.Equal("disconnected", (await Call(hub, "sample.counter", new { })).Error!.Code);
		hub.Dispose();
	}

	[Fact]
	public async Task LeaseExpiresAndHeartbeatKeepsDocumentAlive()
	{
		var time = new FakeTime();
		using var hub = CreateHub(new FakePlatform(), _ => { }, time);
		await Connect(hub);
		time.Now += TimeSpan.FromSeconds(14);
		hub.ReceiveDocumentMessage(JsonSerializer.Serialize(new DocumentMessage(_session, "heartbeat"), PlaygroundJsonContext.Default.DocumentMessage));
		time.Now += TimeSpan.FromSeconds(14);
		hub.ExpireDocument();
		Assert.True(hub.IsCurrent(_session));
		var running = Call(hub, "sample.work", new { steps = 20, delayMs = 100 });
		time.Now += TimeSpan.FromSeconds(16);
		hub.ExpireDocument();
		Assert.False(hub.IsCurrent(_session));
		Assert.Equal("cancelled", (await running).Status);
	}

	[Fact]
	public async Task DuplicateActiveRequestAndSuccessfulWorkAreHandled()
	{
		using var hub = CreateHub(new FakePlatform(), _ => { });
		await Connect(hub);
		var id = Guid.NewGuid().ToString("D");
		var running = Call(hub, "sample.work", new { steps = 1, delayMs = 100 }, id: id);
		Assert.Equal("duplicate_request", (await Call(hub, "sample.work", new { steps = 1, delayMs = 100 }, id: id)).Error!.Code);
		Assert.Equal("success", (await running).Status);
		Assert.Equal("duplicate_request", (await Call(hub, "sample.counter", new { }, id: id)).Error!.Code);
	}

	[Fact]
	public async Task UnsupportedPlatformApiIsNotReportedAsSuccess()
	{
		using var hub = CreateHub(new FakePlatform(), _ => { });
		await Connect(hub);
		var response = await Call(hub, "app.info", new { });
		Assert.Equal("error", response.Status);
		Assert.Equal("unsupported", response.Error!.Code);
		Assert.Null(response.Result);
	}

	[Fact]
	public async Task ConcurrentWorkIsBoundedAndDisconnectCancelsEveryCall()
	{
		using var hub = CreateHub(new FakePlatform(), _ => { });
		await Connect(hub);
		var running = Enumerable.Range(0, 16).Select(_ => Call(hub, "sample.work", new { steps = 20, delayMs = 100 })).ToArray();
		Assert.Equal("busy", (await Call(hub, "sample.counter", new { })).Error!.Code);
		hub.Disconnect();
		Assert.All(await Task.WhenAll(running), response => Assert.Equal("cancelled", response.Status));
		var next = Guid.NewGuid().ToString("D");
		await Connect(hub, next);
		Assert.Equal("success", (await Call(hub, "sample.counter", new { }, next)).Status);
	}

	[Fact]
	public async Task ProvidersExtendRegistryWithoutCentralRouterChanges()
	{
		var registry = new CommandRegistry();
		var json = PlaygroundJsonContext.Default;
		registry.Register("test.explicit", json.TextArgs, json.TextResult, (args, _) => Task.FromResult(new TextResult(args.Text)));
		Assert.Throws<InvalidOperationException>(() => registry.Register("test.explicit", json.TextArgs, json.TextResult, (args, _) => Task.FromResult(new TextResult(args.Text))));
		var result = await registry.DispatchAsync("test.explicit", JsonSerializer.SerializeToElement(new TextArgs("test"), json.TextArgs),
			new Operation(_session, Guid.NewGuid().ToString("D"), CancellationToken.None, (_, _) => { }));
		Assert.Equal("test", result.Deserialize(json.TextResult)!.Text);
	}

	[Fact]
	public async Task HostChoosesProvidersMetadataAndTopicsWithoutMauiUi()
	{
		var registry = new CommandRegistry();
		var json = PlaygroundJsonContext.Default;
		var events = new List<NativeEvent>();
		registry.RegisterTopic("custom.changed");
		Assert.Throws<InvalidOperationException>(() => registry.RegisterTopic("custom.changed"));
		registry.Register("custom.echo", json.TextArgs, json.TextResult, (args, op) =>
		{
			var result = new TextResult(args.Text);
			op.Publish("custom.changed", JsonSerializer.SerializeToElement(result, json.TextResult));
			return Task.FromResult(result);
		});
		using var hub = new HybridAppTargetObject(registry, () => new ReadyResult(registry.Names, "native-host.", 128),
			events.Add);
		var response = await Call(hub, "core.connect", new { });
		var ready = response.Result!.Value.Deserialize(json.ReadyResult)!;
		Assert.Equal("native-host.", ready.PreferenceNamespace);
		Assert.Equal(128, ready.MaximumTextLength);
		Assert.Contains("custom.echo", ready.Commands);
		Assert.DoesNotContain("app.info", ready.Commands);
		Assert.DoesNotContain("sample.echo", ready.Commands);
		Assert.Equal("unknown_command", (await Call(hub, "clipboard.read", new { })).Error!.Code);
		Assert.Equal("unknown_topic", (await Call(hub, "core.subscribe", new { topic = "service.changed" })).Error!.Code);
		Assert.Equal("success", (await Call(hub, "core.subscribe", new { topic = "custom.changed" })).Status);
		Assert.Equal("success", (await Call(hub, "custom.echo", new { text = "native-only host" })).Status);
		Assert.Equal("native-only host", Assert.Single(events).Data.GetProperty("text").GetString());
	}

	sealed class FakeTime : TimeProvider
	{
		public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
		public override DateTimeOffset GetUtcNow() => Now;
	}

	sealed class FakePlatform : IPlaygroundPlatform
	{
		readonly Dictionary<string, string> _preferences = new();
		string? _clipboard;
		public int Calls { get; private set; }
		public Task<IdentityResult> GetIdentityAsync(CancellationToken cancellation) => throw new NotSupportedException("Tests do not invent native device identity.");
		public Task<TextResult> GetClipboardAsync(CancellationToken cancellation) { Calls++; return Task.FromResult(new TextResult(_clipboard)); }
		public Task<Ack> SetClipboardAsync(string text, CancellationToken cancellation) { Calls++; _clipboard = text; return Task.FromResult(new Ack(true)); }
		public Task<PreferenceResult> GetPreferenceAsync(string key, CancellationToken cancellation) { Calls++; return Task.FromResult(new PreferenceResult(key, _preferences.GetValueOrDefault(key))); }
		public Task<PreferenceResult> SetPreferenceAsync(string key, string? value, CancellationToken cancellation)
		{
			Calls++;
			if (value is null)
				_preferences.Remove(key);
			else
				_preferences[key] = value;
			return Task.FromResult(new PreferenceResult(key, value));
		}
	}
}
