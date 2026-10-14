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
		.Concat([typeof(HybridActionAttribute).Assembly.Location, typeof(IServiceCollection).Assembly.Location,
			typeof(ServiceCollection).Assembly.Location, typeof(PluginActions).Assembly.Location]).Distinct(StringComparer.Ordinal).Select(path => MetadataReference.CreateFromFile(path)).ToArray();

	static (Compilation Output, GeneratorDriverRunResult Result) Generate(string source)
	{
		var parse = new CSharpParseOptions(LanguageVersion.Preview);
		var compilation = CSharpCompilation.Create("ActionCompilation", [CSharpSyntaxTree.ParseText(source, parse)],
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
			"""[JsonConverter(typeof(JsonStringEnumConverter<Mode>))] public enum Mode { First, Second }""",
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
