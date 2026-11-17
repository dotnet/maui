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

	private record AdditionalXamlFile(string Path)
		: AdditionalFile(
			Text: ToAdditionalText(Path, Xaml),
			Kind: "Xaml",
			RelativePath: Path,
			TargetPath: Path,
			ManifestResourceName: "Test.TestPage.xaml",
			TargetFramework: "net10.0",
			NoWarn: null);

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
