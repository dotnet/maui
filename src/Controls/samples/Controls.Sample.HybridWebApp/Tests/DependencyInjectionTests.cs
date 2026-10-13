using System.Reflection;
using System.Text.Json;
using HybridApp.ExternalProvider;
using Maui.Controls.Sample.HybridWebApp.Playground;
using Maui.HybridApp;
using Maui.HybridApp.Essentials.AppInfo;
using Maui.HybridApp.Essentials.Clipboard;
using Maui.HybridApp.Essentials.DeviceInfo;
using Maui.HybridApp.Essentials.Preferences;
using Maui.HybridApp.Essentials.Battery;
using Maui.HybridApp.Essentials.Display;
using Maui.HybridApp.Essentials.FilePicker;
using Maui.HybridApp.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class DependencyInjectionTests
{
	static readonly string[] NewCommands = ["battery.info", "display.info", "clipboard.hasText",
		"preferences.get", "preferences.set", "preferences.contains", "preferences.delete", "preferences.clear",
		"filePicker.pick", "filePicker.pickMultiple",
		"file.openRead", "file.readBytes", "file.seek", "file.closeReader", "file.release",
		"dialogs.alert", "dialogs.confirm", "dialogs.prompt", "dialogs.actionSheet", "remote.open", "remote.info", "remote.bounds", "remote.control", "remote.navigate", "menus.info", "menus.install", "menus.update", "menus.item", "menus.clear", "window.find", "window.closeCurrent", "window.activate", "window.center", "window.size", "window.position", "window.info", "window.title", "application.theme", "application.setTheme",
		"window.bounds", "window.open", "window.list", "window.close"];
	static ServiceProvider Build(ServiceCollection services) =>
		services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

	static async Task<Response> Call(HybridAppTargetObject hub, string session, string command, string args = "{}")
	{
		using var document = JsonDocument.Parse(args);
		var request = new Request(1, session, Guid.NewGuid().ToString("D"), command, document.RootElement);
		return JsonSerializer.Deserialize(await hub.Invoke(JsonSerializer.Serialize(request, ProtocolJsonContext.Default.Request)),
			ProtocolJsonContext.Default.Response)!;
	}

	[Fact]
	public async Task ProductConvenienceIsIdempotentPreservesHostOverridesAndRegistersOnlyImplementedApis()
	{
		var services = new ServiceCollection();
		var clipboard = new FakeClipboard();
		var preferences = new FakePreferences();
		var dispatcher = new NativeDispatcher(action => action());
		var clock = new TrackedTimeProvider();
		services.AddSingleton<IClipboard>(clipboard);
		services.AddSingleton<IPreferences>(preferences);
		services.AddSingleton(dispatcher);
		services.AddSingleton<TimeProvider>(clock);
		services.AddHybridApp(new HybridAppOptions("host-policy.", 8));
		var registrations = services.Count;
		services.AddHybridApp(new HybridAppOptions("host-policy.", 8));
		Assert.Equal(registrations, services.Count);
		Assert.Throws<InvalidOperationException>(() => services.AddHybridApp(new HybridAppOptions("other-policy.", 8)));
		Assert.Throws<InvalidOperationException>(() => services.AddHybridApp(new HybridAppOptions("host-policy.", 9)));
		Assert.Equal(registrations, services.Count);
		using var root = Build(services);
		using var scope = root.CreateScope();
		Assert.Same(clipboard, scope.ServiceProvider.GetRequiredService<IClipboard>());
		Assert.Same(preferences, scope.ServiceProvider.GetRequiredService<IPreferences>());
		Assert.Same(dispatcher, scope.ServiceProvider.GetRequiredService<NativeDispatcher>());
		Assert.Same(clock, scope.ServiceProvider.GetRequiredService<TimeProvider>());
		Assert.Equal(new[] { typeof(BatteryProvider), typeof(DisplayProvider), typeof(FilePickerProvider),
			typeof(WindowProvider), typeof(DialogProvider), typeof(RemoteWebViewProvider), typeof(MenuProvider), typeof(WindowEventsProvider), typeof(ApplicationProvider),
			typeof(AppInfoProvider), typeof(DeviceInfoProvider), typeof(ClipboardProvider), typeof(PreferencesProvider) },
			scope.ServiceProvider.GetServices<IHybridCommandProvider>().Select(provider => provider.GetType()));
		var hub = scope.ServiceProvider.GetRequiredService<HybridAppTargetObject>();
		var session = Guid.NewGuid().ToString("D");
		var ready = (await Call(hub, session, "core.connect")).Result!.Value.Deserialize(ProtocolJsonContext.Default.ReadyResult)!;
		Assert.Equal(new[] { "app.info", "clipboard.read", "clipboard.write", "core.cancel", "core.connect",
			"core.subscribe", "core.unsubscribe", "device.info", "preferences.read", "preferences.remove", "preferences.write" }.Concat(NewCommands).Order(StringComparer.Ordinal), ready.Commands);
		Assert.Equal(new[] { "dialogs.actionSheet", "dialogs.alert", "dialogs.confirm", "dialogs.prompt", "filePicker.pick", "filePicker.pickMultiple" }, ready.InteractiveCommands);
		Assert.Equal("host-policy.", ready.PreferenceNamespace);
		Assert.Equal(8, ready.MaximumTextLength);
		Assert.Equal("unknown_command", (await Call(hub, session, "camera.capture")).Error!.Code);
		Assert.Equal("unknown_command", (await Call(hub, session, "sample.echo")).Error!.Code);
		Assert.Equal(1, clock.ActiveTimers);
		scope.Dispose();
		Assert.Equal(0, clock.ActiveTimers);
	}

	[Fact]
	public async Task StartupComposesExternalAndEssentialsProvidersWithActualHostPolicy()
	{
		var services = new ServiceCollection();
		var clipboard = new FakeClipboard();
		var preferences = new FakePreferences();
		services.AddSingleton<IAppInfo>(new FakeAppInfo());
		services.AddSingleton<IDeviceInfo>(new FakeDeviceInfo());
		services.AddSingleton<IClipboard>(clipboard);
		services.AddSingleton<IPreferences>(preferences);
		services.AddScoped(_ => new NativeDispatcher(action => action()));
		services.AddHybridApp(new HybridAppOptions("di-host.", 4))
			.AddHybridProvider<PluginProvider>();
		using var root = Build(services);
		Assert.Throws<InvalidOperationException>(() => root.GetRequiredService<HybridAppTargetObject>());
		using var scope = root.CreateScope();
		var hub = scope.ServiceProvider.GetRequiredService<HybridAppTargetObject>();
		Assert.Same(hub, scope.ServiceProvider.GetRequiredService<HybridAppTargetObject>());
		Assert.Same(clipboard, scope.ServiceProvider.GetRequiredService<IClipboard>());
		Assert.Same(preferences, scope.ServiceProvider.GetRequiredService<IPreferences>());
		Assert.Equal(14, scope.ServiceProvider.GetServices<IHybridCommandProvider>().Count());
		Assert.NotEqual(typeof(PluginProvider).Assembly, typeof(HybridAppTargetObject).Assembly);
		Assert.Null(ProtocolJsonContext.Default.GetTypeInfo(typeof(PluginArgs)));
		var session = Guid.NewGuid().ToString("D");
		var ready = (await Call(hub, session, "core.connect")).Result!.Value.Deserialize(ProtocolJsonContext.Default.ReadyResult)!;
		Assert.Equal("di-host.", ready.PreferenceNamespace);
		Assert.Equal(4, ready.MaximumTextLength);
		Assert.Equal(new[] { "app.info", "clipboard.read", "clipboard.write", "core.cancel", "core.connect", "core.subscribe",
			"core.unsubscribe", "device.info", "plugin.disabled", "plugin.echo", "preferences.read", "preferences.remove", "preferences.write" }.Concat(NewCommands).Order(StringComparer.Ordinal), ready.Commands);
		Assert.Equal("Test app", (await Call(hub, session, "app.info")).Result!.Value.GetProperty("appName").GetString());
		Assert.Equal("Android", (await Call(hub, session, "device.info")).Result!.Value.GetProperty("platform").GetString());
		Assert.Equal("success", (await Call(hub, session, "preferences.write", "{\"key\":\"key\",\"value\":\"1234\"}")).Status);
		Assert.Equal("1234", preferences.Values["di-host.key"]);
		Assert.Equal("invalid_arguments", (await Call(hub, session, "preferences.write", "{\"key\":\"key\",\"value\":\"12345\"}")).Error!.Code);
		Assert.Equal("invalid_arguments", (await Call(hub, session, "clipboard.write", "{\"text\":\"12345\"}")).Error!.Code);
		var events = new List<NativeEvent>();
		hub.NativeEventReceived += events.Add;
		Assert.Equal("success", (await Call(hub, session, "core.subscribe", "{\"topic\":\"plugin.changed\"}")).Status);
		Assert.Equal("success", (await Call(hub, session, "plugin.echo", "{\"text\":\"\\u4f60\\u597d\"}")).Status);
		Assert.Equal("你好", Assert.Single(events).Data.Deserialize(PluginJsonContext.Default.PluginResult)!.Text);
		Assert.Equal("disabled_command", (await Call(hub, session, "plugin.disabled", "{\"text\":\"no\"}")).Error!.Code);
		Assert.Equal("invalid_arguments", (await Call(hub, session, "plugin.echo", "{\"text\":null}")).Error!.Code);
	}

	[Fact]
	public async Task TwoOwningScopesIsolateProvidersSessionsCountersAndEventsWhileReloadPreservesState()
	{
		var services = new ServiceCollection();
		services.AddHybridApp(new HybridAppOptions("scope-test.")).AddPlayground("test-startup", DateTimeOffset.UtcNow);
		using var root = Build(services);
		using var first = root.CreateScope();
		using var second = root.CreateScope();
		var hub1 = first.ServiceProvider.GetRequiredService<HybridAppTargetObject>();
		var hub2 = second.ServiceProvider.GetRequiredService<HybridAppTargetObject>();
		Assert.NotSame(hub1, hub2);
		Assert.NotSame(first.ServiceProvider.GetRequiredService<SampleCounter>(), second.ServiceProvider.GetRequiredService<SampleCounter>());
		var providers1 = first.ServiceProvider.GetServices<IHybridCommandProvider>().ToArray();
		var providers2 = second.ServiceProvider.GetServices<IHybridCommandProvider>().ToArray();
		Assert.All(providers1.Zip(providers2), pair => Assert.NotSame(pair.First, pair.Second));
		var session1 = Guid.NewGuid().ToString("D");
		var session2 = Guid.NewGuid().ToString("D");
		var events1 = new List<NativeEvent>();
		var events2 = new List<NativeEvent>();
		hub1.NativeEventReceived += events1.Add;
		hub2.NativeEventReceived += events2.Add;
		await Call(hub1, session1, "core.connect");
		await Call(hub2, session2, "core.connect");
		await Call(hub1, session1, "core.subscribe", "{\"topic\":\"service.changed\"}");
		await Call(hub2, session2, "core.subscribe", "{\"topic\":\"service.changed\"}");
		const string echo = "{\"text\":null,\"number\":0,\"flag\":false,\"items\":[]}";
		Assert.Equal(1, (await Call(hub1, session1, "sample.echo", echo)).Result!.Value.GetProperty("counter").GetInt32());
		Assert.Equal(0, (await Call(hub2, session2, "sample.counter")).Result!.Value.GetProperty("counter").GetInt32());
		Assert.Single(events1);
		Assert.Empty(events2);
		var reloaded = Guid.NewGuid().ToString("D");
		await Call(hub1, reloaded, "core.connect");
		Assert.Equal(1, (await Call(hub1, reloaded, "sample.counter")).Result!.Value.GetProperty("counter").GetInt32());
		Assert.Equal("stale_session", (await Call(hub1, session1, "sample.counter")).Error!.Code);
		Assert.Equal("stale_session", (await Call(hub1, session2, "sample.counter")).Error!.Code);
		Assert.Equal(2, (await Call(hub1, reloaded, "sample.echo", echo)).Result!.Value.GetProperty("counter").GetInt32());
		Assert.Single(events1);
		Assert.Equal(1, (await Call(hub2, session2, "sample.echo", echo)).Result!.Value.GetProperty("counter").GetInt32());
		Assert.Equal(session2, Assert.Single(events2).Session);
		Assert.True(hub1.IsCurrent(reloaded));
		Assert.True(hub2.IsCurrent(session2));
	}

	[Fact]
	public async Task ScopeDisposalCancelsWorkClearsListenersStopsLeaseAndDisposesProvidersOnce()
	{
		var services = new ServiceCollection();
		var clock = new TrackedTimeProvider();
		services.AddSingleton<TimeProvider>(clock);
		services.AddScoped<ScopeProbeState>();
		services.AddHybridApp(new HybridAppOptions("scope-test.")).AddPlayground("test-startup", DateTimeOffset.UtcNow)
			.AddHybridProvider<ScopeProbeProvider>();
		using var root = Build(services);
		using var first = root.CreateScope();
		using var second = root.CreateScope();
		var state1 = first.ServiceProvider.GetRequiredService<ScopeProbeState>();
		var state2 = second.ServiceProvider.GetRequiredService<ScopeProbeState>();
		var hub1 = first.ServiceProvider.GetRequiredService<HybridAppTargetObject>();
		var hub2 = second.ServiceProvider.GetRequiredService<HybridAppTargetObject>();
		Assert.Same(hub1, first.ServiceProvider.GetRequiredService<HybridAppTargetObject>());
		Assert.Equal(1, state1.Registrations);
		Assert.Equal(1, state2.Registrations);
		Assert.Equal(2, clock.ActiveTimers);
		var session1 = Guid.NewGuid().ToString("D");
		var session2 = Guid.NewGuid().ToString("D");
		await Call(hub1, session1, "core.connect");
		await Call(hub2, session2, "core.connect");
		await Call(hub1, session1, "core.subscribe", "{\"topic\":\"operation.progress\"}");
		var events = new List<NativeEvent>();
		var progressed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		hub1.NativeEventReceived += value =>
		{
			events.Add(value);
			progressed.TrySetResult();
		};
		var work = Call(hub1, session1, "sample.work", "{\"steps\":20,\"delayMs\":100}");
		await progressed.Task.WaitAsync(TimeSpan.FromSeconds(5));
		first.Dispose();
		Assert.Equal("cancelled", (await work).Status);
		Assert.Equal(1, state1.Disposals);
		Assert.Equal(0, state2.Disposals);
		Assert.Equal(1, clock.ActiveTimers);
		Assert.False(hub1.IsCurrent(session1));
		Assert.Equal("disconnected", (await Call(hub1, session1, "sample.counter")).Error!.Code);
		Assert.Null(typeof(HybridAppTargetObject).GetField(nameof(HybridAppTargetObject.NativeEventReceived),
			BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(hub1));
		hub1.Dispose();
		first.Dispose();
		Assert.Equal(1, state1.Disposals);
		Assert.Equal(1, clock.ActiveTimers);
		clock.Tick();
		Assert.True(hub2.IsCurrent(session2));
		Assert.Equal("success", (await Call(hub2, session2, "sample.counter")).Status);
		second.Dispose();
		Assert.Equal(1, state2.Disposals);
		Assert.Equal(0, clock.ActiveTimers);
	}

	[Fact]
	public async Task GenericHubCanStillUseOnlyExplicitExternalProvidersWithoutImplicitEssentials()
	{
		var services = new ServiceCollection();
		services.AddSingleton(new HybridAppOptions("custom-host."));
		services.AddScoped<HybridAppTargetObject>();
		services.AddHybridProvider<PluginProvider>();
		using var root = Build(services);
		using var scope = root.CreateScope();
		Assert.Single(scope.ServiceProvider.GetServices<IHybridCommandProvider>());
		var hub = scope.ServiceProvider.GetRequiredService<HybridAppTargetObject>();
		var session = Guid.NewGuid().ToString("D");
		var ready = (await Call(hub, session, "core.connect")).Result!.Value.Deserialize(ProtocolJsonContext.Default.ReadyResult)!;
		Assert.Equal(new[] { "core.cancel", "core.connect", "core.subscribe", "core.unsubscribe", "plugin.disabled", "plugin.echo" }, ready.Commands);
		Assert.Equal("custom-host.", ready.PreferenceNamespace);
		Assert.Equal(4096, ready.MaximumTextLength);
		Assert.Equal("unknown_command", (await Call(hub, session, "app.info")).Error!.Code);
		Assert.Equal("unknown_command", (await Call(hub, session, "sample.counter")).Error!.Code);
		Assert.Equal("unknown_topic", (await Call(hub, session, "core.subscribe", "{\"topic\":\"service.changed\"}")).Error!.Code);
	}

	[Fact]
	public void ConflictingProvidersFailBeforeStartingALeaseAndAreDisposedByTheirScope()
	{
		var services = new ServiceCollection();
		var clock = new TrackedTimeProvider();
		services.AddSingleton<TimeProvider>(clock);
		services.AddScoped<ScopeProbeState>();
		services.AddHybridApp(new HybridAppOptions("scope-test.")).AddHybridProvider<ScopeProbeProvider>().AddHybridProvider<ScopeProbeProvider>();
		using var root = Build(services);
		using var scope = root.CreateScope();
		var state = scope.ServiceProvider.GetRequiredService<ScopeProbeState>();
		Assert.Throws<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<HybridAppTargetObject>());
		Assert.Equal(2, state.Registrations);
		Assert.Equal(0, clock.ActiveTimers);
		scope.Dispose();
		Assert.Equal(2, state.Disposals);
	}

	[Fact]
	public void InvalidHostPolicyCannotLeakALeaseOrExposeTheEntirePreferenceStore()
	{
		var clock = new TrackedTimeProvider();
		Assert.Throws<ArgumentNullException>(() => new HybridAppOptions(null!));
		Assert.Throws<ArgumentException>(() => new HybridAppOptions(""));
		Assert.Throws<ArgumentOutOfRangeException>(() => new HybridAppOptions("host.", maximumTextLength: 0));
		Assert.Throws<ArgumentNullException>(() => new HybridAppTargetObject(Array.Empty<IHybridCommandProvider>(), null!, clock));
		Assert.Equal(0, clock.ActiveTimers);
		var services = new ServiceCollection();
		services.AddSingleton<TimeProvider>(clock);
		services.AddSingleton<IPreferences>(new FakePreferences());
		services.AddSingleton(new HybridAppOptions("host."));
		services.AddScoped<HybridAppTargetObject>();
		services.AddScoped<IHybridCommandProvider>(provider => new PreferencesProvider(
			provider.GetRequiredService<IPreferences>(), new NativeDispatcher(action => action()), ""));
		using var root = Build(services);
		using var scope = root.CreateScope();
		Assert.Throws<ArgumentException>(() => scope.ServiceProvider.GetRequiredService<HybridAppTargetObject>());
		Assert.Equal(0, clock.ActiveTimers);
	}

	public sealed class ScopeProbeState
	{
		public int Registrations { get; set; }
		public int Disposals { get; set; }
	}

	public sealed class ScopeProbeProvider(ScopeProbeState state) : IHybridCommandProvider, IDisposable
	{
		public void Register(CommandRegistry registry)
		{
			state.Registrations++;
			registry.Register("scope.probe", ProtocolJsonContext.Default.EmptyArgs, ProtocolJsonContext.Default.Ack,
				(_, _) => Task.FromResult(new Ack(true)));
		}

		public void Dispose() => state.Disposals++;
	}

	sealed class TrackedTimeProvider : TimeProvider
	{
		readonly List<TrackedTimer> _timers = [];
		public int ActiveTimers => _timers.Count(timer => !timer.Disposed);

		public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
		{
			Assert.True(dueTime == TimeSpan.FromSeconds(2) || dueTime == TimeSpan.FromSeconds(20) || dueTime == TimeSpan.FromSeconds(300));
			Assert.True(period == TimeSpan.FromSeconds(2) || period == Timeout.InfiniteTimeSpan);
			var timer = new TrackedTimer(callback, state, period);
			_timers.Add(timer);
			return timer;
		}

		public void Tick()
		{
			foreach (var timer in _timers.Where(timer => !timer.Disposed && timer.Period == TimeSpan.FromSeconds(2)))
				timer.Tick();
		}

		sealed class TrackedTimer(TimerCallback callback, object? state, TimeSpan period) : ITimer
		{
			public TimeSpan Period { get; } = period;
			public bool Disposed { get; private set; }
			public bool Change(TimeSpan dueTime, TimeSpan period) => !Disposed;
			public void Tick() => callback(state);
			public void Dispose() => Disposed = true;
			public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
		}
	}
}
