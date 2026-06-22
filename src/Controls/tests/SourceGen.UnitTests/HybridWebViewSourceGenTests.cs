using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.HybridWebViewSourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class HybridWebViewSourceGenTests
{
	[Fact]
	public void GeneratesInterceptorWhenJsonSerializerContextDefaultIsSourceGenerated()
	{
		const string source = """
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Maui.Controls;

namespace TestApp;

public static class Registration
{
	public static void Register(HybridWebView hybridWebView)
	{
		hybridWebView.SetInvokeJavaScriptTarget(new InvokeTarget(), InvokeTargetJsonContext.Default);
	}
}

public sealed class InvokeTarget
{
	public string Echo(string value) => value;
}

internal sealed class InvokeTargetJsonContext : JsonSerializerContext
{
	public static InvokeTargetJsonContext Default { get; } = new();
	private InvokeTargetJsonContext() : base(null) { }
	protected override JsonSerializerOptions? GeneratedSerializerOptions => null;
	public override JsonTypeInfo? GetTypeInfo(Type type) => null;
}
""";

		var (result, updatedCompilation, generatorDiagnostics) = RunGenerator(source);
		AssertNoCompilationErrors(updatedCompilation, generatorDiagnostics);

		var generated = Assert.Single(result.Results.Single().GeneratedSources);
		Assert.Equal("HybridWebViewInterceptors.g.cs", generated.HintName);
		Assert.Contains("file static class HybridWebViewInterceptors", generated.SourceText.ToString(), StringComparison.Ordinal);
		Assert.Contains("SetInvokeJavaScriptTarget_0", generated.SourceText.ToString(), StringComparison.Ordinal);
	}

	[Fact]
	public void GeneratesDispatchForInheritedPublicMethods()
	{
		const string source = """
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Maui.Controls;

namespace TestApp;

public static class Registration
{
	public static void Register(HybridWebView hybridWebView)
	{
		hybridWebView.SetInvokeJavaScriptTarget(new InvokeTarget(), InvokeTargetJsonContext.Default);
	}
}

public class BaseInvokeTarget
{
	public string InheritedEcho(string value) => value;
}

public sealed class InvokeTarget : BaseInvokeTarget
{
	public string OwnEcho(string value) => value;
}

internal sealed class InvokeTargetJsonContext : JsonSerializerContext
{
	public static InvokeTargetJsonContext Default { get; } = new();
	private InvokeTargetJsonContext() : base(null) { }
	protected override JsonSerializerOptions? GeneratedSerializerOptions => null;
	public override JsonTypeInfo? GetTypeInfo(Type type) => null;
}
""";

		var (result, updatedCompilation, generatorDiagnostics) = RunGenerator(source);
		AssertNoCompilationErrors(updatedCompilation, generatorDiagnostics);

		var generated = Assert.Single(result.Results.Single().GeneratedSources).SourceText.ToString();
		Assert.Contains("case \"InheritedEcho\":", generated, StringComparison.Ordinal);
		Assert.Contains("_target.InheritedEcho(__value)", generated, StringComparison.Ordinal);
		Assert.Contains("case \"OwnEcho\":", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void GeneratesArgumentCountCheckForParameterlessMethods()
	{
		const string source = """
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Maui.Controls;

namespace TestApp;

public static class Registration
{
	public static void Register(HybridWebView hybridWebView)
	{
		hybridWebView.SetInvokeJavaScriptTarget(new InvokeTarget(), InvokeTargetJsonContext.Default);
	}
}

public sealed class InvokeTarget
{
	public string NoArgs() => "";
}

internal sealed class InvokeTargetJsonContext : JsonSerializerContext
{
	public static InvokeTargetJsonContext Default { get; } = new();
	private InvokeTargetJsonContext() : base(null) { }
	protected override JsonSerializerOptions? GeneratedSerializerOptions => null;
	public override JsonTypeInfo? GetTypeInfo(Type type) => null;
}
""";

		var (result, updatedCompilation, generatorDiagnostics) = RunGenerator(source);
		AssertNoCompilationErrors(updatedCompilation, generatorDiagnostics);

		var generated = Assert.Single(result.Results.Single().GeneratedSources).SourceText.ToString();
		Assert.Contains("case \"NoArgs\":", generated, StringComparison.Ordinal);
		Assert.Contains("if (paramJsonValues is not null && paramJsonValues.Length != 0)", generated, StringComparison.Ordinal);
		Assert.Contains("Method 'NoArgs' expects 0 argument(s) but received ", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void EscapesKeywordMethodIdentifiers()
	{
		const string source = """
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Maui.Controls;

namespace TestApp;

public static class Registration
{
	public static void Register(HybridWebView hybridWebView)
	{
		hybridWebView.SetInvokeJavaScriptTarget(new InvokeTarget(), InvokeTargetJsonContext.Default);
	}
}

public sealed class InvokeTarget
{
	public string @event() => "";
}

internal sealed class InvokeTargetJsonContext : JsonSerializerContext
{
	public static InvokeTargetJsonContext Default { get; } = new();
	private InvokeTargetJsonContext() : base(null) { }
	protected override JsonSerializerOptions? GeneratedSerializerOptions => null;
	public override JsonTypeInfo? GetTypeInfo(Type type) => null;
}
""";

		var (result, updatedCompilation, generatorDiagnostics) = RunGenerator(source);
		AssertNoCompilationErrors(updatedCompilation, generatorDiagnostics);

		var generated = Assert.Single(result.Results.Single().GeneratedSources).SourceText.ToString();
		Assert.Contains("case \"event\":", generated, StringComparison.Ordinal);
		Assert.Contains("_target.@event()", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("_target.event()", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void SkipsMethodsWithUnsafePointerParameters()
	{
		const string source = """
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Maui.Controls;

namespace TestApp;

public static class Registration
{
	public static void Register(HybridWebView hybridWebView)
	{
		hybridWebView.SetInvokeJavaScriptTarget(new InvokeTarget(), InvokeTargetJsonContext.Default);
	}
}

public sealed class InvokeTarget
{
	public string Supported() => "";
	public unsafe string Pointer(int* value) => "";
	public unsafe string FunctionPointer(delegate*<int, void> callback) => "";
}

internal sealed class InvokeTargetJsonContext : JsonSerializerContext
{
	public static InvokeTargetJsonContext Default { get; } = new();
	private InvokeTargetJsonContext() : base(null) { }
	protected override JsonSerializerOptions? GeneratedSerializerOptions => null;
	public override JsonTypeInfo? GetTypeInfo(Type type) => null;
}
""";

		var (result, updatedCompilation, generatorDiagnostics) = RunGenerator(source, allowUnsafe: true);
		AssertNoCompilationErrors(updatedCompilation, generatorDiagnostics);

		var generated = Assert.Single(result.Results.Single().GeneratedSources).SourceText.ToString();
		Assert.Contains("case \"Supported\":", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("case \"Pointer\":", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("case \"FunctionPointer\":", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("Deserialize<global::System.Int32*>", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("delegate*", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void InterfaceReceiverAssignsInvokerThroughInterface()
	{
		const string source = """
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Maui;

namespace TestApp;

public static class Registration
{
	public static void Register(IHybridWebView hybridWebView)
	{
		hybridWebView.SetInvokeJavaScriptTarget(new InvokeTarget(), InvokeTargetJsonContext.Default);
	}
}

public sealed class InvokeTarget
{
	public string Echo(string value) => value;
}

internal sealed class InvokeTargetJsonContext : JsonSerializerContext
{
	public static InvokeTargetJsonContext Default { get; } = new();
	private InvokeTargetJsonContext() : base(null) { }
	protected override JsonSerializerOptions? GeneratedSerializerOptions => null;
	public override JsonTypeInfo? GetTypeInfo(Type type) => null;
}
""";

		var (result, updatedCompilation, generatorDiagnostics) = RunGenerator(source);
		AssertNoCompilationErrors(updatedCompilation, generatorDiagnostics);

		var generated = Assert.Single(result.Results.Single().GeneratedSources).SourceText.ToString();
		Assert.Contains("this global::Microsoft.Maui.IHybridWebView hybridWebView", generated, StringComparison.Ordinal);
		Assert.Contains("hybridWebView.Invoker = new Invoker_0(typedTarget, jsonSerializerContext);", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("concreteHybridWebView", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("can only be registered on Microsoft.Maui.Controls.HybridWebView", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void GeneratesDispatchForInterfaceTargetMethods()
	{
		const string source = """
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Maui.Controls;

namespace TestApp;

public static class Registration
{
	public static void Register(HybridWebView hybridWebView, InvokeTarget target)
	{
		hybridWebView.SetInvokeJavaScriptTarget<IInvokeTarget>(target, InvokeTargetJsonContext.Default);
	}
}

public interface IInvokeTarget
{
	string Echo(string value);
}

public sealed class InvokeTarget : IInvokeTarget
{
	public string Echo(string value) => value;
}

internal sealed class InvokeTargetJsonContext : JsonSerializerContext
{
	public static InvokeTargetJsonContext Default { get; } = new();
	private InvokeTargetJsonContext() : base(null) { }
	protected override JsonSerializerOptions? GeneratedSerializerOptions => null;
	public override JsonTypeInfo? GetTypeInfo(Type type) => null;
}
""";

		var (result, updatedCompilation, generatorDiagnostics) = RunGenerator(source);
		AssertNoCompilationErrors(updatedCompilation, generatorDiagnostics);

		var generated = Assert.Single(result.Results.Single().GeneratedSources).SourceText.ToString();
		Assert.Contains("private readonly global::TestApp.IInvokeTarget _target;", generated, StringComparison.Ordinal);
		Assert.Contains("case \"Echo\":", generated, StringComparison.Ordinal);
		Assert.Contains("_target.Echo(__value)", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void ReturnSerializationDoesNotCollideWithResultParameter()
	{
		const string source = """
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Maui.Controls;

namespace TestApp;

public static class Registration
{
	public static void Register(HybridWebView hybridWebView)
	{
		hybridWebView.SetInvokeJavaScriptTarget(new InvokeTarget(), InvokeTargetJsonContext.Default);
	}
}

public sealed class InvokeTarget
{
	public string Echo(string result) => result;
}

internal sealed class InvokeTargetJsonContext : JsonSerializerContext
{
	public static InvokeTargetJsonContext Default { get; } = new();
	private InvokeTargetJsonContext() : base(null) { }
	protected override JsonSerializerOptions? GeneratedSerializerOptions => null;
	public override JsonTypeInfo? GetTypeInfo(Type type) => null;
}
""";

		var (result, updatedCompilation, generatorDiagnostics) = RunGenerator(source);
		AssertNoCompilationErrors(updatedCompilation, generatorDiagnostics);

		var generated = Assert.Single(result.Results.Single().GeneratedSources).SourceText.ToString();
		Assert.Contains("return SerializeResult<", generated, StringComparison.Ordinal);
		Assert.Contains("_target.Echo(__result), _ctx, \"Echo\");", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("var __result = _target.Echo(__result);", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void ReportsDiagnosticForPrivateNestedInvokeTarget()
	{
		const string source = """
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Maui.Controls;

namespace TestApp;

public sealed class Registration
{
	public void Register(HybridWebView hybridWebView)
	{
		hybridWebView.SetInvokeJavaScriptTarget(new InvokeTarget(), InvokeTargetJsonContext.Default);
	}

	private sealed class InvokeTarget
	{
		public string Echo(string value) => value;
	}
}

internal sealed class InvokeTargetJsonContext : JsonSerializerContext
{
	public static InvokeTargetJsonContext Default { get; } = new();
	private InvokeTargetJsonContext() : base(null) { }
	protected override JsonSerializerOptions? GeneratedSerializerOptions => null;
	public override JsonTypeInfo? GetTypeInfo(Type type) => null;
}
""";

		var (result, _, _) = RunGenerator(source);

		var diagnostic = Assert.Single(result.Results.Single().Diagnostics);
		Assert.Equal("MAUIHWVSG003", diagnostic.Id);
		Assert.Contains("InvokeTarget", diagnostic.GetMessage(), StringComparison.Ordinal);
	}

	[Fact]
	public void ReportsDiagnosticForOverloadsBeforeUnsupportedMethodsAreFiltered()
	{
		const string source = """
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Maui.Controls;

namespace TestApp;

public static class Registration
{
	public static void Register(HybridWebView hybridWebView)
	{
		hybridWebView.SetInvokeJavaScriptTarget(new InvokeTarget(), InvokeTargetJsonContext.Default);
	}
}

public sealed class InvokeTarget
{
	public string Echo() => "";
	public string Echo<T>() => "";
}

internal sealed class InvokeTargetJsonContext : JsonSerializerContext
{
	public static InvokeTargetJsonContext Default { get; } = new();
	private InvokeTargetJsonContext() : base(null) { }
	protected override JsonSerializerOptions? GeneratedSerializerOptions => null;
	public override JsonTypeInfo? GetTypeInfo(Type type) => null;
}
""";

		var (result, _, _) = RunGenerator(source);

		var diagnostic = Assert.Single(result.Results.Single().Diagnostics);
		Assert.Equal("MAUIHWVSG001", diagnostic.Id);
		Assert.Contains("Echo", diagnostic.GetMessage(), StringComparison.Ordinal);
	}

	[Fact]
	public void ReportsDiagnosticForOpenGenericInvokeTarget()
	{
		const string source = """
using System.Text.Json.Serialization;
using Microsoft.Maui.Controls;

namespace TestApp;

public static class Registration
{
	public static void Register<TTarget>(HybridWebView hybridWebView, TTarget target, JsonSerializerContext jsonSerializerContext)
		where TTarget : class
	{
		hybridWebView.SetInvokeJavaScriptTarget<TTarget>(target, jsonSerializerContext);
	}
}
""";

		var (result, _, _) = RunGenerator(source);

		var diagnostic = Assert.Single(result.Results.Single().Diagnostics);
		Assert.Equal("MAUIHWVSG002", diagnostic.Id);
		Assert.Contains("TTarget", diagnostic.GetMessage(), StringComparison.Ordinal);
	}

	static (GeneratorDriverRunResult Result, Compilation UpdatedCompilation, ImmutableArray<Diagnostic> GeneratorDiagnostics) RunGenerator(string source, bool allowUnsafe = false)
	{
		var parseOptions = CSharpParseOptions.Default
			.WithLanguageVersion(LanguageVersion.Preview)
			.WithFeatures([new KeyValuePair<string, string>("InterceptorsNamespaces", "Microsoft.Maui.Controls.Generated")]);
		var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
			.WithAllowUnsafe(allowUnsafe);
		var compilation = CSharpCompilation.Create(
			"HwvSourceGenTest",
			[CSharpSyntaxTree.ParseText(source, parseOptions, path: "/tmp/TestApp/Registration.cs")],
			GetReferences(),
			compilationOptions);

		GeneratorDriver driver = CSharpGeneratorDriver.Create([new HybridWebViewInvokeTargetGenerator().AsSourceGenerator()], parseOptions: parseOptions);
		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var generatorDiagnostics);
		return (driver.GetRunResult(), updatedCompilation, generatorDiagnostics);
	}

	static void AssertNoCompilationErrors(Compilation compilation, ImmutableArray<Diagnostic> generatorDiagnostics)
	{
		var errors = generatorDiagnostics
			.Concat(compilation.GetDiagnostics())
			.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();

		Assert.Empty(errors);
	}

	static IEnumerable<MetadataReference> GetReferences()
	{
		var trustedPlatformAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
			.Split(Path.PathSeparator);

		return trustedPlatformAssemblies
			.Append(typeof(IHybridWebView).Assembly.Location)
			.Append(typeof(HybridWebView).Assembly.Location)
			.Distinct()
			.Select(static path => MetadataReference.CreateFromFile(path));
	}
}
