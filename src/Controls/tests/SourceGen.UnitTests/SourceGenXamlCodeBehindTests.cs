using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class SourceGenXamlCodeBehindTests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn);

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

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		Assert.IsTrue(generated.Contains("Microsoft.Maui.Controls.Button MyButton", StringComparison.Ordinal));
		Assert.IsTrue(generated.Contains("public partial class TestPage : global::Microsoft.Maui.Controls.ContentPage", StringComparison.Ordinal));

	}

	[Test]
	public void TestCodeBehindGenerator_GlobalNamespace()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage x:Class="Test.TestPage">
		<Button x:Name="MyButton" Text="Hello MAUI!" />
</ContentPage>
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("[assembly: global::Microsoft.Maui.Controls.Xaml.Internals.AllowImplicitXmlnsDeclaration]"));
		var result = SourceGeneratorDriver.RunGenerator<CodeBehindGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		Assert.IsTrue(generated.Contains("Microsoft.Maui.Controls.Button MyButton", StringComparison.Ordinal));
	}

	[Test]
	public void TestCodeBehindGenerator_AggregatedXmlns()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/maui/global"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label x:Name="label" Text="Hello MAUI!" />
</ContentPage>
""";

		var code =
		"""
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "http://schemas.microsoft.com/dotnet/2021/maui")]
""";
		var compilation = CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
		var result = RunGenerator<CodeBehindGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs")).SourceText.ToString();

		Assert.IsTrue(generated.Contains("Microsoft.Maui.Controls.ContentPage", StringComparison.Ordinal));
		Assert.IsTrue(generated.Contains("global::Microsoft.Maui.Controls.Label label", StringComparison.Ordinal));
	}

	[Test]
	public void TestCodeBehindGenerator_AggregatedXmlnsOnRD()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ResourceDictionary
	xmlns="http://schemas.microsoft.com/dotnet/maui/global"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
	<x:String x:Key="MyString">Hello MAUI!</x:String>
</ResourceDictionary>
""";

		var code =
		"""
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "http://schemas.microsoft.com/dotnet/2021/maui")]
""";
		var compilation = CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
		var result = RunGenerator<CodeBehindGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs")).SourceText.ToString();

		Assert.IsTrue(generated.Contains("public partial class __Type", StringComparison.Ordinal));
	}

	public void TestCodeBehindGenerator_LocalXaml([Values] bool resolvedType)
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

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class TestControl : ContentView
{
}
""";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		if (resolvedType)
			compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));

		var result = SourceGeneratorDriver.RunGenerator<CodeBehindGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		if (resolvedType)
		{
			Assert.IsFalse(result.Diagnostics.Any());

			var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

			Assert.IsTrue(generated.Contains("Test.TestControl MyTestControl", StringComparison.Ordinal));
		}
		else
		{
			Assert.IsTrue(result.Diagnostics.Any(d => d.Descriptor.Id == "MAUIX2000"));
		}
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
		var output1 = result1.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();
		var output2 = result2.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		// Assert.IsTrue(result1.TrackedSteps.All(s => s.Value.Single().Outputs.Single().Reason == IncrementalStepRunReason.New));
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
			{ TrackingNames.XamlProjectItemProviderForCB, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlSourceProviderForCB, IncrementalStepRunReason.Cached }
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
		var output1 = result1.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();
		var output2 = result2.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		// Assert.IsTrue(result1.TrackedSteps.All(s => s.Value.Single().Outputs.Single().Reason == IncrementalStepRunReason.New));
		Assert.AreEqual(output1, output2);

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			return (driver, compilation.AddReferences(MetadataReference.CreateFromFile(typeof(SourceGenXamlCodeBehindTests).Assembly.Location)));
		}

		var expectedReasons = new Dictionary<string, IncrementalStepRunReason>
		{
			{ TrackingNames.ProjectItemProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlProjectItemProviderForCB, IncrementalStepRunReason.Modified },
			{ TrackingNames.ReferenceCompilationProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.XmlnsDefinitionsProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.ReferenceTypeCacheProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.XamlSourceProviderForCB, IncrementalStepRunReason.Modified }
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
		var output1 = result1.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();
		var output2 = result2.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		// Assert.IsTrue(result1.TrackedSteps.All(s => s.Value.Single().Outputs.Single().Reason == IncrementalStepRunReason.New));
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
			{ TrackingNames.XamlProjectItemProviderForCB, IncrementalStepRunReason.Modified },
			{ TrackingNames.ReferenceCompilationProvider, IncrementalStepRunReason.Unchanged },
			{ TrackingNames.XmlnsDefinitionsProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.ReferenceTypeCacheProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlSourceProviderForCB, IncrementalStepRunReason.Modified }
		};

		VerifyStepRunReasons(result2, expectedReasons);
	}

	[Test]
	public void TestCodeBehindGenerator_NotXaml()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<foo>
</foo>
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("[assembly: global::Microsoft.Maui.Controls.Xaml.Internals.AllowImplicitXmlnsDeclaration]"));
		var result = SourceGeneratorDriver.RunGenerator<CodeBehindGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs")).SourceText.ToString();
		Assert.That(result.Diagnostics.Any() || string.IsNullOrWhiteSpace(generated));
	}

	[Test]
	public void TestCodeBehindGenerator_ConflictingNames()
	{
		var code =
"""
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "http://schemas.microsoft.com/dotnet/2021/maui")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "Ns1")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "Ns2")]

