using System.Text.Json;
using HybridApp.ExternalProvider;
using Maui.HybridApp;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddHybridApp(new HybridAppOptions("consumer.")).AddHybridProvider<PluginProvider>();
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
Check(typeof(CommandRegistry).Assembly != typeof(PluginProvider).Assembly, "The plugin must live in a separate assembly.");
Check(ProtocolJsonContext.Default.GetTypeInfo(typeof(PluginArgs)) is null, "Plugin DTOs must not enter global metadata.");
var connect = await Call("core.connect", "{}");
Check(connect.Status == "success", "Connect failed.");
var ready = connect.Result!.Value.Deserialize(ProtocolJsonContext.Default.ReadyResult)!;
Check(ready.PreferenceNamespace == "consumer." && ready.Commands.Contains("app.info", StringComparer.Ordinal) &&
	ready.Commands.Contains("device.info", StringComparer.Ordinal) && ready.Commands.Contains("preferences.write", StringComparer.Ordinal),
	"Product convenience did not register its implemented APIs and actual policy.");
Check((await Call("core.subscribe", "{\"topic\":\"plugin.changed\"}")).Status == "success", "Subscribe failed.");
var response = await Call("plugin.echo", "{\"text\":\"\\u4f60\\u597d \\ud83d\\udc4b\"}");
Check(response.Status == "success" && response.Result!.Value.Deserialize(PluginJsonContext.Default.PluginResult)!.Text == "你好 👋", "Generated plugin JSON failed.");
Check(events.Count == 1 && events[0].Session == session && events[0].Data.Deserialize(PluginJsonContext.Default.PluginResult)!.Text == "你好 👋", "Plugin event failed.");
Check((await Call("plugin.disabled", "{\"text\":\"no\"}")).Error?.Code == "disabled_command", "Disabled policy failed.");
Check((await Call("plugin.echo", "{\"text\":null}")).Error?.Code == "invalid_arguments", "Strict plugin nullability failed.");
Check((await Call("plugin.echo", "{\"text\":\"ok\",\"extra\":true}")).Error?.Code == "invalid_arguments", "Strict plugin schema failed.");
Check((await Call("essentials.notRegistered", "{}")).Error?.Code == "unknown_command", "Unimplemented Essentials commands must not be exposed.");
Check((await Call("sample.echo", "{}")).Error?.Code == "unknown_command", "Sample code must not ship.");
using var secondScope = root.CreateScope();
var secondHub = secondScope.ServiceProvider.GetRequiredService<HybridAppTargetObject>();
Check(!ReferenceEquals(hub, secondHub), "The target must be scoped, never shared across WebViews.");
Check(!ReferenceEquals(scope.ServiceProvider.GetServices<IHybridCommandProvider>().OfType<PluginProvider>().Single(),
	secondScope.ServiceProvider.GetServices<IHybridCommandProvider>().OfType<PluginProvider>().Single()), "Provider instances must be scoped.");
var secondSession = Guid.NewGuid().ToString("D");
Check((await Call("core.connect", "{}", secondHub, secondSession)).Status == "success", "Second scope connect failed.");
Check((await Call("plugin.echo", "{\"text\":\"second\"}", secondHub, secondSession)).Status == "success", "Second scope plugin failed.");
Check(events.Count == 1 && hub.IsCurrent(session), "A second view changed the first view's session/events.");
var registry = new CommandRegistry();
new PluginProvider().Register(registry);
try
{
	registry.RegisterTopic("plugin.changed");
	throw new Exception("Duplicate topic allowed.");
}
catch (InvalidOperationException) { }
try
{
	registry.Register("plugin.echo", PluginJsonContext.Default.PluginArgs, PluginJsonContext.Default.PluginResult,
		(args, _) => Task.FromResult(new PluginResult(args.Text)));
	throw new Exception("Duplicate command allowed.");
}
catch (InvalidOperationException) { }
Check(!typeof(CommandRegistry).Assembly.GetTypes().Any(type => type.FullName!.Contains("Sample", StringComparison.Ordinal) || type.Name.Contains("Fake", StringComparison.Ordinal)), "App/sample types entered the product.");
scope.Dispose();
Check(!hub.IsCurrent(session), "Scope disposal must retire the native document.");
Check(secondHub.IsCurrent(secondSession), "Disposing one view must not disconnect another.");
Check((await Call("plugin.echo", "{\"text\":\"after\"}")).Error?.Code == "disconnected", "Disposed hub accepted work.");
Console.WriteLine("PACKED_CONSUMER_PASS DI/scoped hubs and providers, generated JSON, separate plugin assembly, Unicode, events, strict schema, policy and duplicates; no product source.");
