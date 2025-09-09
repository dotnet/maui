using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class SourceGenXamlCodeBehindTests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null, bool TreeOrder = false)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn, TreeOrder: TreeOrder);

	[Fact]
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
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.False(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		Assert.True(generated.Contains("Microsoft.Maui.Controls.Button MyButton", StringComparison.Ordinal));
		Assert.True(generated.Contains("public partial class TestPage : global::Microsoft.Maui.Controls.ContentPage", StringComparison.Ordinal));

	}

	[Fact]
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
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.False(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		Assert.True(generated.Contains("Microsoft.Maui.Controls.Button MyButton", StringComparison.Ordinal));
	}

	[Fact]
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
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.False(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs")).SourceText.ToString();

		Assert.True(generated.Contains("Microsoft.Maui.Controls.ContentPage", StringComparison.Ordinal));
		Assert.True(generated.Contains("global::Microsoft.Maui.Controls.Label label", StringComparison.Ordinal));
	}

	[Fact]
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
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.False(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs")).SourceText.ToString();

		Assert.True(generated.Contains("public partial class __Type", StringComparison.Ordinal));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void TestCodeBehindGenerator_LocalXaml(bool resolvedType)
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

		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		if (resolvedType)
		{
			Assert.False(result.Diagnostics.Any());

			var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

			Assert.True(generated.Contains("Test.TestControl MyTestControl", StringComparison.Ordinal));
		}
		else
		{
			Assert.True(result.Diagnostics.Any(d => d.Descriptor.Id == "MAUIX2000"));
		}
	}

	[Fact]
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
		var result = SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(compilation, ApplyChanges, xamlFile);

		var result1 = result.result1.Results.Single();
		var result2 = result.result2.Results.Single();
		var output1 = result1.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();
		var output2 = result2.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		// Assert.IsTrue(result1.TrackedSteps.All(s => s.Value.Single().Outputs.Single().Reason == IncrementalStepRunReason.New));
		Assert.Equal(output1, output2);

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			return (driver, compilation.Clone());
		}

		var expectedReasons = new Dictionary<string, IncrementalStepRunReason>
		{
			{ TrackingNames.ProjectItemProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.CompilationProvider, IncrementalStepRunReason.Unchanged },
			{ TrackingNames.ReferenceTypeCacheProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XmlnsDefinitionsProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlProjectItemProviderForCB, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlSourceProviderForCB, IncrementalStepRunReason.Unchanged }
		};

		VerifyStepRunReasons(result2, expectedReasons);
	}

	/// <summary>
	/// Verifies that changing method implementation (body) does not trigger XAML regeneration.
	/// This is important for IDE responsiveness - typing in method bodies should not regenerate all XAML.
	/// </summary>
	[Fact]
	public void TestCodeBehindGenerator_ImplementationChangeDoesNotTriggerRegeneration()
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
		// Initial C# code with a method
		var initialCode =
"""
namespace Test;
public class Helper 
{ 
    public void DoWork() 
    { 
        var x = 1; 
    } 
}
""";
		// Modified C# code - only method body changed
		var modifiedCode =
"""
namespace Test;
public class Helper 
{ 
    public void DoWork() 
    { 
        var x = 2; // Changed implementation
        var y = 3;
    } 
}
""";

		var xamlFile = new AdditionalXamlFile("Test.xaml", xaml);
		var compilation = SourceGeneratorDriver.CreateMauiCompilation()
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(initialCode, path: "Helper.cs"));
		var result = SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(compilation, ApplyChanges, xamlFile);

		var result1 = result.result1.Results.Single();
		var result2 = result.result2.Results.Single();
		var output1 = result1.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();
		var output2 = result2.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		// Output should be identical
		Assert.Equal(output1, output2);

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			// Replace the syntax tree with modified implementation
			var oldTree = compilation.SyntaxTrees.First(t => t.FilePath.EndsWith("Helper.cs", StringComparison.Ordinal));
			var newTree = CSharpSyntaxTree.ParseText(modifiedCode, path: "Helper.cs");
			return (driver, compilation.ReplaceSyntaxTree(oldTree, newTree));
		}

		// Key assertion: CompilationProvider should be Unchanged because only implementation changed
		// This means the CompilationSignaturesComparer correctly identified that signatures are the same
		var expectedReasons = new Dictionary<string, IncrementalStepRunReason>
		{
			{ TrackingNames.ProjectItemProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.CompilationProvider, IncrementalStepRunReason.Unchanged },
			{ TrackingNames.ReferenceTypeCacheProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XmlnsDefinitionsProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlProjectItemProviderForCB, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlSourceProviderForCB, IncrementalStepRunReason.Unchanged }
		};

		VerifyStepRunReasons(result2, expectedReasons);
	}

	/// <summary>
	/// Verifies that adding a new public member DOES trigger XAML regeneration.
	/// This ensures the CompilationSignaturesComparer detects signature changes.
	/// </summary>
	[Fact]
	public void TestCodeBehindGenerator_SignatureChangeTriggersRegeneration()
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
		// Initial C# code
		var initialCode =
