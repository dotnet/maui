using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class SourceGenDeterminismTests : SourceGenTestsBase
{
	private const string Xaml = """
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="Hello MAUI!" />
</ContentPage>
""";

	private record AdditionalXamlFile(string Path, string Inflator = "Runtime")
		: AdditionalFile(
			Text: ToAdditionalText(Path, Xaml),
			Kind: "Xaml",
			RelativePath: Path,
			TargetPath: Path,
			ManifestResourceName: "Test.TestPage.xaml",
			TargetFramework: "net10.0",
			NoWarn: null,
			Inflator: Inflator);

	[Fact]
	public void RuntimeInflatorStillGeneratesAssemblyMetadata()
	{
		var result = RunGenerator<XamlGenerator>(
			CreateMauiCompilation(),
			new AdditionalXamlFile("Test.xaml"));

		var sources = GetGeneratedSources(result);

		Assert.DoesNotContain(sources.Keys, hintName => hintName.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase));
		Assert.Contains(
			"[assembly: global::Microsoft.Maui.Controls.Xaml.XamlResourceId(",
			sources.Single(source => source.Key.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).Value,
			StringComparison.Ordinal);
		Assert.Contains("GlobalXmlns.g.cs", sources.Keys);
	}

	[Fact]
	public void EquivalentReferenceOrderProducesIdenticalGeneratedOutput()
	{
		var firstReference = CreateXmlnsReference("External.One", "External.One");
		var secondReference = CreateXmlnsReference("External.Two", "External.Two");
		var compilation = CreateCompilationWithGlobalXmlns();

		var first = RunGenerator<XamlGenerator>(
			compilation.AddReferences(firstReference, secondReference),
			new AdditionalXamlFile("Test.xaml"));
		var second = RunGenerator<XamlGenerator>(
			compilation.AddReferences(secondReference, firstReference),
			new AdditionalXamlFile("Test.xaml"));

		AssertGeneratedSourcesEqual(GetGeneratedSources(first), GetGeneratedSources(second));
	}

	[Fact]
	public void HydratedReferenceAddsGlobalXmlnsGeneratedDocument()
	{
		var xamlFile = new AdditionalXamlFile("Test.xaml");
		var compilation = CreateCompilationWithGlobalXmlns();
		var reference = CreateXmlnsReference("External.Hydrated", "External.Hydrated");

		var (beforeHydration, afterHydration) = RunGeneratorWithChanges<XamlGenerator>(
			compilation,
			(driver, currentCompilation) => (driver, currentCompilation.AddReferences(reference)),
			xamlFile);

		var beforeSources = GetGeneratedSources(beforeHydration);
		var afterSources = GetGeneratedSources(afterHydration);

		Assert.Contains("Global.Xmlns.cs", afterSources.Keys);
		Assert.DoesNotContain("External.Hydrated", beforeSources["Global.Xmlns.cs"], StringComparison.Ordinal);
		Assert.Contains(
			"""[assembly: global::Microsoft.Maui.Controls.XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "External.Hydrated", AssemblyName = "External.Hydrated")]""",
			afterSources["Global.Xmlns.cs"],
			StringComparison.Ordinal);
	}

	private static Compilation CreateCompilationWithGlobalXmlns()
		=> CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(
			"""
using Microsoft.Maui.Controls;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "urn:test")]
"""));

	private static PortableExecutableReference CreateXmlnsReference(string assemblyName, string clrNamespace)
	{
		var compilation = CreateMauiCompilation(assemblyName).AddSyntaxTrees(CSharpSyntaxTree.ParseText(
			$$"""
using Microsoft.Maui.Controls;

[assembly: XmlnsDefinition("urn:test", "{{clrNamespace}}")]

namespace {{clrNamespace}};
public class Marker;
"""));

		using var stream = new MemoryStream();
		var emitResult = compilation.Emit(stream);
		Assert.True(
			emitResult.Success,
			string.Join(Environment.NewLine, emitResult.Diagnostics.Select(diagnostic => diagnostic.ToString())));

		return MetadataReference.CreateFromImage(
			ImmutableArray.Create(stream.ToArray()),
			filePath: $"{assemblyName}.dll");
	}

	private static SortedDictionary<string, string> GetGeneratedSources(GeneratorDriverRunResult result)
		=> new(
			result.Results.Single().GeneratedSources.ToDictionary(
				source => source.HintName,
				source => source.SourceText.ToString(),
				StringComparer.Ordinal),
			StringComparer.Ordinal);

	private static void AssertGeneratedSourcesEqual(
		SortedDictionary<string, string> expected,
		SortedDictionary<string, string> actual)
	{
		Assert.Equal(expected.Keys, actual.Keys);
		foreach (var hintName in expected.Keys)
		{
			Assert.True(
				string.Equals(expected[hintName], actual[hintName], StringComparison.Ordinal),
				$"Generated source '{hintName}' changed when only metadata reference order changed.{Environment.NewLine}" +
				$"Expected:{Environment.NewLine}{expected[hintName]}{Environment.NewLine}" +
				$"Actual:{Environment.NewLine}{actual[hintName]}");
		}
	}
}
