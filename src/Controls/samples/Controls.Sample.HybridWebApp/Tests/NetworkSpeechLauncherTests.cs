using System.Text.Json;
using Maui.HybridApp;
using Maui.HybridApp.Essentials.Connectivity;
using Maui.HybridApp.Essentials.Launcher;
using Maui.HybridApp.Essentials.TextToSpeech;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class NetworkSpeechLauncherTests
{
	static readonly NativeDispatcher Inline = new(action => action());
	static async Task<JsonElement> Dispatch(CommandRegistry registry, string command, string args = "{}", CancellationToken cancellation = default)
	{
		using var json = JsonDocument.Parse(args);
		return await registry.DispatchAsync(command, json.RootElement,
			new Operation(Guid.NewGuid().ToString("D"), Guid.NewGuid().ToString("D"), cancellation, (_, _) => { }));
	}
	static async Task<Response> Call(HybridAppTargetObject hub, string session, string command, object args, string? id = null)
	{
		var wire = JsonSerializer.Serialize(new { version = 1, session, id = id ?? Guid.NewGuid().ToString("D"), command, args });
		return JsonSerializer.Deserialize(await hub.Invoke(wire), ProtocolJsonContext.Default.Response)!;
	}

	[Fact]
	public async Task ConnectivityReadsFreshNativeConditionsWithoutStartingAListener()
	{
		var native = new Network();
		using var notifications = new NativeNotifications();
		using var provider = new ConnectivityProvider(native, Inline, notifications);
		var registry = new CommandRegistry();
		provider.Register(registry);
		Assert.True(registry.HasTopic("connectivity.changed"));
		Assert.Equal("Internet", (await Dispatch(registry, "connectivity.info")).GetProperty("networkAccess").GetString());
		native.Access = NetworkAccess.Local;
		native.Profiles = [ConnectionProfile.Ethernet, ConnectionProfile.Ethernet];
		var value = await Dispatch(registry, "connectivity.info");
		Assert.Equal("Local", value.GetProperty("networkAccess").GetString());
		Assert.Equal(new[] { "Ethernet" }, value.GetProperty("connectionProfiles").EnumerateArray().Select(item => item.GetString()));
		Assert.Equal(0, native.Adds);
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "connectivity.info", "{\"probeUrl\":\"https://example.com\"}"))).Code);
	}

	[Fact]
	public async Task NetworkEventsRetireOnUnsubscribeReconnectAndScopeDisposalWithoutAffectingAnotherScope()
	{
		var native = new Network();
		using var notices1 = new NativeNotifications();
		using var notices2 = new NativeNotifications();
		using var provider1 = new ConnectivityProvider(native, Inline, notices1);
		using var provider2 = new ConnectivityProvider(native, Inline, notices2);
		using var hub1 = new HybridAppTargetObject([provider1], new HybridAppOptions("test."), notifications: notices1);
		using var hub2 = new HybridAppTargetObject([provider2], new HybridAppOptions("test."), notifications: notices2);
		var events1 = new List<NativeEvent>();
		var events2 = new List<NativeEvent>();
		hub1.NativeEventReceived += events1.Add;
		hub2.NativeEventReceived += events2.Add;
		var session1 = Guid.NewGuid().ToString("D");
		var session2 = Guid.NewGuid().ToString("D");
		await Call(hub1, session1, "core.connect", new { });
		await Call(hub2, session2, "core.connect", new { });
		Assert.Equal("success", (await Call(hub1, session1, "core.subscribe", new { topic = "connectivity.changed" })).Status);
		await Call(hub2, session2, "core.subscribe", new { topic = "connectivity.changed" });
		Assert.Equal(2, native.Listeners);
		Assert.Empty(events1);
		var retired = native.Handler;
		native.Change(NetworkAccess.None, []);
		Assert.Equal("None", Assert.Single(events1).Data.GetProperty("networkAccess").GetString());
		Assert.Single(events2);
		await Call(hub1, session1, "core.unsubscribe", new { topic = "connectivity.changed" });
		Assert.Equal(1, native.Listeners);
		retired?.Invoke(native, new ConnectivityChangedEventArgs(NetworkAccess.Internet, [ConnectionProfile.WiFi]));
		Assert.Single(events1);
		Assert.Equal(2, events2.Count);
		session1 = Guid.NewGuid().ToString("D");
		await Call(hub1, session1, "core.connect", new { });
		await Call(hub1, session1, "core.subscribe", new { topic = "connectivity.changed" });
		retired?.Invoke(native, new ConnectivityChangedEventArgs(NetworkAccess.Local, []));
		Assert.Single(events1);
		native.Change(NetworkAccess.ConstrainedInternet, [ConnectionProfile.Cellular]);
		Assert.Equal(session1, events1.Last().Session);
		Assert.Equal(2, events1.Count);
		hub1.Dispose();
		Assert.Equal(1, native.Listeners);
		native.Change(NetworkAccess.None, []);
		Assert.Equal(2, events1.Count);
		Assert.Equal(5, events2.Count);
		provider2.Dispose();
		Assert.Equal(0, native.Listeners);
	}

	[Fact]
	public async Task NetworkSubscribePermissionFailureIsExplicitAndCanRetryWithoutMultiplyingHandlers()
	{
		var native = new Network { SubscribeError = new PermissionException("Denied") };
		using var notifications = new NativeNotifications();
		using var provider = new ConnectivityProvider(native, Inline, notifications);
		using var hub = new HybridAppTargetObject([provider], new HybridAppOptions("test."), notifications: notifications);
		var session = Guid.NewGuid().ToString("D");
		await Call(hub, session, "core.connect", new { });
		Assert.Equal("permission_denied", (await Call(hub, session, "core.subscribe", new { topic = "connectivity.changed" })).Error!.Code);
		Assert.Equal(0, native.Listeners);
		native.SubscribeError = null;
		Assert.Equal("success", (await Call(hub, session, "core.subscribe", new { topic = "connectivity.changed" })).Status);
		Assert.Equal(1, native.Listeners);
	}

	[Fact]
	public async Task SpeechLocalesAndOptionsUseTheActualInterfaceWithNativeDefaults()
	{
		var native = new Speech();
		var registry = new CommandRegistry();
		new TextToSpeechProvider(native, Inline, new HybridAppOptions("test.", 32)).Register(registry);
		var locale = Assert.Single((await Dispatch(registry, "speech.locales")).GetProperty("locales").EnumerateArray());
		Assert.Equal("Native voice", locale.GetProperty("name").GetString());
		Assert.Equal("", locale.GetProperty("country").GetString());
		Assert.Equal(0, native.Speaks);
		Assert.True(registry.IsInteractive("speech.speak"));
		Assert.True((await Dispatch(registry, "speech.speak", "{\"text\":\"你好 👋\"}")).GetProperty("accepted").GetBoolean());
		Assert.Null(native.Options);
		await Dispatch(registry, "speech.speak",
			"{\"text\":\"Hello\",\"options\":{\"pitch\":1.5,\"volume\":0.25,\"rate\":0.8,\"locale\":{\"id\":\"native\",\"language\":\"en-US\",\"country\":\"\",\"name\":\"Native voice\"}}}");
		Assert.Equal("Hello", native.Text);
		Assert.Equal(1.5f, native.Options!.Pitch);
		Assert.Equal(0.25f, native.Options.Volume);
		Assert.Equal(0.8f, native.Options.Rate);
		Assert.Equal("native", native.Options.Locale!.Id);
	}

	[Theory]
	[InlineData("{}")]
	[InlineData("{\"text\":null}")]
	[InlineData("{\"text\":\"  \"}")]
	[InlineData("{\"text\":\"longer than host budget\"}")]
	[InlineData("{\"text\":\"Hello\",\"stop\":true}")]
	[InlineData("{\"text\":\"Hello\",\"options\":{\"pitch\":2.1}}")]
	[InlineData("{\"text\":\"Hello\",\"options\":{\"volume\":-0.1}}")]
	[InlineData("{\"text\":\"Hello\",\"options\":{\"rate\":0}}")]
	[InlineData("{\"text\":\"Hello\",\"options\":{\"locale\":{\"id\":null,\"language\":\"en\",\"country\":\"\",\"name\":\"\"}}}")]
	public async Task InvalidSpeechContractsAreRejectedBeforeAnyNativeSpeech(string args)
	{
		var native = new Speech();
		var registry = new CommandRegistry();
		new TextToSpeechProvider(native, Inline, new HybridAppOptions("test.", 8)).Register(registry);
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() => Dispatch(registry, "speech.speak", args))).Code);
		Assert.Equal(0, native.Speaks);
	}

	[Fact]
	public async Task CancellingSpeechPassesItsOperationTokenToNativeAndLeavesOtherFeaturesUsable()
	{
		var native = new Speech { Wait = true };
		using var notifications = new NativeNotifications();
		using var network = new ConnectivityProvider(new Network(), Inline, notifications);
		using var hub = new HybridAppTargetObject([new TextToSpeechProvider(native, Inline, new HybridAppOptions("test.")), network],
			new HybridAppOptions("test."), notifications: notifications);
		var session = Guid.NewGuid().ToString("D");
		await Call(hub, session, "core.connect", new { });
		var id = Guid.NewGuid().ToString("D");
		var pending = Call(hub, session, "speech.speak", new { text = "Speak until cancelled" }, id);
		await native.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
		Assert.False(native.Cancellation.IsCancellationRequested);
		Assert.True((await Call(hub, session, "core.cancel", new { requestId = id })).Result!.Value.GetProperty("accepted").GetBoolean());
		Assert.Equal("cancelled", (await pending).Status);
		Assert.True(native.Cancellation.IsCancellationRequested);
		Assert.Equal("success", (await Call(hub, session, "connectivity.info", new { })).Status);
		Assert.Equal("success", (await Call(hub, session, "speech.locales", new { })).Status);
	}

	[Fact]
	public async Task DisconnectCancelsActualNativeSpeech()
	{
		var native = new Speech { Wait = true };
		using var hub = new HybridAppTargetObject([new TextToSpeechProvider(native, Inline, new HybridAppOptions("test."))], new HybridAppOptions("test."));
		var session = Guid.NewGuid().ToString("D");
		await Call(hub, session, "core.connect", new { });
		var pending = Call(hub, session, "speech.speak", new { text = "Disconnect me" });
		await native.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
		hub.Disconnect();
		Assert.Equal("cancelled", (await pending).Status);
		Assert.True(native.Cancellation.IsCancellationRequested);
	}

	[Theory]
	[InlineData("http://example.com")]
	[InlineData("file:///private/file")]
	[InlineData("mailto:user@example.com")]
	[InlineData("javascript:alert(1)")]
	[InlineData("https://user:password@example.com")]
	[InlineData("https://127.0.0.1")]
	[InlineData("https://10.0.0.1")]
	[InlineData("https://[::1]")]
	[InlineData("https://localhost")]
	[InlineData("https://app.local")]
	[InlineData("https://host.internal")]
	[InlineData("https://example.com:444")]
	public async Task UnsafeLaunchUrlsAreRejectedBeforeNativeSideEffects(string url)
	{
		var native = new SystemLauncher();
		var registry = new CommandRegistry();
		new LauncherProvider(native, Inline).Register(registry);
		var args = JsonSerializer.Serialize(new { url });
		Assert.Equal("invalid_url", (await Assert.ThrowsAsync<CommandException>(() => Dispatch(registry, "launcher.openUrl", args))).Code);
		Assert.Equal(0, native.Opens);
	}

	[Fact]
	public async Task LauncherActuallyOpensTheUriAndRejectsRefusalUnsupportedAndExtraAuthority()
	{
		var native = new SystemLauncher();
		var registry = new CommandRegistry();
		new LauncherProvider(native, Inline).Register(registry);
		Assert.True((await Dispatch(registry, "launcher.openUrl", "{\"url\":\"https://dot.net/maui\"}")).GetProperty("accepted").GetBoolean());
		Assert.Equal("https://dot.net/maui", native.Uri!.AbsoluteUri);
		Assert.Equal(1, native.Opens);
		native.Accepted = false;
		Assert.Equal("launch_failed", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "launcher.openUrl", "{\"url\":\"https://dot.net/maui\"}"))).Code);
		native.Error = new NotSupportedException();
		await Assert.ThrowsAsync<NotSupportedException>(() => Dispatch(registry, "launcher.openUrl", "{\"url\":\"https://dot.net/maui\"}"));
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "launcher.openUrl", "{\"url\":\"https://dot.net/maui\",\"path\":\"private\"}"))).Code);
		using var cancelled = new CancellationTokenSource();
		cancelled.Cancel();
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
			Dispatch(registry, "launcher.openUrl", "{\"url\":\"https://dot.net/maui\"}", cancelled.Token));
		Assert.Equal(3, native.Opens);
	}

	[Fact]
	public async Task NativeUnsupportedIsAnErrorNotAnInventedSuccess()
	{
		var speech = new Speech { Error = new NotSupportedException() };
		var launcher = new SystemLauncher { Error = new NotSupportedException() };
		using var notifications = new NativeNotifications();
		using var network = new ConnectivityProvider(new Network { ReadError = new NotSupportedException() }, Inline, notifications);
		using var hub = new HybridAppTargetObject([
			new TextToSpeechProvider(speech, Inline, new HybridAppOptions("test.")), new LauncherProvider(launcher, Inline), network
		], new HybridAppOptions("test."), notifications: notifications);
		var session = Guid.NewGuid().ToString("D");
		await Call(hub, session, "core.connect", new { });
		foreach (var command in new[] { "connectivity.info", "speech.locales", "speech.speak", "launcher.openUrl" })
		{
			var result = await Call(hub, session, command, command is "speech.locales" or "connectivity.info" ? new { } :
				command == "speech.speak" ? (object)new { text = "Hello" } : new { url = "https://dot.net/maui" });
			Assert.Equal("error", result.Status);
			Assert.Equal("unsupported", result.Error!.Code);
			Assert.Null(result.Result);
		}
	}

	sealed class Network : IConnectivity
	{
		public NetworkAccess Access { get; set; } = NetworkAccess.Internet;
		public ConnectionProfile[] Profiles { get; set; } = [ConnectionProfile.WiFi];
		public Exception? ReadError;
		public NetworkAccess NetworkAccess => ReadError is null ? Access : throw ReadError;
		public IEnumerable<ConnectionProfile> ConnectionProfiles => Profiles;
		public EventHandler<ConnectivityChangedEventArgs>? Handler;
		public Exception? SubscribeError;
		public int Adds;
		public int Listeners => Handler?.GetInvocationList().Length ?? 0;
		public event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged
		{
			add { if (SubscribeError is not null) throw SubscribeError; Adds++; Handler += value; }
			remove { Handler -= value; }
		}
		public void Change(NetworkAccess access, ConnectionProfile[] profiles)
		{
			Access = access; Profiles = profiles;
			Handler?.Invoke(this, new ConnectivityChangedEventArgs(access, profiles));
		}
	}

	sealed class Speech : ITextToSpeech
	{
		public string? Text;
		public SpeechOptions? Options;
		public CancellationToken Cancellation;
		public int Speaks;
		public bool Wait;
		public Exception? Error;
		public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
		public Task<IEnumerable<Locale>> GetLocalesAsync() => Error is null
			? Task.FromResult<IEnumerable<Locale>>([new Locale("en-US", "", "Native voice", "native")])
			: Task.FromException<IEnumerable<Locale>>(Error);
		public async Task SpeakAsync(string text, SpeechOptions? options = null, CancellationToken cancelToken = default)
		{
			Speaks++; Text = text; Options = options; Cancellation = cancelToken;
			Started.TrySetResult();
			if (Error is not null) throw Error;
			if (Wait) await Task.Delay(Timeout.Infinite, cancelToken);
		}
	}

	sealed class SystemLauncher : ILauncher
	{
		public Uri? Uri;
		public int Opens;
		public bool Accepted = true;
		public Exception? Error;
		public Task<bool> OpenAsync(Uri uri)
		{
			Uri = uri; Opens++;
			return Error is null ? Task.FromResult(Accepted) : Task.FromException<bool>(Error);
		}
		public Task<bool> CanOpenAsync(Uri uri) => throw new InvalidOperationException("No capability-query substitute for actual launch.");
		public Task<bool> TryOpenAsync(Uri uri) => throw new InvalidOperationException("Use the actual native OpenAsync operation.");
		public Task<bool> OpenAsync(OpenFileRequest request) => throw new InvalidOperationException("No file authority.");
	}
}
