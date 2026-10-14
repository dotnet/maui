using System.Text.Json;
using System.Text.Json.Serialization;
using HybridApp.ExternalProvider;
using Maui.HybridApp;
using Maui.HybridApp.Generated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;

[assembly: HybridJsonContext(typeof(DevicePlatform), typeof(ConsumerPlatformJsonContext))]

var services = new ServiceCollection();
services.AddHybridApp(new HybridAppOptions("consumer."));
services.AddScoped<PluginActionState>();
services.AddKeyedScoped<PluginActionState>("action-key", (_, _) => new() { Calls = 10 });
services.AddExternalProviderActions();
services.AddPackageConsumerActions();
services.AddScoped<INativeCounterContract>(_ => new ConsumerCounter());
using var root = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
using var hostScope = root.CreateScope();
using var scope = hostScope.ServiceProvider.CreateHybridAppProvider();
var hub = scope.GetRequiredService<HybridAppTargetObject>();
var events = new List<NativeEvent>();
hub.NativeEventReceived += events.Add;
var session = Guid.NewGuid().ToString("D");

async Task<Response> Call(string command, string args, HybridAppTargetObject? target = null, string? documentSession = null)
{
	using var document = JsonDocument.Parse(args);
	var request = new Request(1, documentSession ?? session, Guid.NewGuid().ToString("D"), command, document.RootElement);
	return JsonSerializer.Deserialize(await (target ?? hub).Invoke(JsonSerializer.Serialize(request, ProtocolJsonContext.Default.Request)), ProtocolJsonContext.Default.Response)!;
}

void Check(bool condition, string message)
{
	if (!condition)
		throw new InvalidOperationException(message);
}

Check(!JsonSerializer.IsReflectionEnabledByDefault, "Reflection serialization must be disabled.");
Check(typeof(HybridActionAttribute).Assembly.GetName().Name == "Microsoft.Maui.Essentials", "Metadata must remain in existing Essentials, below the optional runtime.");
Check(typeof(CommandRegistry).Assembly != typeof(PluginActions).Assembly, "The plugin must live in a separate assembly.");
Check(typeof(PluginActions).Assembly != typeof(ConsumerActions).Assembly, "Generated plugin actions must live in a separate provider assembly.");
Check(ProtocolJsonContext.Default.GetTypeInfo(typeof(PluginActionValue)) is null, "Automatic action DTO metadata must remain provider-owned.");
Check(ProtocolJsonContext.Default.GetTypeInfo(typeof(PluginResult)) is null, "Plugin DTOs must not enter global metadata.");
var connect = await Call("core.connect", "{}");
Check(connect.Status == "success", "Connect failed.");
var ready = connect.Result!.Value.Deserialize(ProtocolJsonContext.Default.ReadyResult)!;
Check(ready.PreferenceNamespace == "consumer." && ready.Commands.Contains("app.info", StringComparer.Ordinal) &&
	ready.Commands.Contains("device.info", StringComparer.Ordinal) && ready.Commands.Contains("preferences.set", StringComparer.Ordinal),
	"Product convenience did not register its implemented APIs and actual policy.");
Check(ready.Commands.Contains("window.info", StringComparer.Ordinal) && ready.Commands.Contains("battery.info", StringComparer.Ordinal) &&
	ready.Commands.Contains("display.info", StringComparer.Ordinal) && ready.Commands.Contains("filePicker.pick", StringComparer.Ordinal),
	"UI/device modules did not survive real package consumption.");
Check(ready.InteractiveCommands is not null && ready.InteractiveCommands.Contains("dialogs.prompt", StringComparer.Ordinal) &&
	ready.InteractiveCommands.Contains("filePicker.pick", StringComparer.Ordinal) && ready.InteractiveTimeoutMilliseconds == 300000,
	"Typed interactive handshake policy did not survive packaging.");
Check(typeof(CommandRegistry).Assembly.GetReferencedAssemblies().Any(name => name.Name == "Microsoft.Maui.Controls"),
	"The current WIP product must declare its actual Controls dependency.");
Check((await Call("core.subscribe", "{\"topic\":\"plugin.changed\"}")).Status == "success", "Subscribe failed.");
var response = await Call("plugin.echo", "{\"text\":\"\\u4f60\\u597d \\ud83d\\udc4b\"}");
Check(response.Status == "success" && response.Result!.Value.Deserialize(PluginJsonContext.Default.PluginResult)!.Text == "你好 👋", "Generated plugin JSON failed.");
Check(events.Count == 1 && events[0].Session == session && events[0].Data.Deserialize(PluginJsonContext.Default.PluginResult)!.Text == "你好 👋", "Plugin event failed.");
var action = await Call("plugin.action.echo", """{"value":{"text":"automatic 你好 👋","kind":"second","note":null,"values":[1,2]}}""");
Check(action.Status == "success" && action.Result!.Value.GetProperty("text").GetString() == "automatic 你好 👋" &&
	action.Result.Value.GetProperty("kind").GetString() == "second" &&
	action.Result.Value.GetProperty("note").ValueKind == JsonValueKind.Null &&
	action.Result.Value.GetProperty("values").GetArrayLength() == 2, "Automatic external action DTO metadata failed with reflection disabled.");
