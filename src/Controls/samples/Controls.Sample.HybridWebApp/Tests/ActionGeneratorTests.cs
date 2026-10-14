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
	public void DuplicateNamesAreDiagnosedOnlyWithinProviderOrKnownOptIns()
	{
		var source = """
			using Maui.HybridApp;
			using Microsoft.Extensions.DependencyInjection;
			public class One { [HybridAction("same")] public static int A() => 1; }
			public class Two { [HybridAction("same")] public static int B() => 2; }
			public class Startup { public void Register(IServiceCollection services) { services.AddHybridActions<One>(); services.AddHybridActions<Two>(); } }
			""";
		Assert.Contains(Generate(source).Result.Diagnostics, diagnostic => diagnostic.Id == "HYBRID003");
		Assert.DoesNotContain(Generate(source.Replace("services.AddHybridActions<Two>();", "", StringComparison.Ordinal)).Result.Diagnostics, diagnostic => diagnostic.Id == "HYBRID003");
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
			public class LocalProvider { [HybridAction("plugin.action.echo")] public static int Echo() => 1; }
			public class Startup { public void Register(IServiceCollection services) { services.AddHybridActions<LocalProvider>(); services.AddHybridActions<PluginActions>(); } }
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
		Assert.Contains("AddMathActionsHybridActions", string.Join("\n", generated.Result.GeneratedTrees), StringComparison.Ordinal);
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
