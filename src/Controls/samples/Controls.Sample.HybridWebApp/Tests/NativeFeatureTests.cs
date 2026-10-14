using System.Text;
using System.Text.Json;
using Maui.HybridApp;
using Maui.HybridApp.UI;
using Maui.HybridApp.Essentials.FilePicker;
using Maui.HybridApp.Essentials.Battery;
using Maui.HybridApp.Essentials.Display;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class NativeFeatureTests
{
	static readonly NativeDispatcher Inline = new(action => action());
	static WindowProvider WindowPlugin(HybridViewContext context, HybridWindowHost host, IDeviceInfo device) =>
		new(context, host, device, Inline, new NativeNotifications(), TimeProvider.System);

	static async Task<JsonElement> Dispatch(CommandRegistry registry, string name, string args = "{}", CancellationToken cancellation = default)
	{
		using var document = JsonDocument.Parse(args);
		return await registry.DispatchAsync(name, document.RootElement,
			new Operation(Guid.NewGuid().ToString("D"), Guid.NewGuid().ToString("D"), cancellation, (_, _) => { }));
	}

	static HybridViewContext Bind(string route = "showcase")
	{
		var page = new ContentPage();
		var context = new HybridViewContext();
		context.Bind(page, new Window(page) { Title = "Owning window" }, route);
		return context;
	}

	[Fact]
	public void WindowBindingPreservesNavigationPageOwnership()
	{
		var page = new ContentPage();
		var window = new Window(new NavigationPage(page));
		using var context = new HybridViewContext();
		context.Bind(page, window, "index.html#/welcome");
		Assert.Same(window, context.Page.Window);
		Assert.Same(window, context.Window);
	}

	[Fact]
	public void WindowBindingRejectsAContentPageDetachedByNavigationWrapping()
	{
		var page = new ContentPage();
		var window = new Window(page);
		window.Page = new NavigationPage(page);
		Assert.Null(page.Window);
		using var context = new HybridViewContext();
		Assert.Throws<ArgumentException>(() => context.Bind(page, window, "index.html#/welcome"));
	}

	[Theory]
	[InlineData("http://dot.net/maui")]
	[InlineData("file:///private/example")]
	[InlineData("javascript:alert(1)")]
	[InlineData("data:text/html,hello")]
	[InlineData("https://user:password@example.com")]
	[InlineData("https://0.0.0.1/_framework/hybridwebview.js")]
	[InlineData("https://127.0.0.1/")]
	[InlineData("https://[::1]/")]
	[InlineData("https://localhost/")]
	[InlineData("https://app.local/")]
	[InlineData("https://host.internal/")]
	[InlineData("https://example.com:9443")]
	[InlineData("https://example.com./")]
	[InlineData("https://example.com\\@localhost/")]
	public void RemoteTopLevelPolicyRejectsUnsafeDestinations(string address) =>
		Assert.Equal("invalid_url", Assert.Throws<CommandException>(() => RemoteWebAddress.Validate(address)).Code);

	[Fact]
	public void RemotePolicyAllowsPublicHttpsButMakesNoDnsFirewallClaim()
	{
		Assert.Equal("https://dot.net/maui", RemoteWebAddress.Validate("https://dot.net/maui").AbsoluteUri);
		Assert.Equal("https://learn.microsoft.com/dotnet/maui/", RemoteWebAddress.Validate("https://learn.microsoft.com/dotnet/maui/").AbsoluteUri);
		Assert.Throws<CommandException>(() => RemoteWebAddress.Validate("https://example.com/" + new string('a', 2048)));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(65536)]
	public async Task NativeReaderReturnsActualPartialBinaryChunks(int length)
	{
		await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(new string('a', length)));
		var result = await FilePickerProvider.ReadChunkAsync(stream, 65536, default);
		Assert.Equal(length, result.Bytes.Length);
		Assert.True(result.Eof);
	}

	[Fact]
	public async Task ReaderPreservesBinaryAndUnicodeBytesWithoutArbitraryFilePaths()
	{
		var expected = "你好 👋\n\"quoted\"\\tab\t";
		await using var stream = new MemoryStream(new byte[] { 0xef, 0xbb, 0xbf }.Concat(Encoding.UTF8.GetBytes(expected)).ToArray());
		var result = await FilePickerProvider.ReadChunkAsync(stream, 65536, default);
		Assert.Equal(new byte[] { 0xef, 0xbb, 0xbf }.Concat(Encoding.UTF8.GetBytes(expected)), result.Bytes);
	}

	[Fact]
	public async Task LargeBinaryFilesAreReadFullyAcrossBoundedMessagesNotRejectedAt64KiB()
	{
		var bytes = Enumerable.Range(0, 200000).Select(index => (byte)(index % 256)).ToArray();
		await using var stream = new MemoryStream(bytes);
		var all = new List<byte>();
		bool eof;
		do
		{
			var chunk = await FilePickerProvider.ReadChunkAsync(stream, 65536, default);
			Assert.True(chunk.Bytes.Length <= 65536);
			all.AddRange(chunk.Bytes);
			eof = chunk.Eof;
		} while (!eof);
		Assert.Equal(bytes, all);
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			FilePickerProvider.ReadChunkAsync(stream, 65537, default))).Code);
	}

	[Fact]
	public async Task PreviewHonorsDocumentCancellation()
	{
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		await using var stream = new MemoryStream([65]);
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => FilePickerProvider.ReadChunkAsync(stream, 65536, cancellation.Token));
	}

	[Fact]
	public async Task PickerDismissalIsNormalAndProviderOwnsStrictMetadata()
	{
		using var context = Bind();
		var picker = new CancelledPicker();
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new FilePickerProvider(picker, context));
		var result = await Dispatch(registry, "filePicker.pick", "{\"pickerTitle\":\"From TypeScript\",\"fileTypes\":{\"android\":[\"text/plain\"]}}");
		Assert.Equal(JsonValueKind.Null, result.ValueKind);
		Assert.Equal(1, picker.Calls);
		Assert.True(registry.IsInteractive("filePicker.pick"));
		Assert.True(registry.IsInteractive("filePicker.pickMultiple"));
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "filePicker.pick", "{\"path\":\"private\"}"))).Code);
		Assert.Equal(1, picker.Calls);
	}

	[Fact]
	public async Task ReadOnlyPowerAndDisplayUseDirectInterfacesAndNoSubscriptions()
	{
		var battery = new ReadOnlyBattery();
		var display = new ReadOnlyDisplay();
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new BatteryProvider(battery), new DisplayProvider(display));
		var power = await Dispatch(registry, "battery.info");
		Assert.Equal(0.42, power.GetProperty("chargeLevel").GetDouble());
		Assert.Equal("charging", power.GetProperty("state").GetString());
		Assert.Equal("ac", power.GetProperty("powerSource").GetString());
		Assert.Equal("off", power.GetProperty("energySaverStatus").GetString());
		var result = await Dispatch(registry, "display.info");
		Assert.Equal(3, result.GetProperty("density").GetDouble());
		Assert.Equal("portrait", result.GetProperty("orientation").GetString());
		Assert.Equal("rotation0", result.GetProperty("rotation").GetString());
		Assert.False(display.KeepScreenOn);
		Assert.False(registry.IsInteractive("battery.info"));
		Assert.False(registry.IsInteractive("display.info"));
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() => Dispatch(registry, "battery.info", "{\"extra\":true}"))).Code);
	}

	[Fact]
	public async Task BatteryPreservesTheNativeSinglePrecisionValueWithoutScalingOrRounding()
	{
		var level = (double)0.01f;
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new BatteryProvider(new ReadOnlyBattery(level)));
		var result = await Dispatch(registry, "battery.info");
		Assert.Equal(0.009999999776482582, result.GetProperty("chargeLevel").GetDouble());
		Assert.Equal(level, result.GetProperty("chargeLevel").GetDouble());
	}

	[Fact]
	public async Task WindowContextIsBoundOnceAndMobilePlacementIsExplicitlyUnsupported()
	{
		using var first = Bind();
		using var second = Bind("welcome");
		Assert.NotEqual(first.Id, second.Id);
		Assert.Throws<InvalidOperationException>(() => first.Bind(first.Page, first.Window, "welcome"));
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(WindowPlugin(first, new HybridWindowHost(), new FakeDeviceInfo()));
		var result = await Dispatch(registry, "window.info");
		Assert.Equal(first.Id, result.GetProperty("id").GetString());
		Assert.False(result.GetProperty("canResize").GetBoolean());
		Assert.True(result.GetProperty("canOpenWindows").GetBoolean());
		await Dispatch(registry, "window.title", "{\"title\":\"\\u4f60\\u597d\"}");
		Assert.Equal("你好", first.Window.Title);
		Assert.Equal("Owning window", second.Window.Title);
		await Assert.ThrowsAsync<NotSupportedException>(() => Dispatch(registry, "window.bounds", "{\"x\":10,\"y\":10,\"width\":800,\"height\":600}"));
		first.Dispose();
		Assert.Throws<CommandException>(() => first.Window);
		Assert.Equal("Owning window", second.Window.Title);
	}

	[Theory]
	[InlineData("iOS", false, false)]
	[InlineData("iOS", true, true)]
	[InlineData("MacCatalyst", false, true)]
	[InlineData("WinUI", false, true)]
	public async Task WindowCapabilitiesDistinguishIPhoneIPadAndDesktop(string platform, bool tablet, bool multiple)
	{
		using var context = Bind();
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(WindowPlugin(context, new HybridWindowHost(), new WindowDevice(platform, tablet)));
		var result = await Dispatch(registry, "window.info");
		Assert.Equal(multiple, result.GetProperty("canOpenWindows").GetBoolean());
		Assert.Equal(platform != "iOS", result.GetProperty("canResize").GetBoolean());
		if (!multiple)
			await Assert.ThrowsAsync<NotSupportedException>(() => Dispatch(registry, "window.open", "{\"url\":\"index.html#/showcase\"}"));
	}

	[Theory]
	[InlineData("{\"x\":0,\"y\":0,\"width\":0,\"height\":700}")]
	[InlineData("{\"x\":1e400,\"y\":0,\"width\":800,\"height\":700}")]
	[InlineData("{\"x\":0,\"y\":0,\"width\":800,\"height\":-1}")]
	[InlineData("{\"title\":null}")]
	public async Task NativeWindowValidationPrecedesWrites(string args)
	{
		using var context = Bind();
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(WindowPlugin(context, new HybridWindowHost(), new WindowDevice("MacCatalyst", false)));
		var name = args.Contains("title", StringComparison.Ordinal) ? "window.title" : "window.bounds";
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() => Dispatch(registry, name, args))).Code);
		Assert.Equal("Owning window", context.Window.Title);
	}

	[Fact]
	public async Task GeometryUsesPositiveFiniteNativeValuesNotSampleSizeOrPositionLimits()
	{
		using var context = Bind();
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(WindowPlugin(context, new HybridWindowHost(), new WindowDevice("MacCatalyst", false)));
		await Dispatch(registry, "window.size", "{\"width\":200,\"height\":6000}");
		Assert.Equal(200, context.Window.Width);
		Assert.Equal(6000, context.Window.Height);
		await Dispatch(registry, "window.position", "{\"x\":20000,\"y\":-20000}");
		Assert.Equal(20000, context.Window.X);
		Assert.Equal(-20000, context.Window.Y);
	}

	[Fact]
	public void DesktopCenterUsesActualDisplayOriginAndWindowSize()
	{
		var position = WindowCenter.Position(new Microsoft.Maui.Graphics.Rect(1920, 40, 1440, 900),
			new Microsoft.Maui.Graphics.Size(1000, 600));
		Assert.Equal(2140, position.X);
		Assert.Equal(190, position.Y);
		Assert.Equal("native_error", Assert.Throws<CommandException>(() =>
			WindowCenter.Position(new(0, 0, 0, 900), new(1000, 600))).Code);
	}

	[Theory]
	[InlineData("dialogs.alert", "{}")]
	[InlineData("dialogs.confirm", "{\"title\":null,\"message\":\"message\"}")]
	[InlineData("dialogs.prompt", "{\"title\":\"title\",\"message\":\"message\",\"extra\":true}")]
	public async Task DialogMetadataRejectsInvalidInputsWithoutInvokingAnyOsUi(string command, string args)
	{
		using var context = Bind();
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new DialogProvider(context));
		Assert.True(registry.IsInteractive(command));
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() => Dispatch(registry, command, args))).Code);
	}

	[Theory]
	[InlineData("dialogs.alert", "{\"title\":\"t\",\"message\":\"m\",\"cancel\":\" \"}")]
	[InlineData("dialogs.confirm", "{\"title\":\"t\",\"message\":\"m\",\"accept\":\"\",\"cancel\":\"Cancel\"}")]
	[InlineData("dialogs.prompt", "{\"title\":\"t\",\"message\":\"m\",\"maxLength\":-2}")]
	[InlineData("dialogs.prompt", "{\"title\":\"t\",\"message\":\"m\",\"maxLength\":2,\"initialValue\":\"long\"}")]
	[InlineData("dialogs.prompt", "{\"title\":\"t\",\"message\":\"m\",\"keyboard\":\"unknown\"}")]
	[InlineData("dialogs.prompt", "{\"title\":\"t\",\"message\":\"m\",\"keyboard\":\"text\",\"keyboardFlags\":[\"spellcheck\"]}")]
	public async Task DialogOptionsAreValidatedBeforeNativeUi(string command, string args)
	{
		using var context = Bind();
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new DialogProvider(context));
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() => Dispatch(registry, command, args))).Code);
	}

	[Theory]
	[InlineData("https://example.com")]
	[InlineData("javascript:alert(1)")]
	[InlineData("../showcase")]
	[InlineData("#/showcase")]
	public async Task TrustedWindowFactoryNeverAcceptsRemoteOrUnsafeBundleLocations(string route)
	{
		var host = new HybridWindowHost();
		var calls = 0;
		host.Bind(new Application(), _ => { calls++; return new Window(); });
		using var context = Bind();
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(WindowPlugin(context, host, new FakeDeviceInfo()));
		Assert.Equal("invalid_location", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "window.open", JsonSerializer.Serialize(new { url = route })))).Code);
		Assert.Equal(0, calls);
		host.ValidateLocation("index.html#/any-frontend-router");
		host.ValidateLocation("pages/help.html?mode=compact#section");
	}

	[Fact]
	public async Task DesktopGuestUsesAClippedNativeSurfaceWithoutChangingPrimaryLayoutOrAttachingABridge()
	{
		using var context = Bind();
		var trusted = new HybridWebView();
		var layout = context.CreateLayout(trusted);
		Assert.Single(layout.Children);
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new RemoteWebViewProvider(context, new WindowDevice("MacCatalyst", false), new NativeNotifications()));
		var result = await Dispatch(registry, "remote.open", "{\"url\":\"https://dot.net/maui\"}");
		Assert.True(result.GetProperty("supported").GetBoolean());
		var id = result.GetProperty("id").GetString();
		Assert.Equal(2, layout.Children.Count);
		Assert.Same(trusted, layout.Children[0]);
		var remoteGrid = Assert.IsAssignableFrom<AbsoluteLayout>(layout.Children[1]);
		var web = Assert.Single(remoteGrid.Children.OfType<WebView>());
		Assert.Equal("remote-webview", web.AutomationId);
		Assert.Empty(remoteGrid.Children.OfType<HybridWebView>());
		Assert.True(remoteGrid.IsClippedToBounds);
		Assert.False(remoteGrid.IsVisible, "Guest remains hidden until valid measured geometry arrives.");
		Assert.Equal("invalid_view", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "remote.control", "{\"id\":\"foreign\",\"action\":\"close\"}"))).Code);
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() => Dispatch(registry, "remote.bounds",
			$"{{\"id\":\"{id}\",\"x\":0,\"y\":0,\"width\":200,\"height\":200,\"clipX\":0,\"clipY\":0,\"clipWidth\":300,\"clipHeight\":200,\"viewportWidth\":1000,\"viewportHeight\":800,\"visible\":true}}"))).Code);
		await Dispatch(registry, "remote.open", "{\"url\":\"https://learn.microsoft.com/\"}");
		Assert.Equal(2, layout.Children.Count);
		Assert.DoesNotContain(web, ((AbsoluteLayout)layout.Children[1]).Children);
		context.Dispose();
		Assert.Single(layout.Children);
		Assert.Same(trusted, layout.Children[0]);
	}

	[Fact]
	public async Task MobileGuestIsExplicitlyUnsupportedWithoutIframeOrModalFallback()
	{
		using var context = Bind();
		context.CreateLayout(new HybridWebView());
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new RemoteWebViewProvider(context, new FakeDeviceInfo(), new NativeNotifications()));
		Assert.False((await Dispatch(registry, "remote.info")).GetProperty("supported").GetBoolean());
		await Assert.ThrowsAsync<NotSupportedException>(() => Dispatch(registry, "remote.open", "{\"url\":\"https://dot.net/maui\"}"));
	}

	[Fact]
	public async Task NativeNotificationsRequireRegisteredTopicsAndCurrentDocumentSubscription()
	{
		using var notifications = new NativeNotifications();
		var provider = new NotificationProvider();
		using var hub = TestComposition.CreateHub([provider], new HybridAppOptions("notifications."), notifications: notifications);
		var session = Guid.NewGuid().ToString("D");
		async Task Call(string command, string args)
		{
			using var document = JsonDocument.Parse(args);
			var request = new Request(1, session, Guid.NewGuid().ToString("D"), command, document.RootElement);
			await hub.Invoke(JsonSerializer.Serialize(request, ProtocolJsonContext.Default.Request));
		}
		await Call("core.connect", "{}");
		var received = new List<NativeEvent>();
		hub.NativeEventReceived += received.Add;
		notifications.Publish("menu.test", new Ack(true), ProtocolJsonContext.Default.Ack);
		Assert.Empty(received);
		await Call("core.subscribe", "{\"topic\":\"menu.test\"}");
		notifications.Publish("menu.test", new Ack(true), ProtocolJsonContext.Default.Ack);
		Assert.Equal(session, Assert.Single(received).Session);
		notifications.Publish("unknown", new Ack(true), ProtocolJsonContext.Default.Ack);
		Assert.Single(received);
		hub.Disconnect();
		notifications.Publish("menu.test", new Ack(true), ProtocolJsonContext.Default.Ack);
		Assert.Single(received);
	}

	[HybridTopic("menu.test")]
	internal sealed class NotificationProvider
	{
	}

	[Theory]
	[InlineData("WinUI", true)]
	[InlineData("MacCatalyst", false)]
	public async Task ActualNativeFramesCoalescePairsAndLastUnsubscribeStopsQueuedSources(string platform, bool observesPosition)
	{
		using var context = Bind();
		using var notifications = new NativeNotifications();
		var clock = new InteractivePolicyTests.ManualTime();
		using var provider = new WindowProvider(context, new HybridWindowHost(), new WindowDevice(platform, false), Inline, notifications, clock);
		using var hub = TestComposition.CreateHub([provider], new HybridAppOptions("geometry."), time: clock, notifications: notifications);
		var session = Guid.NewGuid().ToString("D");
		async Task Call(string command, string args = "{}")
		{
			using var document = JsonDocument.Parse(args);
			var request = new Request(1, session, Guid.NewGuid().ToString("D"), command, document.RootElement);
			var response = JsonSerializer.Deserialize(await hub.Invoke(JsonSerializer.Serialize(request, ProtocolJsonContext.Default.Request)),
				ProtocolJsonContext.Default.Response)!;
			Assert.Equal("success", response.Status);
		}
		await Call("core.connect");
		var received = new List<NativeEvent>();
		hub.NativeEventReceived += received.Add;
		await Call("core.subscribe", "{\"topic\":\"window.geometryChanged\"}");
		((IWindow)context.Window).FrameChanged(new Microsoft.Maui.Graphics.Rect(20, 30, 800, 600));
		((IWindow)context.Window).FrameChanged(new Microsoft.Maui.Graphics.Rect(40, 50, 900, 650));
		Assert.Empty(received);
		clock.Advance(TimeSpan.FromMilliseconds(16));
		var data = Assert.Single(received).Data;
		Assert.Equal(context.Id, data.GetProperty("windowId").GetString());
		Assert.Equal(900, data.GetProperty("size").GetProperty("width").GetDouble());
		Assert.Equal(650, data.GetProperty("size").GetProperty("height").GetDouble());
		if (observesPosition)
		{
			Assert.Equal(40, data.GetProperty("position").GetProperty("x").GetDouble());
			Assert.Equal(50, data.GetProperty("position").GetProperty("y").GetDouble());
		}
		else
			Assert.Equal(JsonValueKind.Null, data.GetProperty("position").ValueKind);
		((IWindow)context.Window).FrameChanged(new Microsoft.Maui.Graphics.Rect(70, 80, 1000, 700));
		await Call("core.unsubscribe", "{\"topic\":\"window.geometryChanged\"}");
		clock.Advance(TimeSpan.FromMilliseconds(20));
		Assert.Single(received);
		await Call("core.subscribe", "{\"topic\":\"window.geometryChanged\"}");
		((IWindow)context.Window).FrameChanged(new Microsoft.Maui.Graphics.Rect(90, 100, 1100, 750));
		clock.Advance(TimeSpan.FromMilliseconds(20));
		Assert.Equal(2, received.Count);
		((IWindow)context.Window).FrameChanged(new Microsoft.Maui.Graphics.Rect(110, 120, 1200, 800));
		hub.Dispose();
		clock.Advance(TimeSpan.FromMilliseconds(20));
		Assert.Equal(2, received.Count);
	}

	[Fact]
	public async Task GeometryNotificationDoesNotHoldTheSourceLockWhileEnteringTheHub()
	{
		using var context = Bind();
		using var notifications = new NativeNotifications();
		var clock = new InteractivePolicyTests.ManualTime();
		using var provider = new WindowProvider(context, new HybridWindowHost(), new WindowDevice("WinUI", false), Inline, notifications, clock);
		using var hub = TestComposition.CreateHub([provider], new HybridAppOptions("geometry."), time: clock, notifications: notifications);
		var session = Guid.NewGuid().ToString("D");
		async Task Call(string command, string args = "{}")
		{
			using var document = JsonDocument.Parse(args);
			var request = new Request(1, session, Guid.NewGuid().ToString("D"), command, document.RootElement);
			var response = JsonSerializer.Deserialize(await hub.Invoke(JsonSerializer.Serialize(request, ProtocolJsonContext.Default.Request)),
				ProtocolJsonContext.Default.Response)!;
			Assert.Equal("success", response.Status);
		}
		await Call("core.connect");
		await Call("core.subscribe", "{\"topic\":\"window.geometryChanged\"}");
		var disposalCompletedDuringNotification = false;
		Task? disposal = null;
		hub.NativeEventReceived += _ =>
		{
			disposal = Task.Run(provider.Dispose);
			disposalCompletedDuringNotification = disposal.Wait(TimeSpan.FromSeconds(2));
		};
		((IWindow)context.Window).FrameChanged(new Microsoft.Maui.Graphics.Rect(20, 30, 800, 600));
		clock.Advance(TimeSpan.FromMilliseconds(16));
		Assert.NotNull(disposal);
		await disposal;
		Assert.True(disposalCompletedDuringNotification, "Native delivery must not hold the provider gate while the hub acquires its gate.");
	}

	[Fact]
	public async Task DesktopMenusRenderJsDescriptionsAndUpdateOnlyOwningInstallation()
	{
		using var context = new HybridViewContext();
		using var notifications = new NativeNotifications();
		using var menus = new MenuProvider(context, new WindowDevice("MacCatalyst", false), notifications);
		var page = new ContentPage();
		context.Bind(page, new Window(new NavigationPage(page)), "welcome");
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(menus);
		var definitions = new[] { new { id = "navigation", text = "Navigate", kind = "submenu", enabled = true, @checked = false,
			items = new[] { new { id = "item", text = "JS item", kind = "item", enabled = true, @checked = false,
				shortcut = new { key = "Enter", modifiers = new[] { "cmd", "shift", "alt" } }, items = (object?)null } } } };
		var installed = await Dispatch(registry, "menus.install", JsonSerializer.Serialize(new { windowId = context.Id, items = definitions }));
		Assert.Equal("Navigate", Assert.Single(page.MenuBarItems).Text);
		var sourceId = installed.GetProperty("sourceId").GetString();
		var accelerator = Assert.Single(((MenuFlyoutItem)page.MenuBarItems[0][0]).KeyboardAccelerators);
		Assert.Equal("Enter", accelerator.Key);
		Assert.Equal(KeyboardAcceleratorModifiers.Cmd | KeyboardAcceleratorModifiers.Shift | KeyboardAcceleratorModifiers.Alt, accelerator.Modifiers);
		var invalidDefinitions = new[] { definitions[0], definitions[0] };
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "menus.install", JsonSerializer.Serialize(new { windowId = context.Id, items = invalidDefinitions })))).Code);
		Assert.Equal(sourceId, (await Dispatch(registry, "menus.info", JsonSerializer.Serialize(new { windowId = context.Id }))).GetProperty("sourceId").GetString());
		Assert.Single(page.MenuBarItems);
		await Dispatch(registry, "menus.update", JsonSerializer.Serialize(new { windowId = context.Id, sourceId, id = "item", text = "JS updated", enabled = false }));
		var native = (MenuFlyoutItem)page.MenuBarItems[0][0];
		Assert.Equal("JS updated", native.Text);
		Assert.False(native.IsEnabled);
		var current = await Dispatch(registry, "menus.item", JsonSerializer.Serialize(new { windowId = context.Id, sourceId, id = "item" }));
		Assert.Equal("JS updated", current.GetProperty("text").GetString());
		Assert.False(current.GetProperty("enabled").GetBoolean());
		Assert.Equal(JsonValueKind.Null, current.GetProperty("checked").ValueKind);
		Assert.Same(accelerator, Assert.Single(native.KeyboardAccelerators));
		Assert.Equal("invalid_menu", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "menus.update", JsonSerializer.Serialize(new { windowId = context.Id, sourceId = "stale", id = "item", text = "bad" })))).Code);
		Assert.Equal("invalid_window", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "menus.info", "{\"windowId\":\"foreign\"}"))).Code);
		var replacement = await Dispatch(registry, "menus.install", JsonSerializer.Serialize(new { windowId = context.Id, items = definitions }));
		Assert.NotEqual(sourceId, replacement.GetProperty("sourceId").GetString());
		Assert.Equal("invalid_menu", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "menus.item", JsonSerializer.Serialize(new { windowId = context.Id, sourceId, id = "item" })))).Code);
		menus.Dispose();
		Assert.Empty(page.MenuBarItems);
	}

	[Theory]
	[InlineData("MacCatalyst", true)]
	[InlineData("WinUI", false)]
	public async Task DesktopMenuLabelUpdatesKeepResourcesAndInvalidateTheOwningAppleMenu(string platform, bool rebuilds)
	{
		using var context = Bind();
		using var notifications = new NativeNotifications();
		using var menus = new MenuProvider(context, new WindowDevice(platform, false), notifications);
		var registry = new CommandRegistry();
		var provider = menus;
		registry = TestComposition.CreateRegistry(provider);
		using var hub = TestComposition.CreateHub([provider], new HybridAppOptions("menus."), notifications: notifications);
		var events = await SubscribeMenuClicks(hub);
		var definitions = new[] { new { id = "bar", text = "Original bar", kind = "submenu", enabled = true, @checked = false,
			items = new[] { new { id = "group", text = "Original group", kind = "submenu", enabled = true, @checked = false,
				items = new[] { new { id = "item", text = "Original item", kind = "item", enabled = true, @checked = false,
					shortcut = new { key = "r", modifiers = new[] { "primary", "shift" } }, items = (object?)null } } } } } };
		var installed = await Dispatch(registry, "menus.install", JsonSerializer.Serialize(new { windowId = context.Id, items = definitions }));
		var sourceId = installed.GetProperty("sourceId").GetString();
		var bar = context.Page.MenuBarItems[0];
		var group = (MenuFlyoutSubItem)bar[0];
		var item = (MenuFlyoutItem)group[0];
		var shortcut = Assert.Single(item.KeyboardAccelerators);
		var handler = new MenuWindowHandler();
		context.Window.Handler = handler;
		handler.Rebuilds = 0;

		foreach (var id in new[] { "item", "group", "bar" })
			await Dispatch(registry, "menus.update", JsonSerializer.Serialize(new
			{
				windowId = context.Id, sourceId, id, text = $"Renamed {id}"
			}));
		Assert.Equal(rebuilds ? 3 : 0, handler.Rebuilds);
		Assert.Equal("Renamed item", item.Text);
		Assert.Equal("Renamed group", group.Text);
		Assert.Equal("Renamed bar", bar.Text);
		Assert.Same(bar, context.Page.MenuBarItems[0]);
		Assert.Same(group, bar[0]);
		Assert.Same(item, group[0]);
		Assert.Same(shortcut, Assert.Single(item.KeyboardAccelerators));
		Assert.Equal(sourceId, (await Dispatch(registry, "menus.info",
			JsonSerializer.Serialize(new { windowId = context.Id }))).GetProperty("sourceId").GetString());
		Assert.Equal("Renamed item", (await Dispatch(registry, "menus.item",
			JsonSerializer.Serialize(new { windowId = context.Id, sourceId, id = "item" }))).GetProperty("text").GetString());

		((IMenuElement)item).Clicked();
		Assert.Equal(sourceId, Assert.Single(events).GetProperty("sourceId").GetString());
		Assert.Equal("item", events[0].GetProperty("id").GetString());
		Assert.Equal("invalid_window", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "menus.update", JsonSerializer.Serialize(new
			{
				windowId = "foreign", sourceId, id = "item", text = "Wrong window"
			})))).Code);
		Assert.Equal("invalid_menu", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "menus.update", JsonSerializer.Serialize(new
			{
				windowId = context.Id, sourceId = "retired", id = "item", text = "Wrong source"
			})))).Code);
		Assert.Equal(rebuilds ? 3 : 0, handler.Rebuilds);
	}

	[Fact]
	public async Task DesktopMenuCheckUpdatesAndClicksRebuildWithoutAttachedItemHandlers()
	{
		using var context = Bind();
		using var notifications = new NativeNotifications();
		using var menus = new MenuProvider(context, new WindowDevice("MacCatalyst", false), notifications);
		var registry = new CommandRegistry();
		var provider = menus;
		registry = TestComposition.CreateRegistry(provider);
		using var hub = TestComposition.CreateHub([provider], new HybridAppOptions("menus."), notifications: notifications);
		var events = await SubscribeMenuClicks(hub);
		var definitions = new[] { new { id = "bar", text = "View", kind = "submenu", enabled = true, @checked = false,
			items = new[] { new { id = "check", text = "Checked item", kind = "check", enabled = true, @checked = true,
				shortcut = new { key = "k", modifiers = new[] { "primary" } }, items = (object?)null } } } };
		var installed = await Dispatch(registry, "menus.install", JsonSerializer.Serialize(new { windowId = context.Id, items = definitions }));
		var sourceId = installed.GetProperty("sourceId").GetString();
		var item = (MenuFlyoutItem)context.Page.MenuBarItems[0][0];
		var shortcut = Assert.Single(item.KeyboardAccelerators);
		var handler = new MenuWindowHandler();
		context.Window.Handler = handler;
		handler.Rebuilds = 0;
		foreach (var value in new[] { false, true, false })
		{
			Assert.Null(item.Handler); // Apple handlers exist only during BuildMenu.
			await Dispatch(registry, "menus.update", JsonSerializer.Serialize(new
			{
				windowId = context.Id, sourceId, id = "check", @checked = value
			}));
		}
		Assert.Equal(3, handler.Rebuilds);
		((IMenuElement)item).Clicked();
		((IMenuElement)item).Clicked();
		Assert.Equal(new[] { true, false }, events.Select(value => value.GetProperty("checked").GetBoolean()));
		Assert.All(events, value => Assert.Equal(sourceId, value.GetProperty("sourceId").GetString()));
		Assert.Equal(5, handler.Rebuilds);
		Assert.Equal(sourceId, (await Dispatch(registry, "menus.info",
			JsonSerializer.Serialize(new { windowId = context.Id }))).GetProperty("sourceId").GetString());
		Assert.Same(item, context.Page.MenuBarItems[0][0]);
		Assert.Same(shortcut, Assert.Single(item.KeyboardAccelerators));

		await Dispatch(registry, "menus.update", JsonSerializer.Serialize(new
		{
			windowId = context.Id, sourceId, id = "check", enabled = false
		}));
		((IMenuElement)item).Clicked();
		Assert.Equal(2, events.Count);
		Assert.Equal(6, handler.Rebuilds);
		var replacement = await Dispatch(registry, "menus.install", JsonSerializer.Serialize(new { windowId = context.Id, items = definitions }));
		var replacementSourceId = replacement.GetProperty("sourceId").GetString();
		Assert.NotEqual(sourceId, replacementSourceId);
		handler.Rebuilds = 0;
		((IMenuElement)item).Clicked();
		var liveItem = (MenuFlyoutItem)context.Page.MenuBarItems[0][0];
		((IMenuElement)liveItem).Clicked();
		Assert.Equal(3, events.Count);
		Assert.Equal(replacementSourceId, events[^1].GetProperty("sourceId").GetString());
		Assert.False(events[^1].GetProperty("checked").GetBoolean());
		Assert.Equal(1, handler.Rebuilds);
		await Dispatch(registry, "menus.clear", JsonSerializer.Serialize(new { windowId = context.Id, sourceId = replacementSourceId }));
		handler.Rebuilds = 0;
		((IMenuElement)liveItem).Clicked();
		Assert.Equal(3, events.Count);
		Assert.Equal(0, handler.Rebuilds);
	}

	static async Task<List<JsonElement>> SubscribeMenuClicks(HybridAppTargetObject hub)
	{
		var session = Guid.NewGuid().ToString("D");
		foreach (var (command, args) in new[] { ("core.connect", "{}"), ("core.subscribe", "{\"topic\":\"menus.clicked\"}") })
		{
			using var document = JsonDocument.Parse(args);
			var request = new Request(1, session, Guid.NewGuid().ToString("D"), command, document.RootElement);
			var response = JsonSerializer.Deserialize(await hub.Invoke(JsonSerializer.Serialize(request, ProtocolJsonContext.Default.Request)),
				ProtocolJsonContext.Default.Response)!;
			Assert.Equal("success", response.Status);
		}
		var events = new List<JsonElement>();
		hub.NativeEventReceived += value => events.Add(value.Data);
		return events;
	}

	sealed class MenuWindowHandler : Microsoft.Maui.Handlers.ElementHandler<IWindow, object>
	{
		public int Rebuilds { get; set; }
		public MenuWindowHandler() : base(new PropertyMapper<IWindow, MenuWindowHandler>(ElementMapper)
		{
			[nameof(IMenuBarElement.MenuBar)] = (handler, _) => handler.Rebuilds++
		}) { }
		protected override object CreatePlatformElement() => new();
	}

	[Fact]
	public async Task NativeDeveloperWindowIdsAreUniqueAndClosedGenerationsCanBeReused()
	{
		var host = new HybridWindowHost();
		using var first = new HybridViewContext();
		var firstPage = new ContentPage();
		first.Bind(firstPage, new Window(firstPage), "index.html#/settings", "settings");
		host.Track(first);
		using var duplicate = new HybridViewContext();
		var duplicatePage = new ContentPage();
		duplicate.Bind(duplicatePage, new Window(duplicatePage), "index.html", "settings");
		Assert.Equal("duplicate_window_id", Assert.Throws<CommandException>(() => host.Track(duplicate)).Code);
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(WindowPlugin(first, host, new WindowDevice("MacCatalyst", false)));
		var before = await Dispatch(registry, "window.info");
		Assert.Equal("invalid_window", (await Assert.ThrowsAsync<CommandException>(() => Dispatch(registry, "window.title",
			"{\"windowId\":\"settings\",\"windowLease\":\"stale\",\"title\":\"Wrong generation\"}"))).Code);
		await Dispatch(registry, "window.title", "{\"title\":\"Renamed\"}");
		Assert.Equal("settings", first.Id);
		((IWindow)first.Window).Destroying();
		host.Track(duplicate);
		var replacementRegistry = new CommandRegistry();
		replacementRegistry = TestComposition.CreateRegistry(WindowPlugin(duplicate, host, new WindowDevice("MacCatalyst", false)));
		var after = await Dispatch(replacementRegistry, "window.info");
		Assert.NotEqual(before.GetProperty("windowLease").GetString(), after.GetProperty("windowLease").GetString());
	}

	sealed class CancelledPicker : IFilePicker
	{
		public int Calls { get; private set; }
		public Task<FileResult?> PickAsync(PickOptions? options = null)
		{
			Calls++;
			Assert.Equal("From TypeScript", options?.PickerTitle);
			return Task.FromResult<FileResult?>(null);
		}
		public Task<IEnumerable<FileResult>?> PickMultipleAsync(PickOptions? options = null) => throw new NotSupportedException();
	}
	sealed class ReadOnlyBattery(double level = 0.42) : IBattery
	{
		public double ChargeLevel => level;
		public BatteryState State => BatteryState.Charging;
		public BatteryPowerSource PowerSource => BatteryPowerSource.AC;
		public EnergySaverStatus EnergySaverStatus => EnergySaverStatus.Off;
		public event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChanged { add => throw new InvalidOperationException(); remove => throw new InvalidOperationException(); }
		public event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged { add => throw new InvalidOperationException(); remove => throw new InvalidOperationException(); }
	}
	sealed class ReadOnlyDisplay : IDeviceDisplay
	{
		public bool KeepScreenOn { get => false; set => throw new InvalidOperationException(); }
		public DisplayInfo MainDisplayInfo => new(1080, 1920, 3, DisplayOrientation.Portrait, DisplayRotation.Rotation0, 60);
		public event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged { add => throw new InvalidOperationException(); remove => throw new InvalidOperationException(); }
	}
	sealed class WindowDevice(string platform, bool tablet) : IDeviceInfo
	{
		public string Model => "Unit test metadata";
		public string VersionString => "27";
		public DevicePlatform Platform => DevicePlatform.Create(platform);
		public DeviceIdiom Idiom => tablet ? DeviceIdiom.Tablet : DeviceIdiom.Phone;
		public string Manufacturer => throw new NotSupportedException();
		public string Name => throw new NotSupportedException();
		public Version Version => throw new NotSupportedException();
		public DeviceType DeviceType => throw new NotSupportedException();
	}
}
