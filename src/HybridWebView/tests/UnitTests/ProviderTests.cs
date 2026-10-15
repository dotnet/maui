using System.Text.Json;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.HybridWebApps;
using Microsoft.Maui.Controls.HybridWebApps.Essentials.AppInfo;
using Microsoft.Maui.Controls.HybridWebApps.Essentials.DeviceInfo;
using Microsoft.Maui.Controls.HybridWebApps.Essentials.Preferences;
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
		Assert.Null(ProtocolJsonContext.Default.GetTypeInfo(typeof(CommandRegistry).Assembly.GetType("Microsoft.Maui.Controls.HybridWebApps.Essentials.DeviceInfo.DeviceInfoResult")!));
	}

	[Theory]
	[InlineData("clipboard.write", "{}")]
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
		registry = TestComposition.CreateRegistryWithOptions(new(), clipboard, Inline,
			new PreferencesProvider(preferences),
			new AppInfoProvider(new FakeAppInfo { Error = new InvalidOperationException("Must not run") }), new DeviceInfoProvider(new FakeDeviceInfo()));
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() => Dispatch(registry, command, args))).Code);
		Assert.Equal(0, clipboard.Calls);
		Assert.Equal(0, preferences.Calls);
	}

	[Fact]
	public async Task PreferencesUseExistingNativeKeysWithoutTextKeyOrOwnershipBudgets()
	{
		var preferences = new FakePreferences();
		preferences.Values["native.existing"] = "from native";
		for (var i = 0; i < 4096; i++)
			preferences.Values[$"native{i}"] = i;
		var registry = TestComposition.CreateRegistryWithOptions(new(4), new PreferencesProvider(preferences));
		Assert.True((await Dispatch(registry, "preferences.contains", """{"key":"native.existing"}""")).GetProperty("accepted").GetBoolean());
		Assert.Equal("from native", (await Dispatch(registry, "preferences.get", """{"key":"native.existing","defaultValue":{"kind":"string","text":""}}""")).GetProperty("text").GetString());
		var text = new string('語', 8192);
		foreach (var key in new[] { "", "key with spaces", "a.b", "a/b", "é 👋", new string('a', 1024) })
		{
			await Dispatch(registry, "preferences.set", JsonSerializer.Serialize(new { key, value = new { kind = "string", text } }));
			Assert.Equal(text, preferences.Values[key]);
			Assert.Equal(text, (await Dispatch(registry, "preferences.get", JsonSerializer.Serialize(new { key, defaultValue = new { kind = "string", text = "" } }))).GetProperty("text").GetString());
			await Dispatch(registry, "preferences.remove", JsonSerializer.Serialize(new { key }));
			Assert.False(preferences.Values.ContainsKey(key));
		}
		Assert.DoesNotContain(preferences.Values.Keys, key => key.Contains("@owned-keys", StringComparison.Ordinal));
		Assert.Throws<ArgumentNullException>(() => new PreferencesProvider(null!));
	}

	[Fact]
	public async Task ClipboardRetainsItsIndependentTextBudgetInBothDirections()
	{
		var clipboardRegistry = new CommandRegistry();
		var clipboard = new FakeClipboard();
		clipboardRegistry = TestComposition.CreateRegistryWithOptions(new(4), clipboard, Inline);
		await Dispatch(clipboardRegistry, "clipboard.write", "{\"text\":\"你好👋\"}");
		Assert.Equal("你好👋", clipboard.Text);
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(clipboardRegistry, "clipboard.write", "{\"text\":\"12345\"}"))).Code);
		clipboard.Text = "12345";
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Dispatch(clipboardRegistry, "clipboard.read", "{}"))).Code);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("native shared.語\n")]
	public async Task PreferencesPassContainersUnchangedAndClearTheActualNativeStore(string? sharedName)
	{
		var preferences = new FakePreferences();
		preferences.Store(sharedName)["existing"] = "native";
		preferences.Store("other")["keep"] = "different container";
		var registry = TestComposition.CreateRegistry(new PreferencesProvider(preferences));
		var keyArgs = JsonSerializer.Serialize(new { key = "existing", sharedName });
		Assert.True((await Dispatch(registry, "preferences.contains", keyArgs)).GetProperty("accepted").GetBoolean());
		Assert.Equal(sharedName, preferences.LastSharedName);
		Assert.Equal("native", (await Dispatch(registry, "preferences.get", JsonSerializer.Serialize(new { key = "existing", sharedName, defaultValue = new { kind = "string", text = "" } }))).GetProperty("text").GetString());
		Assert.Equal(sharedName, preferences.LastSharedName);
		await Dispatch(registry, "preferences.set", JsonSerializer.Serialize(new { key = "new key", sharedName, value = new { kind = "string", text = "written" } }));
		Assert.Equal(sharedName, preferences.LastSharedName);
		Assert.Equal("written", preferences.Store(sharedName)["new key"]);
		await Dispatch(registry, "preferences.remove", JsonSerializer.Serialize(new { key = "new key", sharedName }));
		Assert.Equal(sharedName, preferences.LastSharedName);
		Assert.False(preferences.Store(sharedName).ContainsKey("new key"));
		await Dispatch(registry, "preferences.clear", JsonSerializer.Serialize(new { sharedName }));
		Assert.Equal(sharedName, preferences.LastSharedName);
		Assert.Equal(1, preferences.ClearCalls);
		Assert.Empty(preferences.Store(sharedName));
		Assert.Equal("different container", preferences.Store("other")["keep"]);
	}

	[Theory]
	[InlineData("preferences.set", "{\"key\":\"key\",\"value\":{\"kind\":\"string\",\"text\":\"replacement\"}}", false)]
	[InlineData("preferences.set", "{\"key\":\"key\",\"value\":{\"kind\":\"string\",\"text\":\"replacement\"}}", true)]
	public async Task PreferenceWriteFailuresKeepDestinationSemanticsWithoutRollbackOrIndex(string command, string args, bool partialWrite)
	{
		var preferences = new FakePreferences { FailingSetKey = "key", WriteBeforeFailure = partialWrite };
		preferences.Values["key"] = "original";
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new PreferencesProvider(preferences));
		await Assert.ThrowsAsync<IOException>(() => Dispatch(registry, command, args));
		Assert.Equal(partialWrite ? "replacement" : "original", preferences.Values["key"]);
		Assert.Single(preferences.Values);
		Assert.Equal(1, preferences.Calls);
		preferences.FailingSetKey = null;
		await Dispatch(registry, "preferences.clear", "{}");
		Assert.Empty(preferences.Values);
	}

	[Theory]
	[InlineData("preferences.get", """{"key":"native","defaultValue":{"kind":"string","text":""}}""")]
	[InlineData("preferences.set", """{"key":"native","value":{"kind":"string","text":"value"}}""")]
	[InlineData("preferences.contains", """{"key":"native"}""")]
	[InlineData("preferences.remove", """{"key":"native"}""")]
	[InlineData("preferences.clear", "{}")]
	public async Task PreferenceNativeErrorsAreSurfacedAndTheHubRecovers(string command, string args)
	{
		var failure = new IOException("Native store failure");
		var native = new FakePreferences { Error = failure };
		var registry = TestComposition.CreateRegistry(new PreferencesProvider(native));
		Assert.Same(failure, await Assert.ThrowsAsync<IOException>(() => Dispatch(registry, command, args)));
		using var hub = TestComposition.CreateHub(registry, () => new ReadyResult(registry.Names, 4096), null);
		var session = Guid.NewGuid().ToString("D");
		async Task<Response> Call(string name, string arguments)
		{
			using var json = JsonDocument.Parse(arguments);
			return JsonSerializer.Deserialize(await hub.Invoke(JsonSerializer.Serialize(
				new Request(1, session, Guid.NewGuid().ToString("D"), name, json.RootElement), ProtocolJsonContext.Default.Request)),
				ProtocolJsonContext.Default.Response)!;
		}
		await Call("core.connect", "{}");
		Assert.Equal("native_error", (await Call(command, args)).Error!.Code);
		native.Error = null;
		Assert.Equal("success", (await Call(command, args)).Status);
	}

	[Theory]
	[InlineData("{\"kind\":\"string\"}")]
	[InlineData("{\"kind\":\"boolean\"}")]
	[InlineData("{\"kind\":\"dateTime\",\"text\":\"not a date\"}")]
	[InlineData("{\"kind\":\"float\",\"number\":1e300}")]
	[InlineData("{\"kind\":\"int\",\"integer\":2147483648}")]
	public async Task InvalidPreferenceRepresentationsAreRejectedBeforeTheDestination(string scalar)
	{
		var preferences = new FakePreferences();
		var registry = new CommandRegistry();
		registry = TestComposition.CreateRegistry(new PreferencesProvider(preferences));
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
		registry = TestComposition.CreateRegistryWithOptions(new(), clipboard, boundary);
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Dispatch(registry, "clipboard.write", "{\"text\":\"before\"}", cancel.Token));
		Assert.Equal(0, clipboard.Calls);

		using var after = new CancellationTokenSource();
		clipboard.OnWrite = after.Cancel;
		registry = new CommandRegistry();
		registry = TestComposition.CreateRegistryWithOptions(new(), clipboard, Inline);
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
		registry = TestComposition.CreateRegistryWithOptions(new(), clipboard, Inline);
		using var hub = TestComposition.CreateHub(registry, () => new ReadyResult(registry.Names, 4096), _ => { });
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
				Assert.StartsWith("Microsoft.Maui.Controls.HybridWebApps", type.Namespace, StringComparison.Ordinal);
		});
	}
}
