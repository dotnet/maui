using System.Text.Json;
using System.Text.Json.Serialization;
using HybridApp.ExternalProvider;
using Maui.HybridApp;
using Maui.HybridApp.Generated;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class ActionTests
{
	static TestComposition.TestHost Services(Action<ServiceCollection>? configure = null)
	{
		var services = new ServiceCollection();
		services.AddScoped<ActionState>();
		services.AddKeyedScoped<ActionState>("key", (_, _) => new ActionState { Calls = 10 });
		services.AddKeyedScoped<ActionState>((byte)7, (_, _) => new ActionState { Calls = 7 });
		services.AddScoped(_ => new NativeDispatcher(action =>
		{
			services.DispatchCount++;
			return action();
		}));
		services.AddPlayground(new("test", DateTimeOffset.UtcNow));
		configure?.Invoke(services);
		return new(services);
	}

	// Keep dispatch accounting outside the provider so dispatch-once is observable.
	sealed class ServiceCollection : Microsoft.Extensions.DependencyInjection.ServiceCollection
	{
		internal int DispatchCount;
	}

	static CommandRegistry Registry(IServiceProvider services)
	{
		return services.GetRequiredService<CommandRegistry>();
	}

	static Task<JsonElement> Call(CommandRegistry registry, string command, string args = "{}", CancellationToken cancellation = default,
		Action<string, JsonElement>? publish = null)
	{
		using var json = JsonDocument.Parse(args);
		return registry.DispatchAsync(command, json.RootElement.Clone(), new Operation("session", "request", cancellation, publish ?? ((_, _) => { })));
	}

	[Theory]
	[InlineData("{\"count\":4,\"text\":\"hello\"}")]
	[InlineData("[4,\"hello\"]")]
	public async Task NamedAndPositionalArgumentsExcludeSpecialParameters(string json)
	{
		using var root = Services();
		using var scope = root.CreateScope();
		var registry = Registry(scope.ServiceProvider);
		var result = await Call(registry, "actions.bind", json);
		Assert.Equal(4, result.GetProperty("count").GetInt32());
		Assert.Equal("hello", result.GetProperty("text").GetString());
		Assert.Equal(10, result.GetProperty("keyed").GetInt32());
		Assert.Equal("request", result.GetProperty("id").GetString());
		Assert.Equal("session", result.GetProperty("session").GetString());
		Assert.Equal(7, (await Call(registry, "actions.byteKey")).GetInt32());
	}

	[Fact]
	public async Task AutomaticDtoEnumNullableAndCollectionsRoundTrip()
	{
		using var root = Services();
		using var scope = root.CreateScope();
		var registry = Registry(scope.ServiceProvider);
		const string dto = """{"value":{"text":"你好 👋","mode":"second","values":[1,2],"map":{"first":["x",null]},"note":null}}""";
		var result = await Call(registry, "actions.dto", dto);
		Assert.Equal("你好 👋", result.GetProperty("text").GetString());
		Assert.Equal("second", result.GetProperty("mode").GetString());
		Assert.Equal(new[] { 1, 2 }, result.GetProperty("values").EnumerateArray().Select(p => p.GetInt32()));
		Assert.Equal(JsonValueKind.Null, result.GetProperty("map").GetProperty("first")[1].ValueKind);
		Assert.Equal(JsonValueKind.Null, result.GetProperty("note").ValueKind);
		Assert.Equal(JsonValueKind.Null, (await Call(registry, "actions.nullable", """{"value":null}""")).ValueKind);
		Assert.Equal(3, (await Call(registry, "actions.nullable", "[3]")).GetInt32());
		var optional = await Call(registry, "actions.optional", "{}");
		Assert.Equal("fallback", optional.GetProperty("text").GetString());
		Assert.Equal(7, optional.GetProperty("count").GetInt32());
		Assert.Equal(JsonValueKind.Null, (await Call(registry, "actions.optional", """{"text":null}""")).GetProperty("text").ValueKind);
		Assert.Equal(7, (await Call(registry, "actions.optional", "[null]")).GetProperty("count").GetInt32());
		Assert.Equal("two", (await Call(registry, "actions.enumDefault")).GetString());
		Assert.Equal(JsonValueKind.Null, (await Call(registry, "actions.stringNull", "[null]")).ValueKind);
		Assert.Equal(JsonValueKind.Null, (await Call(registry, "actions.stringNullTask", "[null]")).ValueKind);
		Assert.Equal(JsonValueKind.Null, (await Call(registry, "actions.stringNullValueTask", "[null]")).ValueKind);
		Assert.Equal(JsonValueKind.Null, (await Call(registry, "actions.nullItems", "[[null]]"))[0].ValueKind);
		Assert.Equal("2026-10-14", (await Call(registry, "actions.dateOnly", """["2026-10-14"]""")).GetString());
		Assert.Equal("00:00:01", (await Call(registry, "actions.timeSpan", """["00:00:01"]""")).GetString());
		var required = await Call(registry, "actions.properties", """[{"urlValue":"strict","number":1}]""");
		Assert.Equal("strict", required.GetProperty("urlValue").GetString());
		Assert.Equal(1, required.GetProperty("number").GetInt32());
	}

	[Theory]
	[InlineData("actions.bind", "{}")]
	[InlineData("actions.bind", "[1]")]
	[InlineData("actions.bind", "[1,\"text\",true]")]
	[InlineData("actions.bind", """{"count":1,"text":"x","service":{}}""")]
	[InlineData("actions.bind", """{"count":1,"text":"x","count":2}""")]
	[InlineData("actions.bind", """{"count":1,"text":null}""")]
	[InlineData("actions.bind", """{"count":"1","text":"x"}""")]
	[InlineData("actions.bind", "null")]
	[InlineData("actions.bind", "1")]
	[InlineData("actions.dto", """{"value":{"text":"x","mode":"first","values":[],"map":{},"extra":true}}""")]
	[InlineData("actions.dto", """{"value":{"text":null,"mode":"first","values":[],"map":{}}}""")]
	[InlineData("actions.dto", """{"value":{"text":"x","values":[],"map":{}}}""")]
	[InlineData("actions.dto", """{"value":{"text":"x","mode":"first","values":[null],"map":{}}}""")]
	[InlineData("actions.nonNullItems", "[[null]]")]
	[InlineData("actions.properties", """[{"number":1}]""")]
	[InlineData("actions.properties", """[{"urlValue":null,"number":1}]""")]
	public async Task StrictBindingRejectsUnknownMissingExcessMalformedAndNestedNullArguments(string name, string json)
	{
		using var root = Services();
		using var scope = root.CreateScope();
		var registry = Registry(scope.ServiceProvider);
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() => Call(registry, name, json))).Code);
		Assert.Equal(0, scope.ServiceProvider.GetRequiredService<ActionState>().Calls);
	}

	[Fact]
	public async Task AllReturnShapesAndStaticMethodsAreCalledDirectly()
	{
		using var root = Services();
		using var scope = root.CreateScope();
		var registry = Registry(scope.ServiceProvider);
		foreach (var name in new[] { "actions.void", "actions.task", "actions.valueTask" })
			Assert.Equal(JsonValueKind.Null, (await Call(registry, name)).ValueKind);
		Assert.Equal(1, (await Call(registry, "actions.scalar")).GetInt32());
		Assert.Equal(2, (await Call(registry, "actions.taskResult")).GetInt32());
		Assert.Equal(3, (await Call(registry, "actions.valueTaskResult")).GetInt32());
		// There is deliberately no automatic Task.Run hop for synchronous methods.
		var thread = Environment.CurrentManagedThreadId;
		Assert.Equal(thread, (await Call(registry, "actions.thread")).GetInt32());
	}

	[Fact]
	public async Task ScopedServicesAndProviderInstancesNeverLeakAcrossOwningScopes()
	{
		using var root = Services();
		using var first = root.CreateScope();
		using var second = root.CreateScope();
		var one = Registry(first.ServiceProvider);
		var two = Registry(second.ServiceProvider);
		Assert.Equal(1, (await Call(one, "actions.counter")).GetInt32());
		Assert.Equal(2, (await Call(one, "actions.counter")).GetInt32());
		Assert.Equal(1, (await Call(two, "actions.counter")).GetInt32());
		Assert.NotSame(first.ServiceProvider.GetRequiredService<ActionTestProvider>(), second.ServiceProvider.GetRequiredService<ActionTestProvider>());
	}

	[Fact]
	public async Task CancellationContextAndTypedProgressUseCurrentOperation()
	{
		using var root = Services();
		using var scope = root.CreateScope();
		var registry = Registry(scope.ServiceProvider);
		using var cancellation = new CancellationTokenSource();
		Assert.Equal("request", (await Call(registry, "actions.contextOptional", cancellation: cancellation.Token)).GetString());
		var events = new List<(string, JsonElement)>();
		var work = Call(registry, "actions.wait", cancellation: cancellation.Token, publish: (topic, data) => events.Add((topic, data)));
		Assert.True(scope.ServiceProvider.GetRequiredService<ActionState>().Started.Task.IsCompleted);
		Assert.Equal("actions.progress", Assert.Single(events).Item1);
		Assert.Equal("request", events[0].Item2.GetProperty("id").GetString());
		cancellation.Cancel();
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => work);
		Assert.Equal("permission_denied", (await Assert.ThrowsAsync<CommandException>(() => Call(registry, "actions.error"))).Code);
	}

	[Fact]
	public async Task MainThreadDispatchHappensOnceAndInteractivePolicyIsExplicit()
	{
		var services = new ServiceCollection();
		services.AddScoped<ActionState>();
		services.AddScoped<ActionTestProvider>();
		services.AddScoped(_ => new NativeDispatcher(action => { services.DispatchCount++; return action(); }));
		services.AddPlayground(new("test", DateTimeOffset.UtcNow));
		var registrations = services.Count;
		services.AddPlayground(new("test", DateTimeOffset.UtcNow));
		Assert.Equal(registrations, services.Count);
		using var root = new TestComposition.TestHost(services);
		using var scope = root.CreateScope();
		var registry = Registry(scope.ServiceProvider);
		Assert.Contains("actions.ui", registry.InteractiveCommands);
		Assert.True(registry.IsInteractive("actions.ui"));
		Assert.False(registry.IsInteractive("actions.scalar"));
		Assert.Equal(42, (await Call(registry, "actions.ui", "[42]")).GetInt32());
		Assert.Equal(1, services.DispatchCount);
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() => Call(registry, "actions.ui", "{}"))).Code);
		Assert.Equal(1, services.DispatchCount);
	}

	[Fact]
	public async Task MissingServicesAndUnknownCommandsAndDuplicatesAreNotSilentlyAccepted()
	{
		using var root = Services(services => services.Remove(services.Single(s => s.ServiceType == typeof(ActionState) && s.IsKeyedService && s.ServiceKey is string key && key == "key")));
		using var scope = root.CreateScope();
		var registry = Registry(scope.ServiceProvider);
		await Assert.ThrowsAsync<InvalidOperationException>(() => Call(registry, "actions.bind", "[1,\"x\"]"));
		Assert.Equal("unknown_command", (await Assert.ThrowsAsync<CommandException>(() => Call(registry, "actions.missing"))).Code);
		var duplicate = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
		duplicate.AddPlayground(new("test", DateTimeOffset.UtcNow));
		var count = duplicate.Count;
		duplicate.AddPlayground(new("test", DateTimeOffset.UtcNow));
		Assert.Equal(count, duplicate.Count);
		var reflectionMetadata = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver().GetTypeInfo(typeof(int), new JsonSerializerOptions());
		Assert.Throws<ArgumentException>(() => registry.RegisterGenerated("not.generated", [reflectionMetadata], (_, _) => Task.FromResult(default(JsonElement))));
	}

	[Fact]
	public async Task AttributedExternalAssemblyUsesAutomaticMetadataAndScopedTypedCalls()
	{
		var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
		services.AddScoped<PluginActionState>();
		services.AddKeyedScoped<PluginActionState>("action-key", (_, _) => new() { Calls = 10 });
		services.AddExternalProviderActions();
		using var root = new TestComposition.TestHost(services);
		using var first = root.CreateScope();
		using var second = root.CreateScope();
		var one = Registry(first.ServiceProvider);
		var two = Registry(second.ServiceProvider);
		Assert.NotEqual(typeof(CommandRegistry).Assembly, typeof(PluginActions).Assembly);
		Assert.Null(ProtocolJsonContext.Default.GetTypeInfo(typeof(PluginActionValue)));
		Assert.Equal(1, (await Call(one, "plugin.action.count")).GetInt32());
		Assert.Equal(1, (await Call(two, "plugin.action.count")).GetInt32());
		Assert.Equal(11, (await Call(one, "plugin.action.services")).GetInt32());
		var result = await Call(one, "plugin.action.echo", """[{"text":"hello","kind":"second"}]""");
		Assert.Equal("hello", result.GetProperty("text").GetString());
		Assert.Equal("second", result.GetProperty("kind").GetString());
		Assert.Equal("typed", (await Call(one, "plugin.action.static", """{"value":"typed"}""")).GetString());
		Assert.Equal(6, (await Call(one, "plugin.action.staticClass", "[3]")).GetInt32());
	}

	[Fact]
	public async Task ConstructorAnnotationsAndNullableEnumDefaultsWorkInStandaloneProvider()
	{
		var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
		services.AddScoped<PluginActionState>();
		services.AddExternalProviderActions();
		using var root = new TestComposition.TestHost(services);
		using var scope = root.CreateScope();
		var registry = Registry(scope.ServiceProvider);
		Assert.Equal("second", (await Call(registry, "plugin.action.enumDefault")).GetString());
		Assert.Equal("second", (await Call(registry, "plugin.action.enumDefault", "[]")).GetString());
		Assert.Equal(JsonValueKind.Null, (await Call(registry, "plugin.action.enumDefault", "[null]")).ValueKind);
		Assert.Equal("second", (await Call(registry, "plugin.action.dtoDefault", """{"value":{}}""")).GetProperty("kind").GetString());
		Assert.Equal("second", (await Call(registry, "plugin.action.dtoDefault", "[{}]")).GetProperty("kind").GetString());
		Assert.Equal("", (await Call(registry, "plugin.action.normalized", """{"value":{"text":null}}""")).GetProperty("text").GetString());
		Assert.Equal("", (await Call(registry, "plugin.action.normalized", """[{"text":null}]""")).GetProperty("text").GetString());
		Assert.Equal("native", (await Call(registry, "plugin.action.normalized", """[{"text":"native"}]""")).GetProperty("text").GetString());
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() => Call(registry, "plugin.action.normalized", "[{}]"))).Code);
	}
}

