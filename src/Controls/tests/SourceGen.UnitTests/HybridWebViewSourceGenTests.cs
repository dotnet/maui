using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
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
using System.Text.Json.Serialization;
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

[JsonSerializable(typeof(string))]
internal sealed partial class InvokeTargetJsonContext : JsonSerializerContext
{
}
""";

		var parseOptions = CSharpParseOptions.Default
			.WithLanguageVersion(LanguageVersion.Preview)
			.WithFeatures([new KeyValuePair<string, string>("InterceptorsPreviewNamespaces", "Microsoft.Maui.Controls.Generated")]);
		var compilation = CSharpCompilation.Create(
			"HwvSourceGenTest",
			[CSharpSyntaxTree.ParseText(source, parseOptions, path: "/tmp/TestApp/Registration.cs")],
			GetReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		GeneratorDriver driver = CSharpGeneratorDriver.Create([new HybridWebViewInvokeTargetGenerator().AsSourceGenerator()], parseOptions: parseOptions);
		var result = driver.RunGenerators(compilation).GetRunResult();

		var generated = Assert.Single(result.Results.Single().GeneratedSources);
		Assert.Equal("HybridWebViewInterceptors.g.cs", generated.HintName);
		Assert.Contains("file static class HybridWebViewInterceptors", generated.SourceText.ToString(), StringComparison.Ordinal);
		Assert.Contains("SetInvokeJavaScriptTarget_0", generated.SourceText.ToString(), StringComparison.Ordinal);
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

		var parseOptions = CSharpParseOptions.Default
			.WithLanguageVersion(LanguageVersion.Preview)
			.WithFeatures([new KeyValuePair<string, string>("InterceptorsPreviewNamespaces", "Microsoft.Maui.Controls.Generated")]);
		var compilation = CSharpCompilation.Create(
			"HwvSourceGenTest",
			[CSharpSyntaxTree.ParseText(source, parseOptions, path: "/tmp/TestApp/Registration.cs")],
			GetReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		GeneratorDriver driver = CSharpGeneratorDriver.Create([new HybridWebViewInvokeTargetGenerator().AsSourceGenerator()], parseOptions: parseOptions);
		var result = driver.RunGenerators(compilation).GetRunResult();

		var diagnostic = Assert.Single(result.Results.Single().Diagnostics);
		Assert.Equal("MAUIHWVSG002", diagnostic.Id);
		Assert.Contains("TTarget", diagnostic.GetMessage(), StringComparison.Ordinal);
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