namespace Ns1
{
	public class Conflicting : Label { }
}

namespace Ns2
{
	public class Conflicting : Label { }
}
""";

		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/maui/global"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
		<Conflicting x:Name="conflicting" Text="Hello MAUI!" />
</ContentPage>
""";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));

		var result = SourceGeneratorDriver.RunGenerator<CodeBehindGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.IsTrue(result.Diagnostics.Any());

		//var generated = result.Results.Single().GeneratedSources.Single().SourceText.ToString();
	}

	[Test]
	public void TestCodeBehindGenerator_DuplicateNames()
	{
		var xaml = """
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:ext="http://foo.bar/tests"
	x:Class="Test.TestPage">
	<StackLayout>
		<ext:PublicInExternal x:Name="publicInExternal" />
		<ext:PublicInHidden x:Name="publicInHidden" /> 
		<ext:PublicInVisible x:Name="publicInVisible" />
	</StackLayout>
</ContentPage>
""";

		var code = """
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
""";

		var externalone = """
using System.ComponentModel;
using Microsoft.Maui.Controls;

[assembly: XmlnsDefinition("http://foo.bar/tests", "External")]
[assembly: XmlnsDefinition("http://foo.bar/tests", "External", AssemblyName = "external2.Generated")]

namespace External;

public class PublicInExternal : Button { }
internal class PublicInHidden : Button { }
internal class PublicInVisible : Button { }
public class PublicWithSuffix : Button { }

""";

		var externaltwo = """
using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace External;

internal class PublicInExternal : Button { }
public class PublicInHidden : Button { }
internal class PublicInVisible : Button { }
internal class PublicWithSuffixExtension : Button { }
internal class InternalWithSuffixExtension : Button { }
""";

		var externalthree = """
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

[assembly: InternalsVisibleTo("SourceGeneratorDriver.Generated")]
[assembly: XmlnsDefinition("http://foo.bar/tests", "External")]

namespace External;

internal class InternalButVisible : Label { }
public class PublicInVisible : Button { }
internal class InternalWithSuffix : Button { }
""";


		var compilation = SourceGeneratorDriver.CreateMauiCompilation()
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code))
			.AddReferences(
				SourceGeneratorDriver.CreateMauiCompilation("external1.Generated").AddSyntaxTrees(CSharpSyntaxTree.ParseText(externalone)).ToMetadataReference(),
				SourceGeneratorDriver.CreateMauiCompilation("external2.Generated").AddSyntaxTrees(CSharpSyntaxTree.ParseText(externaltwo)).ToMetadataReference(),
				SourceGeneratorDriver.CreateMauiCompilation("external3.Generated").AddSyntaxTrees(CSharpSyntaxTree.ParseText(externalthree)).ToMetadataReference());

		var result = SourceGeneratorDriver.RunGenerator<CodeBehindGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs")).SourceText.ToString();

		Assert.IsTrue(generated.Contains("External.PublicInExternal publicInExternal", StringComparison.Ordinal));
	}

	[Test]
	public void TestCodeBehindGenerator_InternalsVisibleTo()
	{
		var xaml = """
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:ext="http://foo.bar/tests"
	x:Class="Test.TestPage">
	<StackLayout>
		<ext:InternalButVisible x:Name="internalButVisible" />
	</StackLayout>
</ContentPage>
""";

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class TestControl : ContentView
{
}
""";

		var externalcode =
"""
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

[assembly: InternalsVisibleTo("SourceGeneratorDriver.Generated")]
[assembly: XmlnsDefinition("http://foo.bar/tests", "External")]

namespace External;

internal class InternalButVisible : Label { }
""";


		var externalCompilation = SourceGeneratorDriver.CreateMauiCompilation("external.Generated").AddSyntaxTrees(CSharpSyntaxTree.ParseText(externalcode));
		var compilation = SourceGeneratorDriver.CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(code)).AddReferences(externalCompilation.ToMetadataReference());


		var result = SourceGeneratorDriver.RunGenerator<CodeBehindGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.IsFalse(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs")).SourceText.ToString();

		Assert.IsTrue(generated.Contains("External.InternalButVisible internalButVisible", StringComparison.Ordinal));
	}
}
