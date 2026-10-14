using System.Text.Json;
using HybridApp.ExternalProvider;
using Maui.HybridApp;
using Maui.HybridApp.Generated;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddHybridApp(new HybridAppOptions("consumer."));
services.AddScoped<PluginActionState>();
services.AddKeyedScoped<PluginActionState>("action-key", (_, _) => new() { Calls = 10 });
services.AddExternalProviderActions();
services.AddPackageConsumerActions();
using var root = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
using var scope = root.CreateScope();
var hub = scope.ServiceProvider.GetRequiredService<HybridAppTargetObject>();
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
Check((await Call("plugin.disabled", "{\"text\":\"no\"}")).Error?.Code == "disabled_command", "Disabled policy failed.");
Check((await Call("plugin.echo", "{\"text\":null}")).Error?.Code == "invalid_arguments", "Strict plugin nullability failed.");
Check((await Call("plugin.echo", "{\"text\":\"ok\",\"extra\":true}")).Error?.Code == "invalid_arguments", "Strict plugin schema failed.");
Check((await Call("essentials.notRegistered", "{}")).Error?.Code == "unknown_command", "Unimplemented Essentials commands must not be exposed.");
Check((await Call("sample.echo", "{}")).Error?.Code == "unknown_command", "Sample code must not ship.");
using var secondScope = root.CreateScope();
var secondHub = secondScope.ServiceProvider.GetRequiredService<HybridAppTargetObject>();
Check(!ReferenceEquals(hub, secondHub), "The target must be scoped, never shared across WebViews.");
Check(!ReferenceEquals(scope.ServiceProvider.GetRequiredService<PluginActions>(),
	secondScope.ServiceProvider.GetRequiredService<PluginActions>()), "Plugin instances must be scoped.");
var secondSession = Guid.NewGuid().ToString("D");
Check((await Call("core.connect", "{}", secondHub, secondSession)).Status == "success", "Second scope connect failed.");
Check((await Call("plugin.echo", "{\"text\":\"second\"}", secondHub, secondSession)).Status == "success", "Second scope plugin failed.");
Check((await Call("plugin.action.count", "{}", secondHub, secondSession)).Result!.Value.GetInt32() == 1, "Generated provider state leaked between scopes.");
Check(events.Count == 1 && hub.IsCurrent(session), "A second view changed the first view's session/events.");
var registry = scope.ServiceProvider.GetRequiredService<CommandRegistry>();
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
Console.WriteLine("PACKED_CONSUMER_PASS NuGet-only host + separate attributed plugin assembly, automatic action metadata with reflection disabled, DTOs/scalars/enums/nullability/collections, named/positional/default binding, DI/keyed/scoped calls, Unicode, events, strict schema, policy and duplicates; no product source.");

public sealed record ConsumerActionValue(string Text, int Number);
public sealed class ConsumerActions
{
	[HybridAction("consumer.action")]
	public ValueTask<ConsumerActionValue> Echo(string text, int number) => ValueTask.FromResult(new ConsumerActionValue(text, number));
}
