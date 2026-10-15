using System.Text.Json;
using Microsoft.Maui.Controls.HybridWebApps;
using Microsoft.Maui.Devices;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class NativeAnnotationTests
{
	static async Task<JsonElement> Dispatch(CommandRegistry registry, string command, string args = "{}")
	{
		using var document = JsonDocument.Parse(args);
		return await registry.DispatchAsync(command, document.RootElement, new Operation("session", "request", default, (_, _) => { }));
	}
	static async Task<Response> Call(HybridAppTargetObject hub, string session, string command, object args)
	{
		var wire = JsonSerializer.Serialize(new { version = 1, session, id = Guid.NewGuid().ToString("D"), command, args });
		return JsonSerializer.Deserialize(await hub.Invoke(wire), ProtocolJsonContext.Default.Response)!;
	}

	[Fact]
	public async Task CuratedIdentityPropertiesUseNativeEnumsVersionsAndExtensibleValueIdentifiers()
	{
		var device = new FakeDeviceInfo { Platform = DevicePlatform.Create("CustomDesktop"), Idiom = DeviceIdiom.Create("CustomIdiom") };
		var registry = TestComposition.CreateRegistry(new FakeAppInfo(), device);
		foreach (var (command, expected) in new[]
		{
			("appInfo.packageName", "test.native.app"), ("appInfo.name", "Test app"), ("appInfo.versionString", "1.2"),
			("appInfo.version", "1.2"), ("appInfo.buildString", "3"), ("appInfo.requestedTheme", "dark"),
			("appInfo.packagingModel", "packaged"), ("appInfo.requestedLayoutDirection", "rightToLeft"),
			("deviceInfo.model", "Test device"), ("deviceInfo.manufacturer", "Native manufacturer"), ("deviceInfo.name", "Native device"),
			("deviceInfo.versionString", "16"), ("deviceInfo.version", "16.0.1.2"), ("deviceInfo.platform", "CustomDesktop"),
			("deviceInfo.idiom", "CustomIdiom"), ("deviceInfo.deviceType", "virtual"),
		})
			Assert.Equal(expected, (await Dispatch(registry, command)).GetString());
		device.Platform = default;
		device.Idiom = default;
		Assert.Equal("", (await Dispatch(registry, "deviceInfo.platform")).GetString());
		Assert.Equal("", (await Dispatch(registry, "deviceInfo.idiom")).GetString());
		Assert.DoesNotContain("appInfo.showSettingsUI", registry.Names);
	}

	[Fact]
	public async Task DisplaySetterUsesTheHostOverrideAndPreservesVoidNativeCompletionAndErrors()
	{
		var display = new Display();
		var dispatches = 0;
		var registry = TestComposition.CreateRegistry(display, new NativeDispatcher(action => { dispatches++; return action(); }));
		Assert.False((await Dispatch(registry, "display.keepScreenOn")).GetBoolean());
		Assert.Equal(JsonValueKind.Null, (await Dispatch(registry, "display.setKeepScreenOn", """{"value":true}""")).ValueKind);
		Assert.True(display.KeepScreenOn);
		await Dispatch(registry, "display.setKeepScreenOn", "[false]");
		Assert.False(display.KeepScreenOn);
		var failure = new NotSupportedException("Native setter error");
		display.Error = failure;
		Assert.Same(failure, await Assert.ThrowsAsync<NotSupportedException>(() =>
			Dispatch(registry, "display.setKeepScreenOn", "[true]")));
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "display.setKeepScreenOn", "[null]"))).Code);
		Assert.Equal(4, dispatches);
	}

	[Fact]
	public async Task BatteryAndDisplayEventsUseNativeArgsAndDetachOnLastUnsubscribeReloadAndDisposal()
	{
		var battery = new Battery();
		var display = new Display();
		using var notifications1 = new NativeNotifications();
		using var notifications2 = new NativeNotifications();
		using var first = TestComposition.CreateHub([battery, display], new(), notifications: notifications1);
		using var second = TestComposition.CreateHub([battery, display], new(), notifications: notifications2);
		var events1 = new List<NativeEvent>();
		var events2 = new List<NativeEvent>();
		first.NativeEventReceived += events1.Add;
		second.NativeEventReceived += events2.Add;
		var session1 = Guid.NewGuid().ToString("D");
		var session2 = Guid.NewGuid().ToString("D");
		await Call(first, session1, "core.connect", new { });
		await Call(second, session2, "core.connect", new { });
		Assert.Null(battery.Info);
		Assert.Null(display.Changed);
		foreach (var topic in new[] { "battery.infoChanged", "battery.energySaverStatusChanged", "display.mainDisplayInfoChanged" })
		{
			Assert.Equal("success", (await Call(first, session1, "core.subscribe", new { topic })).Status);
			Assert.Equal("success", (await Call(second, session2, "core.subscribe", new { topic })).Status);
		}
		Assert.Equal(2, battery.Info!.GetInvocationList().Length);
		Assert.Equal(2, battery.Saver!.GetInvocationList().Length);
		Assert.Equal(2, display.Changed!.GetInvocationList().Length);
		var retired = battery.Info;
		battery.Raise();
		display.Raise();
		Assert.Equal(3, events1.Count);
		Assert.Equal(3, events2.Count);
		Assert.Equal("charging", events1[0].Data.GetProperty("state").GetString());
		Assert.Equal("usb", events1[0].Data.GetProperty("powerSource").GetString());
		Assert.Equal("on", events1[1].Data.GetProperty("energySaverStatus").GetString());
		var metrics = events1[2].Data.GetProperty("displayInfo");
		Assert.Equal(0, metrics.GetProperty("width").GetDouble());
		Assert.Equal(59.5, metrics.GetProperty("refreshRate").GetDouble());
		Assert.Equal("rotation270", metrics.GetProperty("rotation").GetString());
		await Call(first, session1, "core.unsubscribe", new { topic = "battery.infoChanged" });
		retired.Invoke(battery, new BatteryInfoChangedEventArgs(0.25, BatteryState.Full, BatteryPowerSource.AC));
		Assert.Equal(3, events1.Count);
		Assert.Equal(4, events2.Count);
		session1 = Guid.NewGuid().ToString("D");
		await Call(first, session1, "core.connect", new { });
		Assert.Single(display.Changed.GetInvocationList());
		retired.Invoke(battery, new BatteryInfoChangedEventArgs(0.25, BatteryState.Full, BatteryPowerSource.AC));
		Assert.Equal(3, events1.Count);
		first.Dispose();
		Assert.Single(battery.Info!.GetInvocationList());
		second.Dispose();
		Assert.Null(battery.Info);
		Assert.Null(battery.Saver);
		Assert.Null(display.Changed);
	}

	sealed class Battery : IBattery
	{
		public double ChargeLevel => 0.123;
		public BatteryState State => BatteryState.Charging;
		public BatteryPowerSource PowerSource => BatteryPowerSource.Usb;
		public EnergySaverStatus EnergySaverStatus => EnergySaverStatus.On;
		public EventHandler<BatteryInfoChangedEventArgs>? Info;
		public EventHandler<EnergySaverStatusChangedEventArgs>? Saver;
		public event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChanged { add => Info += value; remove => Info -= value; }
		public event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged { add => Saver += value; remove => Saver -= value; }
		public void Raise()
		{
			Info?.Invoke(this, new(ChargeLevel, State, PowerSource));
			Saver?.Invoke(this, new(EnergySaverStatus));
		}
	}
	sealed class Display : IDeviceDisplay
	{
		bool _keepScreenOn;
		internal Exception? Error;
		public bool KeepScreenOn { get => _keepScreenOn; set { if (Error is not null) throw Error; _keepScreenOn = value; } }
		public DisplayInfo MainDisplayInfo => new(0, 0, 0, DisplayOrientation.Unknown, DisplayRotation.Rotation270, 59.5f);
		public EventHandler<DisplayInfoChangedEventArgs>? Changed;
		public event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged { add => Changed += value; remove => Changed -= value; }
		public void Raise() => Changed?.Invoke(this, new(MainDisplayInfo));
	}
}