internal enum ActionMode { First, Second }
internal sealed record ActionDto(string Text, ActionMode Mode, int[] Values, Dictionary<string, List<string?>> Map, string? Note = null);
internal sealed record ActionBound(int Count, string Text, int Keyed, string Id, string Session);
internal sealed record ActionOptional(string? Text, int Count);
internal sealed record ActionProgress(string Id);
internal sealed class ActionProperties
{
	public required string URLValue { get; init; }
	public int Number { get; set; }
}
internal sealed class ActionState
{
	public int Calls;
	public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
}

// Neither partial classes nor an action DTO JsonSerializerContext are required.
[HybridTopic("actions.progress")]
internal sealed class ActionTestProvider(ActionState state)
{

	[HybridAction("actions.bind")]
	internal ActionBound Bind(int count, [FromServices] ActionState service, string text,
		HybridActionContext context, CancellationToken cancellation, [FromKeyedServices("key")] ActionState keyed)
	{
		Assert.Same(state, service);
		Assert.Equal(context.Cancellation, cancellation);
		state.Calls++;
		return new(count, text, keyed.Calls, context.Id, context.Session);
	}
	[HybridAction("actions.dto")] internal static ActionDto Dto(ActionDto value) => value;
	[HybridAction("actions.nullable")] internal static int? Nullable(int? value) => value;
	[HybridAction("actions.stringNull")] internal static string? StringNull(string? value) => value;
	[HybridAction("actions.stringNullTask")] internal static Task<string?> StringNullTask(string? value) => System.Threading.Tasks.Task.FromResult(value);
	[HybridAction("actions.stringNullValueTask")] internal static ValueTask<string?> StringNullValueTask(string? value) => System.Threading.Tasks.ValueTask.FromResult(value);
	[HybridAction("actions.nullItems")] internal static List<string?> NullItems(List<string?> value) => value;
	[HybridAction("actions.nonNullItems")] internal static List<string> NonNullItems(List<string> value) => value;
	[HybridAction("actions.dateOnly")] internal static DateOnly Date(DateOnly value) => value;
	[HybridAction("actions.timeSpan")] internal static TimeSpan Time(TimeSpan value) => value;
	[HybridAction("actions.properties")] internal static ActionProperties Properties(ActionProperties value) => value;
	[HybridAction("actions.enumDefault")] internal static string EnumDefault(ActionMode mode = ActionMode.Second) => mode == ActionMode.Second ? "two" : "one";
	[HybridAction("actions.optional")] internal static ActionOptional Optional(string? text = "fallback", int count = 7) => new(text, count);
	[HybridAction("actions.counter")] internal int Counter() => ++state.Calls;
	[HybridAction("actions.byteKey")] internal static int ByteKey([FromKeyedServices((byte)7)] ActionState service) => service.Calls;
	[HybridAction("actions.void")] internal static void Void() { }
	[HybridAction("actions.scalar")] internal static int Scalar() => 1;
	[HybridAction("actions.task")] internal static Task Task() => System.Threading.Tasks.Task.CompletedTask;
	[HybridAction("actions.taskResult")] internal static Task<int> TaskResult() => System.Threading.Tasks.Task.FromResult(2);
	[HybridAction("actions.valueTask")] internal static ValueTask ValueTask() => System.Threading.Tasks.ValueTask.CompletedTask;
	[HybridAction("actions.valueTaskResult")] internal static ValueTask<int> ValueTaskResult() => System.Threading.Tasks.ValueTask.FromResult(3);
	[HybridAction("actions.thread")] internal static int Thread() => Environment.CurrentManagedThreadId;
	[HybridAction("actions.contextOptional")]
	internal static string ContextOptional(HybridActionContext? context = null, CancellationToken cancellation = default)
	{
		Assert.Equal(context!.Cancellation, cancellation);
		return context.Id;
	}
	[HybridAction("actions.ui", MainThread = true, Interactive = true)] internal static int Ui(int value) => value;
	[HybridAction("actions.error")] internal static void Error() => throw new CommandException("permission_denied", "OS denied.");
	[HybridAction("actions.wait")]
	internal async Task Wait(HybridActionContext context, CancellationToken cancellation)
	{
		context.Publish("actions.progress", new ActionProgress(context.Id), ActionProgressJson.Default.ActionProgress);
		state.Started.TrySetResult();
		await System.Threading.Tasks.Task.Delay(Timeout.Infinite, cancellation);
	}
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(ActionProgress))]
internal partial class ActionProgressJson : JsonSerializerContext;
