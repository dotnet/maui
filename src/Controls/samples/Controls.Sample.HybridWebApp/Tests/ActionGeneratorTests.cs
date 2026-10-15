using Maui.HybridApp;
using Maui.HybridApp.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using HybridApp.ExternalProvider;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class ActionGeneratorTests
{
	[Fact]
	public void ReferencedContractsRequireAnExplicitRootAndOnlyAnnotatedMembersAreImported()
	{
		var none = Generate("public class Nothing {}");
		Assert.Empty(none.Result.GeneratedTrees);
		var wrong = Generate("[assembly: Microsoft.Maui.Interop.HybridExport(typeof(System.IComparable))] public class Nothing {}");
		Assert.Empty(wrong.Result.GeneratedTrees);
		var source = """
			[assembly: Microsoft.Maui.Interop.HybridExport(typeof(Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard))]
			public class Nothing {}
			""";
		var imported = Generate(source);
		Assert.Empty(imported.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(imported.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		var code = string.Join("\n", imported.Result.GeneratedTrees);
		Assert.Contains("GetRequiredService<global::Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard>()", code, StringComparison.Ordinal);
		Assert.Contains(".@HasText", code, StringComparison.Ordinal);
		Assert.Contains("HybridNativeEvents.Register", code, StringComparison.Ordinal);
		Assert.DoesNotContain("ClipboardImplementation", code, StringComparison.Ordinal);
		Assert.DoesNotContain("Target<global::Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard>", code, StringComparison.Ordinal);
		Assert.DoesNotContain("GetMethod", code, StringComparison.Ordinal);
	}

	[Fact]
	public void AsyncImportedEnumerableAndNullableContractsGenerateGeneralTypedCode()
	{
		var generated = Generate("""
			using System.Collections.Generic; using System.Threading; using System.Threading.Tasks;
			[assembly: HybridExport(typeof(IValues))]
			public record Value(string Name);
			public interface IValues {
				[HybridAction("values.all", MainThread = true)] Task<IEnumerable<Value>> GetAsync();
				[HybridAction("values.optional")] Task<IEnumerable<Value?>?> OptionalAsync();
				[HybridAction("values.write", MainThread = true, Interactive = true)] Task WriteAsync(Value? value = null, CancellationToken cancellation = default);
			}
			""");
		Assert.Empty(generated.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(generated.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		var code = string.Join("\n", generated.Result.GeneratedTrees);
		Assert.Contains("await services.GetRequiredService<global::IValues>().@GetAsync()", code, StringComparison.Ordinal);
		Assert.Contains("operation.Cancellation", code, StringComparison.Ordinal);
		Assert.Contains("foreach (var item in value)", code, StringComparison.Ordinal);
		Assert.DoesNotContain("GetMethod", code, StringComparison.Ordinal);
	}

	[Fact]
	public void InheritedInterfaceMembersBindToTheSelectedRootAndConflictsAreDiagnostics()
	{
		var source = """
			[assembly: Microsoft.Maui.Interop.HybridExport(typeof(ISelected))]
			public interface IBase { [HybridAction("base.count")] int Count(); }
			public interface ISelected : IBase { bool Unannotated { get; } }
			""";
		var result = Generate(source);
		Assert.Empty(result.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Contains("GetRequiredService<global::ISelected>()", string.Join("\n", result.Result.GeneratedTrees), StringComparison.Ordinal);
		Assert.DoesNotContain("Unannotated", string.Join("\n", result.Result.GeneratedTrees), StringComparison.Ordinal);
		var duplicate = Generate(source.Replace("public interface IBase", "[assembly: HybridExport(typeof(IBase))]\npublic interface IBase", StringComparison.Ordinal));
		Assert.Contains(duplicate.Result.Diagnostics, d => d.Id == "HYBRID007");
		var unsupported = Generate("""
			[assembly: HybridExport(typeof(IUnsupported))]
			public interface IUnsupported { [HybridAction("bad")] void Call(ref int value); }
			""");
		Assert.Contains(unsupported.Result.Diagnostics, d => d.Id == "HYBRID002");
	}

	[Fact]
	public async Task ImportedShadowsInvokeOnlyTheAnnotatedDeclaringContract()
	{
		var result = Generate("""
			using System;
			[assembly: HybridExport(typeof(ISelected))]
			public interface IBase {
				[HybridAction("shadow.base.method")] int Count();
				int Value { [HybridAction("shadow.base.getter")] get; }
				[HybridNativeEvent("shadow.base.changed")] event EventHandler<EventArgs> Changed;
			}
			public interface ISelected : IBase {
				new int Count(); new int Value {get;}
				new event EventHandler<EventArgs> Changed;
			}
			public class Implementation : ISelected {
				EventHandler<EventArgs>? basis, selected;
				int IBase.Count() => 7; int IBase.Value => 8;
				public int Count() => 99; public int Value => 100;
				event EventHandler<EventArgs> IBase.Changed {add=>basis+=value;remove=>basis-=value;}
				public event EventHandler<EventArgs> Changed {add=>selected+=value;remove=>selected-=value;}
				public void RaiseBase()=>basis?.Invoke(this,EventArgs.Empty);
				public void RaiseSelected()=>selected?.Invoke(this,EventArgs.Empty);
			}
			""");
		Assert.Empty(result.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		using var image = new MemoryStream();
		Assert.True(result.Output.Emit(image).Success);
		var assembly = System.Reflection.Assembly.Load(image.ToArray());
		var implementationType = assembly.GetType("Implementation")!;
		var implementation = Activator.CreateInstance(implementationType)!;
		var services = new ServiceCollection();
		services.AddHybridApp(new());
		services.AddSingleton(new NativeDispatcher(action => action()));
		services.AddSingleton(assembly.GetType("ISelected")!, implementation);
		assembly.GetType("Maui.HybridApp.Generated.ActionCompilationActionAssembly")!
			.GetMethod("AddActionCompilationActions")!.Invoke(null, [services]);
		using var host = services.BuildServiceProvider();
		using var scope = host.CreateScope();
		using var local = scope.ServiceProvider.CreateHybridAppProvider();
		var hub = local.GetRequiredService<HybridAppTargetObject>();
		var session = Guid.NewGuid().ToString("D");
		async Task<Response> Call(string command, string arguments = "{}")
		{
			using var args = System.Text.Json.JsonDocument.Parse(arguments);
			return System.Text.Json.JsonSerializer.Deserialize(await hub.Invoke(
				System.Text.Json.JsonSerializer.Serialize(new Request(1, session, Guid.NewGuid().ToString("D"),
					command, args.RootElement), ProtocolJsonContext.Default.Request)), ProtocolJsonContext.Default.Response)!;
		}
		Assert.Equal("success", (await Call("core.connect")).Status);
		Assert.Equal(7, (await Call("shadow.base.method")).Result!.Value.GetInt32());
		Assert.Equal(8, (await Call("shadow.base.getter")).Result!.Value.GetInt32());
		var messages = new List<NativeEvent>();
		hub.NativeEventReceived += messages.Add;
		Assert.Equal("success", (await Call("core.subscribe", """{"topic":"shadow.base.changed"}""")).Status);
		implementationType.GetMethod("RaiseSelected")!.Invoke(implementation, null);
		Assert.Empty(messages);
		implementationType.GetMethod("RaiseBase")!.Invoke(implementation, null);
		Assert.Single(messages);
		Assert.Equal("success", (await Call("core.unsubscribe", """{"topic":"shadow.base.changed"}""")).Status);
		implementationType.GetMethod("RaiseBase")!.Invoke(implementation, null);
		Assert.Single(messages);
	}

	[Fact]
	public void CustomConverterHandlesNullableAndNonnullableSymbolsInTheSameGraph()
	{
		var result = Generate("""
			using System; using System.Text.Json; using System.Text.Json.Serialization;
			[JsonConverter(typeof(ValueConverter))] public record Value(string Text);
			public class ValueConverter : JsonConverter<Value> {
				public override Value Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(r.GetString()!);
				public override void Write(Utf8JsonWriter w, Value v, JsonSerializerOptions o) => w.WriteStringValue(v.Text);
			}
			public class Provider {
				[HybridAction("converted")] public static Value? Echo(Value value) => value;
				[HybridAction("converted.nonnull")] public static Value EchoNullable(Value? value) => value ?? new("default");
			}
			""");
		Assert.Empty(result.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(result.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Contains("converted.nonnull", string.Join("\n", result.Result.GeneratedTrees), StringComparison.Ordinal);
	}

	[Fact]
	public void TypedCustomConvertersAndDeclaredStrictContextsAreNotReflectionFallbacks()
	{
		var result = Generate("""
			using System; using System.Text.Json; using System.Text.Json.Serialization;
			[JsonConverter(typeof(ValueConverter))] public record Value(string Text);
			public class ValueConverter : JsonConverter<Value> {
				public override Value Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(r.GetString()!);
				public override void Write(Utf8JsonWriter w, Value v, JsonSerializerOptions o) => w.WriteStringValue(v.Text);
			}
			public class Provider { [HybridAction("converted")] public static Value Echo(Value value) => value; }
			""");
		Assert.Empty(result.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(result.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Contains("static readonly global::ValueConverter CustomConverter", string.Join("\n", result.Result.GeneratedTrees), StringComparison.Ordinal);
		Assert.DoesNotContain("Activator", string.Join("\n", result.Result.GeneratedTrees), StringComparison.Ordinal);
	}

	[Fact]
	public void TypedConstructorFactoriesHonorValueDefaultsAndTypeKeysWithoutReflectionConstruction()
	{
		var result = Generate("""
			using System; using Microsoft.Extensions.DependencyInjection;
			public class Service {}
			public class Provider([FromKeyedServices(typeof(Service))] Service service, int count = 7, int? missing = null) {
				[HybridAction("constructor.defaults")] public int Count() => count;
			}
			""");
		Assert.Empty(result.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(result.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		var code = string.Join("\n", result.Result.GeneratedTrees);
		Assert.Contains("GetRequiredKeyedService<global::Service>(typeof(global::Service))", code, StringComparison.Ordinal);
		Assert.Contains("services.GetService(typeof(int)) is int", code, StringComparison.Ordinal);
		Assert.DoesNotContain("Activator", code, StringComparison.Ordinal);
	}

	[Fact]
	public void BinaryAndChannelsCompileWithDirectCallsAndOnlyJsonPayloadMetadata()
	{
		var generated = Generate("""
			using System.Threading; using System.Threading.Tasks; using Maui.HybridApp;
			public sealed record Progress(int Count);
			public class Provider {
				[HybridAction("bytes.echo", MainThread = true)] public static byte[] Echo(byte[] bytes, int count, CancellationToken cancellation, HybridActionContext context) => bytes;
				[HybridAction("bytes.instance")] public Task<byte[]> Instance(byte[] bytes, [FromServices] System.IServiceProvider services) => Task.FromResult(bytes);
				[HybridAction("bytes.stream")] public async Task<int> Stream(HybridChannel<byte[]> bytes, HybridChannel<Progress> progress) {
					await bytes.SendAsync(new byte[]{0,255}); await progress.SendAsync(new Progress(1)); return 1;
				}
			}
			""");
		Assert.Empty(generated.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(generated.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		var code = string.Join("\n", generated.Result.GeneratedTrees);
		Assert.Contains("RegisterGeneratedBinary", code, StringComparison.Ordinal);
		Assert.Contains("operation.BinaryPayload", code, StringComparison.Ordinal);
		Assert.Contains("operation.BindChannel<byte[]>", code, StringComparison.Ordinal);
		Assert.DoesNotContain("JsonTypeInfo<byte[]>", code, StringComparison.Ordinal);
		Assert.DoesNotContain("GetMethod", code, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("public byte[] Method(byte[] a, byte[] b) => a;", "HYBRID005")]
	[InlineData("public int Method(byte[] a) => 1;", "HYBRID005")]
	[InlineData("public byte[] Method(byte[]? a) => a!;", "HYBRID005")]
	[InlineData("public byte[]? Method() => null;", "HYBRID005")]
	[InlineData("public byte[][] Method() => [];", "HYBRID004")]
	[InlineData("public int Method(HybridChannel<System.IO.Stream> stream) => 1;", "HYBRID004")]
	[InlineData("public int Method([FromServices] HybridChannel<int> channel) => 1;", "HYBRID005")]
	public void InvalidBinaryOrChannelShapesAreDiagnosticNotRuntimeSerialization(string method, string diagnostic)
	{
		var generated = Generate("using Maui.HybridApp; public class Provider { [HybridAction(\"bytes\")] " + method + " }");
		Assert.Contains(generated.Result.Diagnostics, d => d.Id == diagnostic);
		Assert.DoesNotContain(generated.Result.Diagnostics, d => d.Id == "CS8785");
	}

	static readonly MetadataReference[] References = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
		.Concat([typeof(HybridActionAttribute).Assembly.Location, typeof(HybridAppTargetObject).Assembly.Location, typeof(IServiceCollection).Assembly.Location,
			typeof(ServiceCollection).Assembly.Location, typeof(PluginActions).Assembly.Location]).Distinct(StringComparer.Ordinal).Select(path => MetadataReference.CreateFromFile(path)).ToArray();

	static (Compilation Output, GeneratorDriverRunResult Result) Generate(string source)
	{
		var parse = new CSharpParseOptions(LanguageVersion.Preview);
		var compilation = CSharpCompilation.Create("ActionCompilation", [CSharpSyntaxTree.ParseText("global using Microsoft.Maui.Interop;\n" + source, parse)],
			References, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
		GeneratorDriver driver = CSharpGeneratorDriver.Create([new HybridActionGenerator().AsSourceGenerator()], parseOptions: parse);
		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);
		return (output, driver.GetRunResult());
	}

	[Theory]
	[InlineData("byte", "128")]
	[InlineData("sbyte", "-128")]
	[InlineData("short", "-32768")]
	[InlineData("ushort", "32768")]
	[InlineData("int", "int.MinValue")]
	[InlineData("uint", "2147483648U")]
	[InlineData("long", "long.MinValue")]
	[InlineData("ulong", "9223372036854775808UL")]
	public void EveryEnumUnderlyingWidthGeneratesStrictReflectionFreeCode(string underlying, string high)
	{
		var generated = Generate($$"""
			using System;
			using System.Text.Json.Serialization;
			using Maui.HybridApp;
			[Flags] public enum Bits : {{underlying}} { None = 0, Low = 1, High = {{high}}, Alias = High }
			public enum Mode { First, Alias = First, [JsonStringEnumMemberName("chosen-name")] Second }
			public sealed record Dto(Mode? Mode = Mode.Second, Bits Bits = Bits.High);
			public class Provider { [HybridAction("test.enum")] public static Dto Echo(Dto value) => value; }
			""");
		Assert.Empty(generated.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(generated.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		var code = string.Join("\n", generated.Result.GeneratedTrees);
		Assert.Contains("\"chosen-name\"", code, StringComparison.Ordinal);
		Assert.Contains("JsonValueKind.String", code, StringComparison.Ordinal);
		Assert.Contains("JsonValueKind.Array", code, StringComparison.Ordinal);
		Assert.DoesNotContain("Enum.Parse", code, StringComparison.Ordinal);
		Assert.DoesNotContain("Enum.Get", code, StringComparison.Ordinal);
		Assert.DoesNotContain("writer.WriteNumberValue((", code, StringComparison.Ordinal);
	}

	[Fact]
	public void ConflictingWireNamesAndCustomEnumConvertersRemainCompileTimeErrors()
	{
		foreach (var declaration in new[]
		{
			"""public enum Mode { [JsonStringEnumMemberName("same")] First, [JsonStringEnumMemberName("same")] Second }""",
		})
		{
			var generated = Generate("using System.Text.Json.Serialization; using Maui.HybridApp; " + declaration +
				""" public class Provider { [HybridAction("enum")] public static Mode Echo(Mode value) => value; }""");
			Assert.Contains(generated.Result.Diagnostics, d => d.Id == "HYBRID004");
			Assert.DoesNotContain(generated.Result.Diagnostics, d => d.Id == "CS8785");
		}
	}

	[Theory]
	[InlineData("", "HYBRID001")]
	[InlineData("space name", "HYBRID001")]
	[InlineData("é", "HYBRID001")]
	[InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklm", "HYBRID001")]
	public void InvalidNamesHaveActionDiagnostics(string name, string expected)
	{
		var result = Generate($$"""using Maui.HybridApp; public class Provider { [HybridAction("{{name}}")] public void Method() {} }""");
		Assert.Contains(result.Result.Diagnostics, diagnostic => diagnostic.Id == expected);
		Assert.DoesNotContain(result.Result.Diagnostics, diagnostic => diagnostic.Id == "CS8785");
	}

	[Theory]
	[InlineData("private void Method() {}", "HYBRID002")]
	[InlineData("protected void Method() {}", "HYBRID002")]
	[InlineData("public void Method<T>() {}", "HYBRID002")]
	[InlineData("public void Method(ref int value) {}", "HYBRID002")]
	[InlineData("public void Method(out int value) { value = 0; }", "HYBRID002")]
	[InlineData("public void Method(in int value) {}", "HYBRID002")]
	[InlineData("public async void Method() { await Task.Yield(); }", "HYBRID002")]
	[InlineData("public void Method(Action callback) {}", "HYBRID002")]
	[InlineData("public object Method() => new();", "HYBRID004")]
	[InlineData("public void Method(Stream value) {}", "HYBRID004")]
	[InlineData("public void Method(Dictionary<int,string> value) {}", "HYBRID004")]
	[InlineData("public void Method([FromServices, FromKeyedServices(\"key\")] IServiceProvider value) {}", "HYBRID005")]
	[InlineData("public void Method([FromServices] CancellationToken value) {}", "HYBRID005")]
	[InlineData("public void Method([FromKeyedServices] IServiceProvider value) {}", "HYBRID005")]
	public void UnsupportedMethodsAndSerializationHaveActionDiagnostics(string method, string expected)
	{
		var result = Generate($$"""
			using System;
			using System.IO;
			using System.Collections.Generic;
			using System.Threading;
			using System.Threading.Tasks;
			using Microsoft.Extensions.DependencyInjection;
			using Maui.HybridApp;
			public class Provider { [HybridAction("test.method")] {{method}} }
			""");
		Assert.Contains(result.Result.Diagnostics, diagnostic => diagnostic.Id == expected);
		Assert.DoesNotContain(result.Result.Diagnostics, diagnostic => diagnostic.Id == "CS8785");
	}

	[Fact]
	public void ContainingTypeAccessibilityAndGenericsAreDiagnosed()
	{
		foreach (var declaration in new[]
		{
			"""public class Outer { private class Provider { [HybridAction("action")] public void Method() {} } }""",
			"""public class Provider<T> { [HybridAction("action")] public void Method() {} }""",
		})
			Assert.Contains(Generate("using Maui.HybridApp; " + declaration).Result.Diagnostics, diagnostic => diagnostic.Id == "HYBRID002");
	}

	[Fact]
	public void DuplicateNamesAreDiagnosedAcrossTheWholeAssembly()
	{
		var source = """
			using Maui.HybridApp;
			using Microsoft.Extensions.DependencyInjection;
			public class One { [HybridAction("same")] public static int A() => 1; }
			public class Two { [HybridAction("same")] public static int B() => 2; }
			public class Startup { }
			""";
		Assert.Contains(Generate(source).Result.Diagnostics, diagnostic => diagnostic.Id == "HYBRID003");
		Assert.DoesNotContain(Generate(source.Replace("""[HybridAction("same")] public static int B()""", """[HybridAction("different")] public static int B()""", StringComparison.Ordinal)).Result.Diagnostics, diagnostic => diagnostic.Id == "HYBRID003");
		var withinProvider = """using Maui.HybridApp; public class One { [HybridAction("same")] public void A() {} [HybridAction("same")] public void B() {} }""";
		Assert.Contains(Generate(withinProvider).Result.Diagnostics, diagnostic => diagnostic.Id == "HYBRID003");
	}

	[Fact]
	public void NonPartialClassesAndAutomaticJsonMetadataCompileWithoutAnotherGenerator()
	{
		var source = """
			using System;
			using System.Collections.Generic;
			using System.Threading;
			using System.Threading.Tasks;
			using Maui.HybridApp;
			using Microsoft.Extensions.DependencyInjection;
			public enum Mode { One, Two }
			public record Dto(string Name, Mode Mode, int? Number = null, List<string?>? Items = null);
			public class Service { }
			internal class Provider {
				[HybridAction("test.dto", MainThread=true, Interactive=true)]
				internal ValueTask<Dto> Method(Dto value, [FromServices] Service service, CancellationToken cancellation, HybridActionContext context) => ValueTask.FromResult(value);
				[HybridAction("test.scalar")] public static double Scalar(double number = 1.5) => number;
			}

			""";
		var generated = Generate(source);
		Assert.Empty(generated.Result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
		Assert.Empty(generated.Output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
		var text = string.Join("\n", generated.Result.GeneratedTrees.Select(tree => tree.GetText().ToString()));
		Assert.Contains("JsonMetadataServices.CreateValueInfo", text, StringComparison.Ordinal);
		Assert.Contains("OriginatingResolver = this", text, StringComparison.Ordinal);
		Assert.DoesNotContain("JsonSerializable(", text, StringComparison.Ordinal);
		Assert.DoesNotContain("MethodInfo", text, StringComparison.Ordinal);
		Assert.DoesNotContain("Expression.", text, StringComparison.Ordinal);
		Assert.DoesNotContain("Task.Run", text, StringComparison.Ordinal);
	}

	[Fact]
	public void KnownRegistrationsDetectDuplicatesAcrossProviderAssemblies()
	{
		var source = """
			using Maui.HybridApp;
			using Microsoft.Extensions.DependencyInjection;
			using HybridApp.ExternalProvider;
			using Maui.HybridApp.Generated;
			public class LocalProvider { [HybridAction("plugin.action.echo")] public static int Echo() => 1; }
			public class Startup { public void Register(IServiceCollection services) { services.AddExternalProviderActions(); } }
			""";
		Assert.Contains(Generate(source).Result.Diagnostics, diagnostic => diagnostic.Id == "HYBRID003");
	}

	[Fact]
	public void StaticClassesGetAnExplicitGeneratedFactoryWithoutGenericStaticTypeArguments()
	{
		var generated = Generate("""
			using Maui.HybridApp;
			public static class MathActions { [HybridAction("math.double")] public static int Double(int value) => value * 2; }
			""");
		Assert.Empty(generated.Result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
		Assert.Empty(generated.Output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
		Assert.Contains("AddActionCompilationActions", string.Join("\n", generated.Result.GeneratedTrees), StringComparison.Ordinal);
		Assert.DoesNotContain("ModuleInitializer", string.Join("\n", generated.Result.GeneratedTrees), StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("""[HybridEvent("changed", HybridEventLifecycle.Start)] public Task Start() => Task.CompletedTask;""")]
	[InlineData("""[HybridEvent("changed", HybridEventLifecycle.Start)] private Task Start() => Task.CompletedTask; [HybridEvent("changed", HybridEventLifecycle.Stop)] public void Stop() {}""")]
	[InlineData("""[HybridEvent("changed", HybridEventLifecycle.Start)] public void Start() {} [HybridEvent("changed", HybridEventLifecycle.Stop)] public void Stop() {}""")]
	public void InvalidEventLifecyclesAreCompileTimeErrors(string source)
	{
		var generated = Generate("using System.Threading.Tasks; using Maui.HybridApp; public class Plugin { " + source + " }");
		Assert.Contains(generated.Result.Diagnostics, diagnostic => diagnostic.Id == "HYBRID006");
		Assert.DoesNotContain(generated.Result.Diagnostics, diagnostic => diagnostic.Id == "CS8785");
	}

	[Fact]
	public void OneAssemblyExtensionComposesActionsTopicsAndBoundScopedLifecycleHooks()
	{
		var generated = Generate("""
			using System.Threading.Tasks;
			using Maui.HybridApp;
			[HybridTopic("progress")]
			public class First {
				[HybridAction("first")] public int Action(int count, string text = "default") => count;
				[HybridEvent("changed", HybridEventLifecycle.Start)] public Task Start() => Task.CompletedTask;
				[HybridEvent("changed", HybridEventLifecycle.Stop)] public void Stop() {}
			}
			public class Second { [HybridAction("second")] public static void Action() {} }
			""");
		Assert.Empty(generated.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(generated.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		var sources = generated.Result.GeneratedTrees.Select(tree => tree.GetText().ToString()).ToArray();
		Assert.Single(sources, text => text.Contains("public static IServiceCollection Add", StringComparison.Ordinal));
		Assert.Contains(sources, text => text.Contains("var source = services.GetRequiredService<global::First>();", StringComparison.Ordinal) &&
			text.Contains("""registry.RegisterTopic("changed", source.@Start, source.@Stop);""", StringComparison.Ordinal));
		Assert.DoesNotContain(sources, text => text.Contains("GetTypes(", StringComparison.Ordinal) || text.Contains("ModuleInitializer", StringComparison.Ordinal));
	}

	[Theory]
	[InlineData("""
		public enum Mode { First, Second }
		public sealed record Dto(Mode? Mode = Mode.Second);
		public sealed class Provider {
			[HybridAction("enum.parameter")] public static Mode? Parameter(Mode? mode = Mode.Second) => mode;
			[HybridAction("enum.constructor")] public static Dto Constructor(Dto value) => value;
		}
		""")]
	[InlineData("""
		public sealed class Dto {
			public string Text { get; }
			public Dto(string? text) => Text = text ?? "";
		}
		public sealed class Provider { [HybridAction("nullable.constructor")] public static Dto Constructor(Dto value) => value; }
		""")]
	public void ConstructorParameterNullabilityAndNullableEnumDefaultsCompileInIsolation(string source)
	{
		var generated = Generate("using Maui.HybridApp;\n" + source);
		Assert.Empty(generated.Result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
		Assert.Empty(generated.Output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
		Assert.DoesNotContain("Read-1", string.Join("\n", generated.Result.GeneratedTrees), StringComparison.Ordinal);
	}

	[Fact]
	public void UnsupportedDtoShapesReportDiagnosticsRatherThanCrashingGenerator()
	{
		foreach (var dto in new[]
		{
			"""public sealed record Dto(object Value);""",
			"""public sealed class Dto { public string Text { get; init; } = ""; }""",
			"""public sealed class Dto { public string Text { get; } = ""; }""",
		})
		{
			var generated = Generate("using Maui.HybridApp; " + dto + """ public class Provider { [HybridAction("test.dto")] public Dto Method(Dto value) => value; }""");
			Assert.Contains(generated.Result.Diagnostics, diagnostic => diagnostic.Id == "HYBRID004");
			Assert.DoesNotContain(generated.Result.Diagnostics, diagnostic => diagnostic.Id == "CS8785");
		}
	}
}