Check((await Call("plugin.action.echo", """[{"text":"array","kind":"first"}]""")).Result!.Value.GetProperty("text").GetString() == "array", "Positional external DTO binding failed.");
Check((await Call("plugin.action.count", "{}")).Result!.Value.GetInt32() == 1, "Optional action defaults failed.");
Check((await Call("plugin.action.services", "{}")).Result!.Value.GetInt32() == 11, "Current-scope/keyed action services failed.");
Check((await Call("plugin.action.static", """{"value":"static"}""")).Result!.Value.GetString() == "static", "Static direct call failed.");
Check((await Call("plugin.action.staticClass", "[3]")).Result!.Value.GetInt32() == 6, "Generated static-class opt-in failed.");
Check((await Call("plugin.action.enumDefault", "{}")).Result!.Value.GetString() == "second", "Nullable enum parameter defaults failed.");
Check((await Call("plugin.action.dtoDefault", "[{}]")).Result!.Value.GetProperty("kind").GetString() == "second", "Nullable enum constructor defaults failed.");
Check((await Call("plugin.action.normalized", """[{"text":null}]""")).Result!.Value.GetProperty("text").GetString() == "", "Constructor parameter nullability was confused with its nonnullable property.");
Check((await Call("plugin.action.echo", """{"value":{"text":null,"kind":"first"}}""")).Error?.Code == "invalid_arguments", "Automatic action nullability was not enforced.");
Check((await Call("plugin.action.echo", """{"value":{"text":"x","kind":"first","extra":1}}""")).Error?.Code == "invalid_arguments", "Automatic action DTO unknown members were accepted.");
Check((await Call("plugin.action.echo", """{"value":{"text":"x","kind":0}}""")).Error?.Code == "invalid_arguments", "Numeric enum input was accepted.");
Check((await Call("consumer.action", """{"text":"host","number":7}""")).Result!.Value.GetProperty("text").GetString() == "host", "NuGet analyzer did not generate host action metadata.");
Check((await Call("consumer.action", """["array",7]""")).Result!.Value.GetProperty("number").GetInt32() == 7, "Packed host positional parameters failed.");
Check((await Call("consumer.platform", """{"platform":"CustomDesktop"}""")).Result!.Value.GetString() == "CustomDesktop", "Consumer source context/converter did not preserve an extensible native value.");
Check((await Call("plugin.contract.add", """{"increment":3}""")).Result!.Value.GetInt32() == 3, "Packed consuming assembly did not import its curated referenced interface.");
Check((await Call("plugin.contract.value", "{}")).Result!.Value.GetInt32() == 3, "Packed referenced getter did not call the scoped interface override.");
Check((await Call("plugin.disabled", "{\"text\":\"no\"}")).Error?.Code == "disabled_command", "Disabled policy failed.");
Check((await Call("plugin.echo", "{\"text\":null}")).Error?.Code == "invalid_arguments", "Strict plugin nullability failed.");
Check((await Call("plugin.echo", "{\"text\":\"ok\",\"extra\":true}")).Error?.Code == "invalid_arguments", "Strict plugin schema failed.");
Check((await Call("essentials.notRegistered", "{}")).Error?.Code == "unknown_command", "Unimplemented Essentials commands must not be exposed.");
Check((await Call("sample.echo", "{}")).Error?.Code == "unknown_command", "Sample code must not ship.");
using var secondHostScope = root.CreateScope();
using var secondScope = secondHostScope.ServiceProvider.CreateHybridAppProvider();
var secondHub = secondScope.GetRequiredService<HybridAppTargetObject>();
Check(!ReferenceEquals(hub, secondHub), "The target must be scoped, never shared across WebViews.");
Check(!ReferenceEquals(scope.GetRequiredService<PluginActions>(),
	secondScope.GetRequiredService<PluginActions>()), "Plugin instances must be scoped.");
