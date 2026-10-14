using System.Text.Json;
using Maui.HybridApp;
using Maui.HybridApp.Essentials.AppInfo;
using Maui.HybridApp.Essentials.DeviceInfo;
using Maui.HybridApp.Essentials.Clipboard;
using Maui.HybridApp.Essentials.Preferences;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class ProviderTests
{
	static readonly NativeDispatcher Inline = new(action => action());

	static async Task<JsonElement> Dispatch(CommandRegistry registry, string name, string args, CancellationToken cancellation = default)
	{
		using var document = JsonDocument.Parse(args);
		return await registry.DispatchAsync(name, document.RootElement,
			new Operation(Guid.NewGuid().ToString("D"), Guid.NewGuid().ToString("D"), cancellation, (_, _) => { }));
	}

	[Fact]
	public async Task AppAndDeviceProvidersAreIndependentAndReturnOnlyTheirOwnFields()
	{
		var app = TestComposition.CreateRegistry(new AppInfoProvider(new FakeAppInfo()), new DeviceInfoProvider(new FakeDeviceInfo()));
		Assert.Contains("app.info", app.Names);
		var result = await Dispatch(app, "app.info", "{}");
		Assert.Equal(new[] { "appName", "version", "build" }, result.EnumerateObject().Select(p => p.Name));
		Assert.Equal("Test app", result.GetProperty("appName").GetString());
		Assert.Contains("device.info", app.Names);

		var device = app;
		result = await Dispatch(device, "device.info", "{}");
		Assert.Equal(new[] { "platform", "osVersion", "model" }, result.EnumerateObject().Select(p => p.Name));
		Assert.Equal("Android", result.GetProperty("platform").GetString());
		Assert.Contains("app.info", device.Names);
		Assert.Null(ProtocolJsonContext.Default.GetTypeInfo(typeof(CommandRegistry).Assembly.GetType("Maui.HybridApp.Essentials.DeviceInfo.DeviceInfoResult")!));
	}

	[Theory]
	[InlineData("clipboard.write", "{}")]
	[InlineData("clipboard.write", "{\"text\":null}")]
	[InlineData("clipboard.write", "{\"text\":42}")]
	[InlineData("clipboard.write", "{\"text\":\"ok\",\"extra\":true}")]
	[InlineData("clipboard.read", "{\"extra\":true}")]
	[InlineData("preferences.get", "{\"key\":null,\"defaultValue\":{\"kind\":\"string\",\"text\":\"\"}}")]
	[InlineData("preferences.get", "{\"key\":\"ok\",\"extra\":true}")]
	[InlineData("preferences.get", "{\"key\":true}")]
	[InlineData("preferences.set", "{\"key\":\"ok\"}")]
	[InlineData("preferences.set", "{\"key\":\"ok\",\"value\":null}")]
	[InlineData("preferences.set", """{"key":"ok","value":{"kind":0,"text":"not written"}}""")]
	[InlineData("preferences.set", """{"key":"ok","value":{"kind":"unknown","text":"not written"}}""")]
	[InlineData("preferences.set", """{"key":"ok","value":{"kind":"String","text":"not written"}}""")]
	[InlineData("preferences.remove", "null")]
	[InlineData("app.info", "null")]
	[InlineData("app.info", "{\"extra\":true}")]
	[InlineData("device.info", "{\"extra\":true}")]
	public async Task FeatureGeneratedMetadataRejectsInvalidArgumentsBeforeEssentials(string command, string args)
	{
		var registry = new CommandRegistry();
		var clipboard = new FakeClipboard();
		var preferences = new FakePreferences();
		registry = TestComposition.CreateRegistry(new ClipboardProvider(clipboard, Inline, new("host."), new()),
			new PreferencesProvider(preferences, new("host.")),
			new AppInfoProvider(new FakeAppInfo { Error = new InvalidOperationException("Must not run") }), new DeviceInfoProvider(new FakeDeviceInfo()));
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() => Dispatch(registry, command, args))).Code);
		Assert.Equal(0, clipboard.Calls);
		Assert.Equal(0, preferences.Calls);
	}

	[Fact]
	public async Task HostNamespaceIsIsolatedAndTextLimitsApplyInBothDirections()
	{
		var registry = new CommandRegistry();
		var preferences = new FakePreferences();
		preferences.Values["other.key"] = "untouched";
		registry = TestComposition.CreateRegistry(new PreferencesProvider(preferences, new("host.", 4)));
		await Dispatch(registry, "preferences.set", "{\"key\":\"key\",\"value\":{\"kind\":\"string\",\"text\":\"你好👋\"}}");
		Assert.Equal("你好👋", preferences.Values["host.key"]);
		Assert.Equal("untouched", preferences.Values["other.key"]);
		Assert.Equal("你好👋", (await Dispatch(registry, "preferences.get", "{\"key\":\"key\",\"defaultValue\":{\"kind\":\"string\",\"text\":\"\"}}")).GetProperty("text").GetString());
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "preferences.set", "{\"key\":\"key\",\"value\":{\"kind\":\"string\",\"text\":\"12345\"}}"))).Code);
		preferences.Values["host.key"] = "12345";
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "preferences.get", "{\"key\":\"key\",\"defaultValue\":{\"kind\":\"string\",\"text\":\"\"}}"))).Code);
		await Dispatch(registry, "preferences.remove", "{\"key\":\"key\"}");
		Assert.False(preferences.Values.ContainsKey("host.key"));
		Assert.Equal("untouched", preferences.Values["other.key"]);
		foreach (var key in new[] { "", "a.b", "a/b", "é", new string('a', 49) })
			Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
				Dispatch(registry, "preferences.contains", JsonSerializer.Serialize(new { key })))).Code);
		await Dispatch(registry, "preferences.set", JsonSerializer.Serialize(new { key = new string('a', 48), value = new { kind = "string", text = "" } }));
		Assert.Throws<ArgumentException>(() => new PreferencesProvider(preferences, new("")));
		Assert.Throws<ArgumentOutOfRangeException>(() => new PreferencesProvider(preferences, new("host.", 0)));

		var clipboardRegistry = new CommandRegistry();
		var clipboard = new FakeClipboard();
		clipboardRegistry = TestComposition.CreateRegistry(new ClipboardProvider(clipboard, Inline, new("host.", 4), new()));
		await Dispatch(clipboardRegistry, "clipboard.write", "{\"text\":\"你好👋\"}");
		Assert.Equal("你好👋", clipboard.Text);
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(clipboardRegistry, "clipboard.write", "{\"text\":\"12345\"}"))).Code);
		clipboard.Text = "12345";
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(clipboardRegistry, "clipboard.read", "{}"))).Code);
	}

	[Theory]
	[InlineData("preferences.set", "{\"key\":\"overflow\",\"value\":{\"kind\":\"string\",\"text\":\"rejected\"}}")]
	public async Task PreferenceCapacityRejectsBeforeWritingAndRemovalReleasesOwnership(string command, string args)
	{
		var preferences = new FakePreferences();
		var keys = Enumerable.Range(0, 4096).Select(index => $"key{index}").ToArray();
		preferences.Values["host.@owned-keys"] = JsonSerializer.Serialize(keys);
		foreach (var key in keys)
			preferences.Values["host." + key] = "owned";
		preferences.Values["other.key"] = "unrelated";
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new PreferencesProvider(preferences, new("host.")));

		Assert.Equal("resource_limit", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, command, args))).Code);
		Assert.False(preferences.Values.ContainsKey("host.overflow"));
		await Dispatch(registry, "preferences.remove", "{\"key\":\"key0\"}");
		await Dispatch(registry, command, args);
		Assert.Equal("rejected", preferences.Values["host.overflow"]);
		await Dispatch(registry, "preferences.clear", "{}");
		Assert.Equal(new[] { "other.key" }, preferences.Values.Keys);
	}

	[Fact]
	public async Task PreferenceRemovalReleasesOwnershipCapacity()
	{
		var preferences = new FakePreferences();
		var keys = Enumerable.Range(0, 4096).Select(index => $"key{index}").ToArray();
		preferences.Values["host.@owned-keys"] = JsonSerializer.Serialize(keys);
		preferences.Values["host.key0"] = "owned";
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new PreferencesProvider(preferences, new("host.")));
		await Dispatch(registry, "preferences.remove", "{\"key\":\"key0\"}");
		await Dispatch(registry, "preferences.set", "{\"key\":\"new_key\",\"value\":{\"kind\":\"string\",\"text\":\"new\"}}");
		Assert.Equal("new", preferences.Values["host.new_key"]);
		var tracked = JsonSerializer.Deserialize<string[]>((string)preferences.Values["host.@owned-keys"])!;
		Assert.DoesNotContain("key0", tracked);
		Assert.Contains("new_key", tracked);
	}

	[Theory]
	[InlineData("host.@owned-keys", false)]
	[InlineData("host.key", true)]
	public async Task FailedPreferenceStorageCannotLeaveAnUntrackedValue(string failingKey, bool writeBeforeFailure)
	{
		var preferences = new FakePreferences { FailingSetKey = failingKey, WriteBeforeFailure = writeBeforeFailure };
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new PreferencesProvider(preferences, new("host.")));
		await Assert.ThrowsAsync<IOException>(() =>
			Dispatch(registry, "preferences.set", "{\"key\":\"key\",\"value\":{\"kind\":\"string\",\"text\":\"private\"}}"));
		Assert.False(preferences.Values.ContainsKey("host.key"));
		preferences.FailingSetKey = null;
		await Dispatch(registry, "preferences.set", "{\"key\":\"key\",\"value\":{\"kind\":\"string\",\"text\":\"recovered\"}}");
		await Dispatch(registry, "preferences.clear", "{}");
		Assert.Empty(preferences.Values);
	}

	[Theory]
	[InlineData("preferences.set", "{\"key\":\"key\",\"value\":{\"kind\":\"string\",\"text\":\"replacement\"}}", false)]
	[InlineData("preferences.set", "{\"key\":\"key\",\"value\":{\"kind\":\"string\",\"text\":\"replacement\"}}", true)]
	public async Task FailedUpdatesPreservePreviouslyPersistedValuesWithoutAnOwnershipIndex(string command, string args, bool partialWrite)
	{
		var preferences = new FakePreferences { FailingSetKey = "host.key", WriteBeforeFailure = partialWrite };
		preferences.Values["host.key"] = "original";
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new PreferencesProvider(preferences, new("host.")));
		await Assert.ThrowsAsync<IOException>(() => Dispatch(registry, command, args));
		Assert.Equal(partialWrite ? "replacement" : "original", preferences.Values["host.key"]);
		Assert.Contains("key", JsonSerializer.Deserialize<string[]>((string)preferences.Values["host.@owned-keys"])!);
		preferences.FailingSetKey = null;
		await Dispatch(registry, "preferences.clear", "{}");
		Assert.Empty(preferences.Values);
	}

	[Theory]
	[InlineData("{\"kind\":\"string\",\"text\":\"too long\"}")]
	[InlineData("{\"kind\":\"boolean\"}")]
	[InlineData("{\"kind\":\"dateTime\",\"text\":\"not a date\"}")]
	[InlineData("{\"kind\":\"float\",\"number\":1e300}")]
	[InlineData("{\"kind\":\"int\",\"integer\":2147483648}")]
	public async Task InvalidPreferenceValuesAreValidatedBeforeOwnershipReservation(string scalar)
	{
		var preferences = new FakePreferences();
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new PreferencesProvider(preferences, new("host.", 4)));
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(registry, "preferences.set", "{\"key\":\"key\",\"value\":" + scalar + "}"))).Code);
		Assert.Empty(preferences.Values);
	}

	[Fact]
	public async Task MainThreadBoundaryChecksBeforeSideEffectsAndAfterCompletedWrites()
	{
		using var cancel = new CancellationTokenSource();
		var clipboard = new FakeClipboard();
		var registry = new CommandRegistry();
		var boundary = new NativeDispatcher(action =>
		{
			cancel.Cancel();
			return action();
		});
		registry = TestComposition.CreateRegistry(new ClipboardProvider(clipboard, boundary, new("host."), new()), boundary);
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Dispatch(registry, "clipboard.write", "{\"text\":\"before\"}", cancel.Token));
		Assert.Equal(0, clipboard.Calls);

		using var after = new CancellationTokenSource();
		clipboard.OnWrite = after.Cancel;
		registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new ClipboardProvider(clipboard, Inline, new("host."), new()));
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Dispatch(registry, "clipboard.write", "{\"text\":\"persisted\"}", after.Token));
		Assert.Equal("persisted", clipboard.Text);
		Assert.Equal(1, clipboard.Calls);
	}

	[Theory]
	[InlineData("unsupported")]
	[InlineData("permission_denied")]
	[InlineData("essentials_permission_denied")]
	[InlineData("configuration_required")]
	[InlineData("native_error")]
	public async Task OsFailuresRemainExplicitLoggedErrorsAndHubRecovers(string code)
	{
		var registry = new CommandRegistry();
		var clipboard = new FakeClipboard
		{
			Error = code switch
			{
				"unsupported" => new NotSupportedException(),
				"permission_denied" => new UnauthorizedAccessException(),
				"essentials_permission_denied" => new PermissionException("Sensitive OS permission detail"),
				"configuration_required" => new FeatureNotEnabledException(),
				_ => new InvalidOperationException("Sensitive native detail must not be sent")
			}
		};
		registry = TestComposition.CreateRegistry(new ClipboardProvider(clipboard, Inline, new("host."), new()));
		using var hub = TestComposition.CreateHub(registry, () => new ReadyResult(registry.Names, "", 4096), _ => { });
		var session = Guid.NewGuid().ToString("D");
		async Task<Response> Call(string command) => JsonSerializer.Deserialize(await hub.Invoke(JsonSerializer.Serialize(
			new { version = 1, session, id = Guid.NewGuid().ToString("D"), command, args = new { } })), ProtocolJsonContext.Default.Response)!;
		Assert.Equal("success", (await Call("core.connect")).Status);
		var response = await Call("clipboard.read");
		Assert.Equal("error", response.Status);
		Assert.Equal(code == "essentials_permission_denied" ? "permission_denied" : code, response.Error!.Code);
		Assert.Null(response.Result);
		Assert.DoesNotContain("Sensitive", response.Error.Message, StringComparison.Ordinal);
		clipboard.Error = null;
		Assert.Equal("success", (await Call("clipboard.read")).Status);
	}

	[Fact]
	public void ProductAssemblyContainsNoSampleUiOrFakes()
	{
		var assembly = typeof(CommandRegistry).Assembly;
		Assert.Contains(assembly.GetReferencedAssemblies(), name => name.Name == "Microsoft.Maui.Controls");
		Assert.DoesNotContain(assembly.GetTypes(), type => type.FullName!.Contains("Sample", StringComparison.Ordinal) || type.Name.Contains("Fake", StringComparison.Ordinal) || type.Name is "IdentityResult" or "IPlaygroundPlatform");
		Assert.All(assembly.GetTypes(), type =>
		{
			if (type.Namespace is null)
			{
				Assert.True(type.Name == "<PrivateImplementationDetails>" || type.DeclaringType?.Name == "<PrivateImplementationDetails>");
				Assert.False(type.IsPublic);
			}
			else
				Assert.StartsWith("Maui.HybridApp", type.Namespace, StringComparison.Ordinal);
		});
	}
}