"""
namespace Test;
public class Helper 
{ 
    public void DoWork() { } 
}
""";
		// Modified C# code - added a new public method (signature change)
		var modifiedCode =
"""
namespace Test;
public class Helper 
{ 
    public void DoWork() { }
    public void DoMoreWork() { } // New method - signature change
}
""";

		var xamlFile = new AdditionalXamlFile("Test.xaml", xaml);
		var compilation = SourceGeneratorDriver.CreateMauiCompilation()
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(initialCode, path: "Helper.cs"));
		var result = SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(compilation, ApplyChanges, xamlFile);

		var result1 = result.result1.Results.Single();
		var result2 = result.result2.Results.Single();

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			var oldTree = compilation.SyntaxTrees.First(t => t.FilePath.EndsWith("Helper.cs", StringComparison.Ordinal));
			var newTree = CSharpSyntaxTree.ParseText(modifiedCode, path: "Helper.cs");
			return (driver, compilation.ReplaceSyntaxTree(oldTree, newTree));
		}

		// Key assertion: CompilationProvider should be Modified because a new method was added
		var expectedReasons = new Dictionary<string, IncrementalStepRunReason>
		{
			{ TrackingNames.ProjectItemProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.CompilationProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.ReferenceTypeCacheProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.XmlnsDefinitionsProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.XamlProjectItemProviderForCB, IncrementalStepRunReason.Modified },
			{ TrackingNames.XamlSourceProviderForCB, IncrementalStepRunReason.Modified }
		};

		VerifyStepRunReasons(result2, expectedReasons);
	}

	[Fact]
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
		var result = SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(compilation, ApplyChanges, xamlFile);

		var result1 = result.result1.Results.Single();
		var result2 = result.result2.Results.Single();
		var output1 = result1.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();
		var output2 = result2.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		// Assert.IsTrue(result1.TrackedSteps.All(s => s.Value.Single().Outputs.Single().Reason == IncrementalStepRunReason.New));
		Assert.Equal(output1, output2);

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			return (driver, compilation.AddReferences(MetadataReference.CreateFromFile(typeof(SourceGenXamlCodeBehindTests).Assembly.Location)));
		}

		var expectedReasons = new Dictionary<string, IncrementalStepRunReason>
		{
			{ TrackingNames.ProjectItemProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlProjectItemProviderForCB, IncrementalStepRunReason.Modified },
			{ TrackingNames.CompilationProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.XmlnsDefinitionsProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.ReferenceTypeCacheProvider, IncrementalStepRunReason.Modified },
			{ TrackingNames.XamlSourceProviderForCB, IncrementalStepRunReason.Modified }
		};

		VerifyStepRunReasons(result2, expectedReasons);
	}

	[Fact]
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
		var result = SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(compilation, ApplyChanges, xamlFile);

		var result1 = result.result1.Results.Single();
		var result2 = result.result2.Results.Single();
		var output1 = result1.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();
		var output2 = result2.GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		// Assert.IsTrue(result1.TrackedSteps.All(s => s.Value.Single().Outputs.Single().Reason == IncrementalStepRunReason.New));
		Assert.NotEqual(output1, output2);

		Assert.True(output1.Contains("MyButton", StringComparison.Ordinal));
		Assert.False(output1.Contains("MyButton2", StringComparison.Ordinal));
		Assert.True(output2.Contains("MyButton", StringComparison.Ordinal));
		Assert.True(output2.Contains("MyButton2", StringComparison.Ordinal));

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
			{ TrackingNames.CompilationProvider, IncrementalStepRunReason.Unchanged },
			{ TrackingNames.XmlnsDefinitionsProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.ReferenceTypeCacheProvider, IncrementalStepRunReason.Cached },
			{ TrackingNames.XamlSourceProviderForCB, IncrementalStepRunReason.Modified }
		};

		VerifyStepRunReasons(result2, expectedReasons);
	}

	[Fact]
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
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs")).SourceText.ToString();
		Assert.True(result.Diagnostics.Any() || string.IsNullOrWhiteSpace(generated));
	}

	[Fact]
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

		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.True(result.Diagnostics.Any());

		//var generated = result.Results.Single().GeneratedSources.Single().SourceText.ToString();
	}

	[Fact]
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

		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.False(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs")).SourceText.ToString();

		Assert.True(generated.Contains("External.PublicInExternal publicInExternal", StringComparison.Ordinal));
	}

	[Fact]
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


		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.False(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs")).SourceText.ToString();

		Assert.True(generated.Contains("External.InternalButVisible internalButVisible", StringComparison.Ordinal));
	}

	[Fact]
	public void TestCodeBehindGenerator_OnPlatform_Android()
	{
		// Issue #32521: Named controls under OnPlatform should only generate fields for the target platform
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<OnPlatform x:TypeArguments="ScrollView">
		<On Platform="Android">
			<ScrollView>
				<Button x:Name="CounterBtnAndroid" Text="Click me Android" />
			</ScrollView>
		</On>
		<On Platform="iOS">
			<ScrollView>
				<Button x:Name="CounterBtnIOS" Text="Click me iOS" />
			</ScrollView>
		</On>
	</OnPlatform>
</ContentPage>
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml, TargetFramework: "net10.0-android"));

		Assert.False(result.Diagnostics.Any());

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs")).SourceText.ToString();

		// Should contain Android button field
		Assert.Contains("CounterBtnAndroid", generated, StringComparison.Ordinal);
		// Should NOT contain iOS button field
		Assert.DoesNotContain("CounterBtnIOS", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void TestCodeBehindGenerator_StyleWithSetterInResourceDictionary()
	{
		// Regression test: Style with Setter in ResourceDictionary generates invalid C#
		// The generated code was producing "(object).ProvideValue()" instead of 
		// "(object)variableName.ProvideValue()" because the variable name was empty.
		// 
		// Uses a ContentPage with x:Class and embedded Style, which ensures the .xsg.cs
		// file is generated and exercises the DependencyFirstInflator code path.
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    x:Class="Test.TestPage">
  <ContentPage.Resources>
    <Style TargetType="Button" x:Key="style">
      <Setter Property="BackgroundColor" Value="HotPink"/>
    </Style>
  </ContentPage.Resources>
  <Button Style="{StaticResource style}" Text="Test"/>
</ContentPage>
""";

		var code =
"""
using Microsoft.Maui.Controls;

namespace Test;

public partial class TestPage : ContentPage
{
    public TestPage() => InitializeComponent();
}
""";

		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
		
		// TreeOrder = true triggers the DependencyFirstInflator code path which had the bug
		// Pass assertNoCompilationErrors = false because the generated code uses C# 14 'field' keyword
		// which isn't available in unit test compilation
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml, TreeOrder: true), assertNoCompilationErrors: false);

		// Check for generator diagnostics (not compilation errors)
		Assert.False(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
		
		// Get all generated sources
		var generatedSources = result.Results.Single().GeneratedSources;
		
		// Get the .xsg.cs file
		var xsgSource = generatedSources.FirstOrDefault(gs => gs.HintName.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase));
		Assert.NotNull(xsgSource.SourceText);
		
		var generatedCode = xsgSource.SourceText.ToString();
		// This is the key assertion - the bug generates "(object).ProvideValue" which is invalid C#
		// The correct code should be "(object)variableName.ProvideValue(...)"
		Assert.DoesNotContain("(object).ProvideValue", generatedCode, StringComparison.Ordinal);
	}
}