var secondSession = Guid.NewGuid().ToString("D");
Check((await Call("core.connect", "{}", secondHub, secondSession)).Status == "success", "Second scope connect failed.");
Check((await Call("plugin.echo", "{\"text\":\"second\"}", secondHub, secondSession)).Status == "success", "Second scope plugin failed.");
Check((await Call("plugin.action.count", "{}", secondHub, secondSession)).Result!.Value.GetInt32() == 1, "Generated provider state leaked between scopes.");
Check((await Call("plugin.contract.value", "{}", secondHub, secondSession)).Result!.Value.GetInt32() == 0, "Imported interface overrides must remain owned by the original host scope.");
Check(events.Count == 1 && hub.IsCurrent(session), "A second view changed the first view's session/events.");
using (var emptyArgs = JsonDocument.Parse("{}"))
{
	var metadata = JsonSerializer.Serialize(new Request(1, session, Guid.NewGuid().ToString("D"),
		"consumer.bytes", emptyArgs.RootElement), ProtocolJsonContext.Default.Request);
	var bytes = Enumerable.Range(0, 262144).Select(i => (byte)(i % 256)).ToArray();
	var result = await hub.InvokeBinary(metadata, bytes);
	Check(bytes.SequenceEqual(result), "Packed analyzer binary binding failed.");
}
var channelId = Guid.NewGuid().ToString("D");
var channelWork = Call("consumer.channel", $$"""{"values":"{{channelId}}"}""");
var channelEvent = events[^1];
Check(channelEvent.Topic == "core.channel" && channelEvent.Data.GetProperty("value").GetInt32() == 41,
	"Packed analyzer did not generate typed channel metadata.");
Check((await Call("core.channel.ack", $$"""{"channelId":"{{channelId}}","ownerId":"{{channelEvent.Id}}","sequence":1,"accepted":true}""")).Status == "success",
	"Packed channel consumption acknowledgement failed.");
Check((await channelWork).Result!.Value.GetInt32() == 42 &&
	events[^1].Data.GetProperty("kind").GetString() == "complete", "Packed channel completion failed.");
var registry = scope.GetRequiredService<CommandRegistry>();
try
{
	registry.RegisterTopic("plugin.changed");
	throw new Exception("Duplicate topic allowed.");
}
catch (InvalidOperationException) { }
try
{
	registry.RegisterGenerated("plugin.echo", [PluginJsonContext.Default.PluginResult],
		(_, _) => Task.FromResult(default(JsonElement)));
	throw new Exception("Duplicate command allowed.");
}
catch (InvalidOperationException) { }
Check(!typeof(CommandRegistry).Assembly.GetTypes().Any(type => type.FullName!.Contains("Sample", StringComparison.Ordinal) || type.Name.Contains("Fake", StringComparison.Ordinal)), "App/sample types entered the product.");
scope.Dispose();
Check(!hub.IsCurrent(session), "Scope disposal must retire the native document.");
Check(secondHub.IsCurrent(secondSession), "Disposing one view must not disconnect another.");
Check((await Call("plugin.echo", "{\"text\":\"after\"}")).Error?.Code == "disconnected", "Disposed hub accepted work.");
Console.WriteLine("PACKED_CONSUMER_PASS NuGet-only host + separate attributed plugin assembly, automatic action metadata with reflection disabled, DTOs/scalars/enums/nullability/collections, named/positional/default binding, DI/keyed/scoped calls, Unicode, events, binary payloads, typed acknowledged channels, strict schema, policy and duplicates; no product source.");

public sealed record ConsumerActionValue(string Text, int Number);
public sealed class ConsumerActions
{
	[HybridAction("consumer.platform")]
	public static DevicePlatform Platform(DevicePlatform platform) => platform;
	[HybridAction("consumer.action")]
	public ValueTask<ConsumerActionValue> Echo(string text, int number) => ValueTask.FromResult(new ConsumerActionValue(text, number));

	[HybridAction("consumer.bytes")]
	public byte[] EchoBytes(byte[] payload) => payload;

	[HybridAction("consumer.channel")]
	public async Task<int> Send(HybridChannel<int> values)
	{
		await values.SendAsync(41);
		return 42;
	}
}

public sealed class ConsumerCounter : INativeCounterContract
{
	public int Value { get; private set; }
	public int Add(int increment) => Value += increment;
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	AllowDuplicateProperties = false, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true,
	Converters = [typeof(ConsumerPlatformConverter)])]
[JsonSerializable(typeof(DevicePlatform))]
internal partial class ConsumerPlatformJsonContext : JsonSerializerContext;

internal sealed class ConsumerPlatformConverter : JsonConverter<DevicePlatform>
{
	public override DevicePlatform Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
		reader.TokenType == JsonTokenType.String && !string.IsNullOrWhiteSpace(reader.GetString()) ?
		DevicePlatform.Create(reader.GetString()!) : throw new JsonException();
	public override void Write(Utf8JsonWriter writer, DevicePlatform value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
