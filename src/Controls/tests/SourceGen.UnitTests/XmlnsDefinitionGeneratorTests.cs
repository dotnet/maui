using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class XmlnsDefinitionGeneratorTests : SourceGenTestsBase
{
	[Test]
	public void TestXmlnsDefinitionGenerator_NoItems()
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionGenerator>(compilation);

		Assert.IsFalse(result.Diagnostics.Any());
		Assert.AreEqual(0, result.Results.Single().GeneratedSources.Length, "No source should be generated when no XmlnsDefinition items are present");
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_SingleItemWithoutAssemblyName()
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var globalOptions = new Dictionary<string, string>
		{
			{ "build_property.XmlnsDefinitionItems", "My.Local.Namespace.Here" }
		};

		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionGenerator>(compilation, globalOptions: globalOptions);

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single().SourceText.ToString();
		
		Assert.IsTrue(generated.Contains("using Microsoft.Maui.Controls;", StringComparison.Ordinal), "Should include using directive");
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.Local.Namespace.Here\")]", StringComparison.Ordinal), 
			"Should generate XmlnsDefinition attribute without AssemblyName");
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_SingleItemWithAssemblyName()
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var globalOptions = new Dictionary<string, string>
		{
			{ "build_property.XmlnsDefinitionItems", "My.External.Namespace.Here|MyAssemblyName" }
		};

		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionGenerator>(compilation, globalOptions: globalOptions);

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single().SourceText.ToString();
		
		Assert.IsTrue(generated.Contains("using Microsoft.Maui.Controls;", StringComparison.Ordinal), "Should include using directive");
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.External.Namespace.Here\", AssemblyName = \"MyAssemblyName\")]", StringComparison.Ordinal), 
			"Should generate XmlnsDefinition attribute with AssemblyName");
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_MultipleItems()
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var globalOptions = new Dictionary<string, string>
		{
			{ "build_property.XmlnsDefinitionItems", "My.Local.Namespace.Here;My.External.Namespace.Here|MyAssemblyName;Another.Namespace" }
		};

		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionGenerator>(compilation, globalOptions: globalOptions);

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single().SourceText.ToString();
		
		Assert.IsTrue(generated.Contains("using Microsoft.Maui.Controls;", StringComparison.Ordinal), "Should include using directive");
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.Local.Namespace.Here\")]", StringComparison.Ordinal), 
			"Should generate first XmlnsDefinition attribute");
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.External.Namespace.Here\", AssemblyName = \"MyAssemblyName\")]", StringComparison.Ordinal), 
			"Should generate second XmlnsDefinition attribute with AssemblyName");
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"Another.Namespace\")]", StringComparison.Ordinal), 
			"Should generate third XmlnsDefinition attribute");
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_EmptyAndWhitespaceValues()
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var globalOptions = new Dictionary<string, string>
		{
			{ "build_property.XmlnsDefinitionItems", "My.Valid.Namespace;; |;  ;Valid.Namespace.Too" }
		};

		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionGenerator>(compilation, globalOptions: globalOptions);

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single().SourceText.ToString();
		
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.Valid.Namespace\")]", StringComparison.Ordinal), 
			"Should generate first valid XmlnsDefinition attribute");
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"Valid.Namespace.Too\")]", StringComparison.Ordinal), 
			"Should generate second valid XmlnsDefinition attribute");
		
		// Count occurrences to ensure only 2 attributes are generated
		var attributeCount = generated.Split(new[] { "[assembly: XmlnsDefinition(" }, StringSplitOptions.None).Length - 1;
		Assert.AreEqual(2, attributeCount, "Should only generate 2 XmlnsDefinition attributes, ignoring empty/whitespace values");
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_EmptyProperty()
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var globalOptions = new Dictionary<string, string>
		{
			{ "build_property.XmlnsDefinitionItems", "" }
		};

		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionGenerator>(compilation, globalOptions: globalOptions);

		Assert.IsFalse(result.Diagnostics.Any());
		Assert.AreEqual(0, result.Results.Single().GeneratedSources.Length, "No source should be generated when property is empty");
	}

	[Test]
	public void TestXmlnsDefinitionGenerator_ItemsWithEmptyAssemblyName()
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var globalOptions = new Dictionary<string, string>
		{
			{ "build_property.XmlnsDefinitionItems", "My.Namespace|;Another.Namespace|  " }
		};

		var result = SourceGeneratorDriver.RunGenerator<XmlnsDefinitionGenerator>(compilation, globalOptions: globalOptions);

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single().SourceText.ToString();
		
		// When AssemblyName is empty or whitespace, it should be treated as null
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"My.Namespace\")]", StringComparison.Ordinal), 
			"Should generate XmlnsDefinition attribute without AssemblyName when it's empty");
		Assert.IsTrue(generated.Contains("[assembly: XmlnsDefinition(\"http://schemas.microsoft.com/dotnet/maui/global\", \"Another.Namespace\")]", StringComparison.Ordinal), 
			"Should generate XmlnsDefinition attribute without AssemblyName when it's whitespace");
	}
}