using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class XmlnsDefinitionSourceGeneratorTests : SourceGenTestsBase
{
	private record XmlnsDefinitionDataFile(string Path, string Content)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "XmlnsDefinition", RelativePath: Path, TargetPath: null, ManifestResourceName: null, TargetFramework: null);

	[Test]
	public void TestXmlnsDefinitionGenerator_BasicGeneration()
	{
		var xmlnsData = """
			My.Local.Namespace.Here
			My.External.Namespace.Here|MyAssemblyName
			""";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var dataFile = new XmlnsDefinitionDataFile("XmlnsDefinitions.xmlnsdefinitions", xmlnsData);
		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionSourceGenerator>(compilation, dataFile);

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName == "XmlnsDefinitionAttributes.g.cs").SourceText.ToString();

		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.Local.Namespace.Here\")]", StringComparison.Ordinal));
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.External.Namespace.Here\", AssemblyName = \"MyAssemblyName\")]", StringComparison.Ordinal));
		Assert.IsTrue(generated.Contains("using Microsoft.Maui.Controls;", StringComparison.Ordinal));
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_EmptyFile()
	{
		var xmlnsData = "";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var dataFile = new XmlnsDefinitionDataFile("XmlnsDefinitions.xmlnsdefinitions", xmlnsData);
		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionSourceGenerator>(compilation, dataFile);

		Assert.IsFalse(result.Diagnostics.Any());
		Assert.IsEmpty(result.Results.Single().GeneratedSources);
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_WithComments()
	{
		var xmlnsData = """
			# This is a comment
			My.Namespace.One
			# Another comment
			My.Namespace.Two|Assembly.Name
			""";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var dataFile = new XmlnsDefinitionDataFile("XmlnsDefinitions.xmlnsdefinitions", xmlnsData);
		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionSourceGenerator>(compilation, dataFile);

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName == "XmlnsDefinitionAttributes.g.cs").SourceText.ToString();

		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.Namespace.One\")]", StringComparison.Ordinal));
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.Namespace.Two\", AssemblyName = \"Assembly.Name\")]", StringComparison.Ordinal));
		Assert.IsFalse(generated.Contains("comment", StringComparison.OrdinalIgnoreCase));
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_OnlyNamespaces()
	{
		var xmlnsData = """
			My.First.Namespace
			My.Second.Namespace
			My.Third.Namespace
			""";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var dataFile = new XmlnsDefinitionDataFile("XmlnsDefinitions.xmlnsdefinitions", xmlnsData);
		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionSourceGenerator>(compilation, dataFile);

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName == "XmlnsDefinitionAttributes.g.cs").SourceText.ToString();

		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.First.Namespace\")]", StringComparison.Ordinal));
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.Second.Namespace\")]", StringComparison.Ordinal));
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.Third.Namespace\")]", StringComparison.Ordinal));
		Assert.IsFalse(generated.Contains("AssemblyName", StringComparison.Ordinal));
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_OnlyNamespacesWithAssemblies()
	{
		var xmlnsData = """
			My.First.Namespace|FirstAssembly
			My.Second.Namespace|SecondAssembly
			""";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var dataFile = new XmlnsDefinitionDataFile("XmlnsDefinitions.xmlnsdefinitions", xmlnsData);
		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionSourceGenerator>(compilation, dataFile);

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName == "XmlnsDefinitionAttributes.g.cs").SourceText.ToString();

		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.First.Namespace\", AssemblyName = \"FirstAssembly\")]", StringComparison.Ordinal));
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.Second.Namespace\", AssemblyName = \"SecondAssembly\")]", StringComparison.Ordinal));
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_ModifiedFile()
	{
		var originalData = """
			My.Original.Namespace
			""";

		var modifiedData = """
			My.Original.Namespace
			My.New.Namespace|NewAssembly
			""";

		var dataFile = new XmlnsDefinitionDataFile("XmlnsDefinitions.xmlnsdefinitions", originalData);
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGeneratorWithChanges<XmlnsDefinitionSourceGenerator>(compilation, ApplyChanges, dataFile);

		var result1 = result.result1.Results.Single();
		var result2 = result.result2.Results.Single();
		var output1 = result1.GeneratedSources.Single(gs => gs.HintName == "XmlnsDefinitionAttributes.g.cs").SourceText.ToString();
		var output2 = result2.GeneratedSources.Single(gs => gs.HintName == "XmlnsDefinitionAttributes.g.cs").SourceText.ToString();

		Assert.AreNotEqual(output1, output2);
		Assert.IsTrue(output1.Contains("My.Original.Namespace", StringComparison.Ordinal));
		Assert.IsFalse(output1.Contains("My.New.Namespace", StringComparison.Ordinal));
		
		Assert.IsTrue(output2.Contains("My.Original.Namespace", StringComparison.Ordinal));
		Assert.IsTrue(output2.Contains("My.New.Namespace", StringComparison.Ordinal));
		Assert.IsTrue(output2.Contains("NewAssembly", StringComparison.Ordinal));

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			var newDataFile = new XmlnsDefinitionDataFile("XmlnsDefinitions.xmlnsdefinitions", modifiedData);
			driver = driver.ReplaceAdditionalText(dataFile.Text, newDataFile.Text);
			return (driver, compilation);
		}

		var expectedReasons = new Dictionary<string, IncrementalStepRunReason>
		{
			{ TrackingNames.XmlnsDefinitionDataProvider, IncrementalStepRunReason.Modified }
		};

		VerifyStepRunReasons(result2, expectedReasons);
	}
}