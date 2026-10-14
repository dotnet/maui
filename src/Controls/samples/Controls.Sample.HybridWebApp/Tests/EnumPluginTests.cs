using System.Text.Json;
using Maui.HybridApp;
using Maui.HybridApp.UI;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class EnumPluginTests
{
	static readonly NativeDispatcher Inline = new(action => action());
	static async Task<Response> Call(HybridAppTargetObject hub, string session, string command, string args = "{}")
	{
		using var json = JsonDocument.Parse(args);
		var request = new Request(1, session, Guid.NewGuid().ToString("D"), command, json.RootElement);
		return JsonSerializer.Deserialize(await hub.Invoke(JsonSerializer.Serialize(request, ProtocolJsonContext.Default.Request)),
			ProtocolJsonContext.Default.Response)!;
	}

	[Theory]
	[InlineData("dialogs.alert", """{"title":"t","message":"m","flowDirection":1}""")]
	[InlineData("dialogs.confirm", """{"title":"t","message":"m","flowDirection":"unknown"}""")]
	[InlineData("dialogs.prompt", """{"title":"t","message":"m","keyboard":0}""")]
	[InlineData("dialogs.prompt", """{"title":"t","message":"m","keyboard":"custom","keyboardFlags":3}""")]
	[InlineData("dialogs.prompt", """{"title":"t","message":"m","keyboard":"custom","keyboardFlags":["all","spellcheck"]}""")]
	[InlineData("dialogs.prompt", """{"title":"t","message":"m","keyboard":"custom","keyboardFlags":["none","spellcheck"]}""")]
	[InlineData("dialogs.prompt", """{"title":"t","message":"m","keyboard":"custom","keyboardFlags":["spellcheck","spellcheck"]}""")]
	[InlineData("application.setTheme", """{"theme":"system"}""")]
	[InlineData("application.setTheme", """{"theme":1}""")]
	[InlineData("remote.control", """{"id":"guest","action":"unknown"}""")]
	[InlineData("remote.control", """{"id":"guest","action":1}""")]
	[InlineData("menus.install", """{"windowId":"unused","items":[{"id":"menu","text":"Menu","kind":"invalid","enabled":true,"checked":false,"items":null}]}""")]
	[InlineData("menus.install", """{"windowId":"unused","items":[{"id":"menu","text":"Menu","kind":"submenu","enabled":true,"checked":false,"items":null,"shortcut":{"key":"K","modifiers":["invalid"]}}]}""")]
	public async Task UntrustedWireEnumsAreRejectedBeforeResolvingAnyNativeContext(string command, string args)
	{
		using var context = new HybridViewContext();
		using var notifications = new NativeNotifications();
		using var menus = new MenuProvider(context, new FakeDeviceInfo(), notifications);
		using var theme = new ApplicationProvider(new HybridWindowHost(), Inline, notifications);
		var registry = TestComposition.CreateRegistry(new DialogProvider(context), theme, menus,
			new RemoteWebViewProvider(context, new FakeDeviceInfo(), notifications));
		using var json = JsonDocument.Parse(args);
		var error = await Assert.ThrowsAsync<CommandException>(() => registry.DispatchAsync(command, json.RootElement,
			new Operation("session", "request", default, (_, _) => { })));
		Assert.Equal("invalid_arguments", error.Code);
	}

	[Fact]
	public async Task ApplicationThemeResultsAndActualChangeEventsUseIdenticalNativeEnumNames()
	{
		var application = new Application();
		var host = new HybridWindowHost();
		host.Bind(application, _ => throw new InvalidOperationException("No windows should be opened."));
		using var notifications = new NativeNotifications();
		using var plugin = new ApplicationProvider(host, Inline, notifications);
		using var hub = TestComposition.CreateHub([plugin], new HybridAppOptions("test."), notifications: notifications);
		var session = Guid.NewGuid().ToString("D");
		var events = new List<JsonElement>();
		hub.NativeEventReceived += value => events.Add(value.Data);
		await Call(hub, session, "core.connect");
		Assert.Equal("success", (await Call(hub, session, "core.subscribe", """{"topic":"application.themeChanged"}""")).Status);
		foreach (var theme in new[] { "dark", "light", "unspecified" })
		{
			var response = await Call(hub, session, "application.setTheme", $$"""{"theme":"{{theme}}"}""");
			Assert.Equal("success", response.Status);
			Assert.Equal(theme, response.Result!.Value.GetProperty("preference").GetString());
			if (theme != "unspecified") Assert.Equal(theme, response.Result.Value.GetProperty("effective").GetString());
			Assert.Equal(response.Result.Value.GetRawText(), events.Last().GetRawText());
		}
		Assert.Equal(AppTheme.Unspecified, application.UserAppTheme);
	}

	[Fact]
	public async Task NativeBrowserCallbacksUseTypedStatusAndCompletionWithoutEmbeddingErrorText()
	{
		using var context = new HybridViewContext();
		var page = new ContentPage();
		context.Bind(page, new Window(page), "index.html");
		var layout = context.CreateLayout(new HybridWebView());
		using var notifications = new NativeNotifications();
		var plugin = new RemoteWebViewProvider(context, new Desktop(), notifications);
		using var hub = TestComposition.CreateHub([plugin], new HybridAppOptions("test."), notifications: notifications);
		var session = Guid.NewGuid().ToString("D");
		var events = new List<JsonElement>();
		hub.NativeEventReceived += value => events.Add(value.Data);
		await Call(hub, session, "core.connect");
		await Call(hub, session, "core.subscribe", """{"topic":"remote.changed"}""");
		var opened = await Call(hub, session, "remote.open", """{"url":"https://dot.net/maui"}""");
		Assert.Equal("loading", opened.Result!.Value.GetProperty("status").GetString());
		var web = Assert.Single(((AbsoluteLayout)layout.Children[1]).Children.OfType<WebView>());
		((IWebView)web).Navigated(WebNavigationEvent.NewPage, "https://dot.net/maui", WebNavigationResult.Timeout);
		var info = await Call(hub, session, "remote.info");
		Assert.Equal("failed", info.Result!.Value.GetProperty("status").GetString());
		Assert.Equal("timeout", info.Result.Value.GetProperty("navigationResult").GetString());
		Assert.Equal(JsonValueKind.Null, info.Result.Value.GetProperty("error").ValueKind);
		Assert.Equal(info.Result.Value.GetRawText(), events.Last().GetRawText());
		Assert.True(((IWebView)web).Navigating(WebNavigationEvent.NewPage, "http://localhost"));
		((IWebView)web).Navigated(WebNavigationEvent.NewPage, "http://localhost", WebNavigationResult.Cancel);
		info = await Call(hub, session, "remote.info");
		Assert.Equal("blocked", info.Result!.Value.GetProperty("status").GetString());
		Assert.NotEmpty(info.Result.Value.GetProperty("error").GetString()!);
		Assert.Equal(JsonValueKind.Null, info.Result.Value.GetProperty("navigationResult").ValueKind);
		Assert.Equal(info.Result.Value.GetRawText(), events.Last().GetRawText());
	}

	sealed class Desktop : Microsoft.Maui.Devices.IDeviceInfo
	{
		public string Model => "Test";
		public string Manufacturer => "Test";
		public string Name => "Test";
		public string VersionString => "1";
		public Version Version => new(1, 0);
		public Microsoft.Maui.Devices.DevicePlatform Platform => Microsoft.Maui.Devices.DevicePlatform.MacCatalyst;
		public Microsoft.Maui.Devices.DeviceIdiom Idiom => Microsoft.Maui.Devices.DeviceIdiom.Desktop;
		public Microsoft.Maui.Devices.DeviceType DeviceType => Microsoft.Maui.Devices.DeviceType.Virtual;
	}
}
