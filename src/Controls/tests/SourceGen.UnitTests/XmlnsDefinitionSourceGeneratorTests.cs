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
	[Test]
	public void TestXmlnsDefinitionGenerator_BasicGeneration()
	{
		var globalProperties = new Dictionary<string, string>
		{
			["build_property.MauiXmlnsDefinitions"] = "My.Local.Namespace.Here;My.External.Namespace.Here|MyAssemblyName"
		};

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionSourceGenerator>(compilation, globalProperties);

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName == "XmlnsDefinitionAttributes.g.cs").SourceText.ToString();

		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.Local.Namespace.Here\")]", StringComparison.Ordinal));
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.External.Namespace.Here\", AssemblyName = \"MyAssemblyName\")]", StringComparison.Ordinal));
		Assert.IsTrue(generated.Contains("using Microsoft.Maui.Controls;", StringComparison.Ordinal));
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_EmptyProperty()
	{
		var globalProperties = new Dictionary<string, string>
		{
			["build_property.MauiXmlnsDefinitions"] = ""
		};

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionSourceGenerator>(compilation, globalProperties);

		Assert.IsFalse(result.Diagnostics.Any());
		Assert.IsEmpty(result.Results.Single().GeneratedSources);
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_NoProperty()
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionSourceGenerator>(compilation, globalProperties: null);

		Assert.IsFalse(result.Diagnostics.Any());
		Assert.IsEmpty(result.Results.Single().GeneratedSources);
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_OnlyNamespaces()
	{
		var globalProperties = new Dictionary<string, string>
		{
			["build_property.MauiXmlnsDefinitions"] = "My.First.Namespace;My.Second.Namespace;My.Third.Namespace"
		};

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionSourceGenerator>(compilation, globalProperties);

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
		var globalProperties = new Dictionary<string, string>
		{
			["build_property.MauiXmlnsDefinitions"] = "My.First.Namespace|FirstAssembly;My.Second.Namespace|SecondAssembly"
		};

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionSourceGenerator>(compilation, globalProperties);

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName == "XmlnsDefinitionAttributes.g.cs").SourceText.ToString();

		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.First.Namespace\", AssemblyName = \"FirstAssembly\")]", StringComparison.Ordinal));
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.Second.Namespace\", AssemblyName = \"SecondAssembly\")]", StringComparison.Ordinal));
	}
}