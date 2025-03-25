using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class SourceGenXamlTests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework);

	[Test]
	public void TestCodeBehindGenerator_BasicXaml()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
		<Button x:Name="MyButton" Text="Hello MAUI!" />
</ContentPage>
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGenerator<CodeBehindGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single().SourceText.ToString();

		Assert.IsTrue(generated.Contains("Microsoft.Maui.Controls.Button MyButton", StringComparison.Ordinal));
	}

	[Test]
	public void TestCodeBehindGenerator_LocalXaml()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Test"
	x:Class="Test.TestPage">
		<local:TestControl x:Name="MyTestControl" />
</ContentPage>
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGenerator<CodeBehindGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single().SourceText.ToString();

		Assert.IsTrue(generated.Contains("Test.TestControl MyTestControl", StringComparison.Ordinal));
	}

	[Test]
	public void TestCodeBehindGenerator_CompilationClone()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
		<Button x:Name="MyButton" Text="Hello MAUI!" />
</ContentPage>
""";
		var xamlFile = new AdditionalXamlFile("Test.xaml", xaml);
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGeneratorWithChanges<CodeBehindGenerator>(compilation, ApplyChanges, xamlFile);

		var result1 = result.result1.Results.Single();
		var result2 = result.result2.Results.Single();
		var output1 = result1.GeneratedSources.Single().SourceText.ToString();
		var output2 = result2.GeneratedSources.Single().SourceText.ToString();

		Assert.IsTrue(result1.TrackedSteps.All(s => s.Value.Single().Outputs.Single().Reason == IncrementalStepRunReason.New));
		Assert.AreEqual(output1, output2);

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			return (driver, compilation.Clone());
		}

		var expectedReasons = new Dictionary<string, IncrementalStepRunReason>
		{
			{ TrackingNames.ProjectItemProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.ReferenceCompilationProvider, IncrementalStepRunReason.Unchanged },
			{ TrackingNames.ReferenceTypeCacheProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XmlnsDefinitionsProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlProjectItemProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlSourceProvider, IncrementalStepRunReason.Cached }
		};

		VerifyStepRunReasons(result2, expectedReasons);
	}

	[Test]
	public void TestCodeBehindGenerator_ReferenceAdded()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
		<Button x:Name="MyButton" Text="Hello MAUI!" />
</ContentPage>
""";
		var xamlFile = new AdditionalXamlFile("Test.xaml", xaml);
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGeneratorWithChanges<CodeBehindGenerator>(compilation, ApplyChanges, xamlFile);

		var result1 = result.result1.Results.Single();
		var result2 = result.result2.Results.Single();
		var output1 = result1.GeneratedSources.Single().SourceText.ToString();
		var output2 = result2.GeneratedSources.Single().SourceText.ToString();

		Assert.IsTrue(result1.TrackedSteps.All(s => s.Value.Single().Outputs.Single().Reason == IncrementalStepRunReason.New));
		Assert.AreEqual(output1, output2);

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			return (driver, compilation.AddReferences(MetadataReference.CreateFromFile(typeof(SourceGenXamlTests).Assembly.Location)));
		}

		var expectedReasons = new Dictionary<string, IncrementalStepRunReason>
		{
			{ TrackingNames.ProjectItemProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlProjectItemProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.ReferenceCompilationProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.XmlnsDefinitionsProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.ReferenceTypeCacheProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.XamlSourceProvider, IncrementalStepRunReason.Modified }
		};

		VerifyStepRunReasons(result2, expectedReasons);
	}

	[Test]
	public void TestCodeBehindGenerator_ModifiedXaml()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
		<Button x:Name="MyButton" Text="Hello MAUI!" />
</ContentPage>
""";
		var newXaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
		<Button x:Name="MyButton" Text="Hello MAUI!" />
		<Button x:Name="MyButton2" Text="Hello MAUI!" />
</ContentPage>
""";
		var xamlFile = new AdditionalXamlFile("Test.xaml", xaml);
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGeneratorWithChanges<CodeBehindGenerator>(compilation, ApplyChanges, xamlFile);

		var result1 = result.result1.Results.Single();
		var result2 = result.result2.Results.Single();
		var output1 = result1.GeneratedSources.Single().SourceText.ToString();
		var output2 = result2.GeneratedSources.Single().SourceText.ToString();

		Assert.IsTrue(result1.TrackedSteps.All(s => s.Value.Single().Outputs.Single().Reason == IncrementalStepRunReason.New));
		Assert.AreNotEqual(output1, output2);

		Assert.IsTrue(output1.Contains("MyButton", StringComparison.Ordinal));
		Assert.IsFalse(output1.Contains("MyButton2", StringComparison.Ordinal));
		Assert.IsTrue(output2.Contains("MyButton", StringComparison.Ordinal));
		Assert.IsTrue(output2.Contains("MyButton2", StringComparison.Ordinal));

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			var newXamlFile = new AdditionalXamlFile(xamlFile.Path, newXaml);
			driver = driver.ReplaceAdditionalText(xamlFile.Text, newXamlFile.Text);
			return (driver, compilation);
		}

		var expectedReasons = new Dictionary<string, IncrementalStepRunReason>
		{
			{ TrackingNames.ProjectItemProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.XamlProjectItemProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.ReferenceCompilationProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XmlnsDefinitionsProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.ReferenceTypeCacheProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlSourceProvider, IncrementalStepRunReason.Modified }
		};

		VerifyStepRunReasons(result2, expectedReasons);
	}
}
