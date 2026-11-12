using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class SourceGenXamlCodeBehindTests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null, string Inflator = "SourceGen")
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn, Inflator: Inflator);

	private const string CollisionTypes = """
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
[assembly: XmlnsDefinition("http://tests/collision", "One")]
[assembly: XmlnsDefinition("http://tests/collision", "Two")]
[assembly: XmlnsDefinition("http://tests/single", "Only")]
namespace One
{
	public class Conflict : Label { }
	public class ChoiceExtension : IMarkupExtension { public object ProvideValue(IServiceProvider serviceProvider) => null; }
	public class Owner : BindableObject { public static string Value => ""; }
	public class TypeName { }
	public class Value { }
	public class TypeArguments { }
}
namespace Two
{
	public class Conflict : Label { }
	public class ChoiceExtension : IMarkupExtension { public object ProvideValue(IServiceProvider serviceProvider) => null; }
	public class Owner : BindableObject { public static string Value => ""; }
	public class TypeName { }
	public class Value { }
	public class TypeArguments { }
}
namespace Only { public class Conflict : Label { } }
namespace Test
{
	public class Box<T> : ContentView { }
	public class TypeHolder : ContentView
	{
		public Type Type { get; set; }
		[System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))]
		public object ConvertedType { get; set; }
	}
	public class GenericExtension<T> : IMarkupExtension { public object ProvideValue(IServiceProvider serviceProvider) => null; }
	public class TypeOwner : BindableObject
	{
		public static Type GetValue(BindableObject target) => null;
		public static void SetValue(BindableObject target, Type value) { }
	}
	[ContentProperty(nameof(Value))]
	public class TypeValueExtension : IMarkupExtension
	{
		public Type Value { get; set; }
		public object ProvideValue(IServiceProvider serviceProvider) => Value;
	}
}
""";

	private static Compilation CreateCollisionCompilation()
		=> CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(CollisionTypes));

	private static Diagnostic[] ConflictWarnings(GeneratorDriverRunResult result)
		=> result.Diagnostics.Where(d => d.Id == "MAUIX2020").OrderBy(d => d.Location.SourceSpan.Start).ToArray();

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

	[Theory]
	[InlineData("Runtime", false, false)]
	[InlineData("XamlC", false, false)]
	[InlineData("SourceGen", false, false)]
	[InlineData("Runtime", true, false)]
	[InlineData("XamlC", true, false)]
	[InlineData("SourceGen", true, false)]
	[InlineData("Runtime", false, true)]
	[InlineData("SourceGen", false, true)]
	public void ConflictingReferencedElementProducesDesignTimeWarning(string inflator, bool disableImplementationOutput, bool named)
	{
		const string code = """
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://tests/collision", "Ns1")]
[assembly: XmlnsDefinition("http://tests/collision", "Ns2")]
namespace Ns1 { public class Conflicting : Label { } }
namespace Ns2 { public class Conflicting : Label { } }
""";
		var xaml = $$"""
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision"
             x:Class="Test.TestPage">
    <shared:Conflicting{{(named ? " x:Name=\"named\"" : "")}} />
</ContentPage>
""";
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
		var result = RunGenerator<XamlGenerator>(compilation,
			new AdditionalXamlFile("Test.xaml", xaml, Inflator: inflator),
			assertNoCompilationErrors: false,
			disabledOutputs: disableImplementationOutput ? IncrementalGeneratorOutputKind.Implementation : IncrementalGeneratorOutputKind.None);

		var warning = Assert.Single(result.Diagnostics.Where(d => d.Id == "MAUIX2020"));
		Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
		Assert.Equal("Test.xaml", warning.Location.GetLineSpan().Path);
		Assert.Equal("shared:Conflicting", xaml.Substring(warning.Location.SourceSpan.Start, warning.Location.SourceSpan.Length));
		Assert.Equal(xaml.IndexOf("shared:Conflicting", StringComparison.Ordinal), warning.Location.SourceSpan.Start);
		Assert.Contains("http://tests/collision", warning.GetMessage(), StringComparison.Ordinal);
		Assert.Contains("Ns1.Conflicting", warning.GetMessage(), StringComparison.Ordinal);
		Assert.Contains("Ns2.Conflicting", warning.GetMessage(), StringComparison.Ordinal);
		if (named)
			Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2001" && d.Severity == DiagnosticSeverity.Error);
	}

	[Fact]
	public void WarnsForReferencedTypesInXamlSyntax()
	{
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision"
             xmlns:local="clr-namespace:Test"
             x:Class="Test.TestPage">
  <StackLayout>
    <shared:Conflict />
    <local:Box x:TypeArguments="shared:Conflict" />
    <local:Box x:TypeArguments="local:Box(shared:Conflict)" />
    <Label Text="{shared:Choice}" />
    <Label Text="{Binding Converter={shared:Choice}}" />
    <Label BindingContext="{x:Type shared:Conflict}" />
    <Label BindingContext="{x:Static shared:Owner.Value}" />
    <Label x:DataType="shared:Conflict" />
    <Label shared:Owner.Value="1" />
    <Label><shared:Owner.Value>1</shared:Owner.Value></Label>
    <Style TargetType="shared:Conflict" />
    <x:Array Type="shared:Conflict" />
    <local:TypeHolder Type="shared:Conflict" />
    <local:TypeHolder Type="{x:Type TypeName=shared:Conflict}" />
    <local:TypeHolder Type="{local:TypeValue Value=shared:Conflict}" />
  </StackLayout>
</ContentPage>
""";
		var result = RunGenerator<XamlGenerator>(CreateCollisionCompilation(),
			new AdditionalXamlFile("References.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		var warnings = ConflictWarnings(result);
		var actual = warnings.Select(w => xaml.Substring(w.Location.SourceSpan.Start, w.Location.SourceSpan.Length)).ToArray();
		Assert.Equal(new[]
		{
			"shared:Conflict", "shared:Conflict", "shared:Conflict",
			"shared:Choice", "shared:Choice", "shared:Conflict", "shared:Owner",
			"shared:Conflict", "shared:Owner", "shared:Owner", "shared:Conflict",
			"shared:Conflict", "shared:Conflict", "shared:Conflict", "shared:Conflict"
		}, actual);
		Assert.All(warnings, warning => Assert.Equal("References.xaml", warning.Location.GetLineSpan().Path));
		Assert.Equal(warnings.Length, warnings.Select(w => w.Location.SourceSpan).Distinct().Count());
	}

	[Fact]
	public void IgnoresUnusedStringsScopedAliasesAndIgnoredMarkup()
	{
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision"
             xmlns:d="http://tests/collision"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d" x:Class="Test.TestPage">
  <StackLayout xmlns:shared="http://tests/single">
    <shared:Conflict />
    <Label Text="shared:Conflict" />
    <Label Text="{}{shared:Choice}" />
    <Label Text="{Binding Path='shared:Conflict'}" />
    <d:Conflict />
    <Label d:Text="{d:Choice}" />
  </StackLayout>
</ContentPage>
""";
		var result = RunGenerator<XamlGenerator>(CreateCollisionCompilation(),
			new AdditionalXamlFile("Ignored.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		Assert.Empty(ConflictWarnings(result));
	}

	[Theory]
	[InlineData("MAUIX2020")]
	[InlineData("2020")]
	[InlineData("MAUIX2005; mauix2020")]
	public void SupportsMauiXamlNoWarn(string noWarn)
	{
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision" x:Class="Test.TestPage">
  <shared:Conflict />
</ContentPage>
""";
		var result = RunGenerator<XamlGenerator>(CreateCollisionCompilation(),
			new AdditionalXamlFile("Suppressed.xaml", xaml, NoWarn: noWarn, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		Assert.Empty(ConflictWarnings(result));
	}

	[Fact]
	public void SupportsRoslynDiagnosticSuppression()
	{
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision" x:Class="Test.TestPage">
  <shared:Conflict />
</ContentPage>
""";
		var compilation = (CSharpCompilation)CreateCollisionCompilation();
		compilation = compilation.WithOptions(compilation.Options.WithSpecificDiagnosticOptions(
			ImmutableDictionary<string, ReportDiagnostic>.Empty.Add("MAUIX2020", ReportDiagnostic.Suppress)));
		var result = RunGenerator<XamlGenerator>(compilation,
			new AdditionalXamlFile("Suppressed.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		Assert.Empty(ConflictWarnings(result));
	}

	[Fact]
	public void UsesScopedAliasesAndAggregatedGlobalNamespace()
	{
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/maui/global"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision" x:Class="Test.TestPage">
  <StackLayout>
    <Conflict />
    <shared:Conflict />
    <StackLayout xmlns:shared="http://tests/single"><shared:Conflict /></StackLayout>
    <StackLayout xmlns:shared="http://tests/collision"><Label x:DataType="shared:Conflict" /></StackLayout>
  </StackLayout>
</ContentPage>
""";
		const string global = """
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "http://tests/collision")]
""";
		var compilation = CreateCollisionCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(global));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Scoped.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		Assert.Equal(new[] { "Conflict", "shared:Conflict", "shared:Conflict" },
			ConflictWarnings(result).Select(w => xaml.Substring(w.Location.SourceSpan.Start, w.Location.SourceSpan.Length)));
	}

	[Fact]
	public void UsesExactLocationsForEncodedAndElementTextTypeValues()
	{
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision"
             xmlns:local="clr-namespace:Test" x:Class="Test.TestPage">
  <StackLayout>
    <Label BindingContext="{x:Type TypeName=shared:Conf&#108;ict}" />
    <x:Type>shared:Conflict</x:Type>
    <x:Type><![CDATA[shared:Conflict]]></x:Type>
    <x:Static>shared:Owner.Value</x:Static>
    <local:TypeHolder><local:TypeHolder.Type>shared:Conflict</local:TypeHolder.Type></local:TypeHolder>
  </StackLayout>
</ContentPage>
""";
		var result = RunGenerator<XamlGenerator>(CreateCollisionCompilation(),
			new AdditionalXamlFile("Encoded.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		Assert.Equal(new[] { "shared:Conf&#108;ict", "shared:Conflict", "shared:Conflict", "shared:Owner", "shared:Conflict" },
			ConflictWarnings(result).Select(w => xaml.Substring(w.Location.SourceSpan.Start, w.Location.SourceSpan.Length)));
	}

	[Fact]
	public void FindsTypeNamesInTemplateExtensionsBindablePropertiesAndExpressions()
	{
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision"
             xmlns:local="clr-namespace:Test" x:Class="Test.TestPage">
  <StackLayout>
    <Label BindingContext="{DataTemplate shared:Conflict}" />
    <Setter Property="shared:Owner.Value" />
    <Label BindingContext="{= shared:Owner.Value}" />
    <Label Text="{= 'shared:Owner.Value'}" />
    <local:TypeHolder ConvertedType="shared:Conflict" />
    <Label BindingContext="{local:Generic x:TypeArguments=shared:Conflict}" />
    <Label BindingContext="{local:Generic x:TypeArguments={x:Type shared:Conflict}}" />
    <Label BindingContext="{local:TypeValue shared:Owner.Value=example}" />
    <Label BindingContext="{local:TypeValue local:TypeOwner.Value=shared:Conflict}" />
  </StackLayout>
</ContentPage>
""";
		var result = RunGenerator<XamlGenerator>(CreateCollisionCompilation(),
			new AdditionalXamlFile("AdditionalReferences.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		Assert.Equal(new[]
		{
			"shared:Conflict", "shared:Owner", "shared:Owner", "shared:Conflict", "shared:Conflict",
			"shared:Conflict", "shared:Owner", "shared:Conflict"
		}, ConflictWarnings(result).Select(w => xaml.Substring(w.Location.SourceSpan.Start, w.Location.SourceSpan.Length)));
	}

	[Theory]
	[InlineData("Runtime", 0)]
	[InlineData("XamlC", 0)]
	[InlineData("SourceGen", 2)]
	[InlineData("Runtime,SourceGen", 2)]
	public void FindsTypesInSourceGeneratedPropertyElementExpressions(string inflator, int expectedWarnings)
	{
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision" x:Class="Test.TestPage">
  <StackLayout>
    <Label><Label.Text>{= shared:Owner.Value}</Label.Text></Label>
    <Label><Label.Text><![CDATA[{= shared:Owner.Value && true}]]></Label.Text></Label>
    <Label><Label.Text><![CDATA[{= 'shared:Owner.Value & text'}]]></Label.Text></Label>
    <Label><Label.Text>{shared:Choice}</Label.Text></Label>
  </StackLayout>
</ContentPage>
""";
		var result = RunGenerator<XamlGenerator>(CreateCollisionCompilation(),
			new AdditionalXamlFile("PropertyExpressions.xaml", xaml, Inflator: inflator),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		var warnings = ConflictWarnings(result);
		Assert.Equal(expectedWarnings, warnings.Length);
		Assert.All(warnings, warning =>
			Assert.Equal("shared:Owner", xaml.Substring(warning.Location.SourceSpan.Start, warning.Location.SourceSpan.Length)));
	}

	[Fact]
	public void MarksTheValueRatherThanAnIdenticallyNamedMarkupProperty()
	{
		const string xaml = """
<m:ContentPage xmlns="http://tests/collision" xmlns:m="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Test" x:Class="Test.TestPage">
  <m:StackLayout>
    <m:Label BindingContext="{x:Type TypeName=TypeName}" />
    <m:Label BindingContext="{local:TypeValue Value=Value}" />
    <m:Label BindingContext="{local:Generic x:TypeArguments=TypeArguments}" />
  </m:StackLayout>
</m:ContentPage>
""";
		var result = RunGenerator<XamlGenerator>(CreateCollisionCompilation(),
			new AdditionalXamlFile("RepeatedName.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		var warnings = ConflictWarnings(result);
		Assert.Equal(3, warnings.Length);
		Assert.Equal(xaml.IndexOf("TypeName=TypeName", StringComparison.Ordinal) + "TypeName=".Length, warnings[0].Location.SourceSpan.Start);
		Assert.Equal(xaml.IndexOf("Value=Value", StringComparison.Ordinal) + "Value=".Length, warnings[1].Location.SourceSpan.Start);
		Assert.Equal(xaml.IndexOf("TypeArguments=TypeArguments", StringComparison.Ordinal) + "TypeArguments=".Length, warnings[2].Location.SourceSpan.Start);
	}

	[Fact]
	public void MarkupExtensionLookupFallsBackAfterAnAmbiguousSuffix()
	{
		const string code = """
using System;
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://tests/markup", "First")]
[assembly: XmlnsDefinition("http://tests/markup", "Second")]
namespace First
{
	public class Conflict { }
	public class DataTemplate : IMarkupExtension { public object ProvideValue(IServiceProvider provider) => null; }
	public class DataTemplateExtension { }
	public class RepeatedExtension : IMarkupExtension { public object ProvideValue(IServiceProvider provider) => null; }
	public class RepeatedExtensionExtension { }
	[ContentProperty(nameof(Target))]
	public class DoubleExtensionExtension : IMarkupExtension
	{
		public Type Target { get; set; }
		public object ProvideValue(IServiceProvider provider) => null;
	}
}
namespace Second
{
	public class Conflict { }
	public class DataTemplateExtension { }
	public class RepeatedExtensionExtension { }
}
""";
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/markup" x:Class="Test.TestPage">
  <StackLayout>
    <Label Text="{shared:DataTemplate}" />
    <Label Text="{shared:RepeatedExtension}" />
    <Label Text="{shared:DoubleExtension shared:Conflict}" />
  </StackLayout>
</ContentPage>
""";
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
		var result = RunGenerator<XamlGenerator>(compilation,
			new AdditionalXamlFile("MarkupFallback.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		var warning = Assert.Single(ConflictWarnings(result));
		Assert.Equal("shared:Conflict", xaml.Substring(warning.Location.SourceSpan.Start, warning.Location.SourceSpan.Length));
	}

	[Theory]
	[InlineData("Runtime", 9)]
	[InlineData("Runtime,SourceGen", 9)]
	[InlineData("XamlC", 5)]
	[InlineData("SourceGen", 5)]
	public void AnalyzesPlatformBranchesUsedByTheSelectedInflators(string inflator, int expectedWarnings)
	{
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision" xmlns:local="clr-namespace:Test" x:Class="Test.TestPage">
  <OnPlatform x:TypeArguments="x:Object">
    <On Platform="Android"><shared:Conflict /></On>
    <On Platform="iOS"><shared:Conflict /></On>
    <OnPlatform.Default><shared:Conflict /></OnPlatform.Default>
  </OnPlatform>
  <Label Text="{OnPlatform Android={shared:Choice}, iOS={shared:Choice}, Default={shared:Choice}}" />
  <local:TypeHolder Type="{OnPlatform Android=shared:Conflict, iOS=shared:Conflict, Default=shared:Conflict}" />
  <local:TypeHolder Type="{OnIdiom Phone=shared:Conflict, Default=shared:Conflict}" />
</ContentPage>
""";
		var result = RunGenerator<XamlGenerator>(CreateCollisionCompilation(),
			new AdditionalXamlFile("Platforms.xaml", xaml, TargetFramework: "net11.0-android", Inflator: inflator),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		var warnings = ConflictWarnings(result);
		Assert.Equal(expectedWarnings, warnings.Length);
		Assert.Equal(xaml.IndexOf("shared:Conflict", StringComparison.Ordinal), warnings[0].Location.SourceSpan.Start);
		Assert.Equal(xaml.IndexOf("shared:Choice", StringComparison.Ordinal), warnings[expectedWarnings == 5 ? 1 : 3].Location.SourceSpan.Start);
	}

	[Theory]
	[InlineData("Runtime", 2)]
	[InlineData("SourceGen", 1)]
	public void UsesPlatformDefaultWhenTheMatchingBranchHasNoValue(string inflator, int expectedWarnings)
	{
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision" x:Class="Test.TestPage">
  <OnPlatform x:TypeArguments="x:Object">
    <On Platform="Android" />
    <On Platform="iOS"><shared:Conflict /></On>
    <OnPlatform.Default><shared:Conflict /></OnPlatform.Default>
  </OnPlatform>
</ContentPage>
""";
		var result = RunGenerator<XamlGenerator>(CreateCollisionCompilation(),
			new AdditionalXamlFile("PlatformDefault.xaml", xaml, TargetFramework: "net11.0-android", Inflator: inflator),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		var warnings = ConflictWarnings(result);
		Assert.Equal(expectedWarnings, warnings.Length);
		Assert.Equal(xaml.LastIndexOf("shared:Conflict", StringComparison.Ordinal), warnings[expectedWarnings - 1].Location.SourceSpan.Start);
	}

	[Fact]
	public void HonorsAssemblyRestrictionsAndDeduplicatesMappings()
	{
		const string one = """
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://tests/limited", "Shared")]
namespace Shared { public class Thing : Label { } }
""";
		const string two = """
using Microsoft.Maui.Controls;
namespace Shared { public class Thing : Label { } }
""";
		const string mappings = """
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://tests/limited", "Shared", AssemblyName = "Library.One")]
""";
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:limited="http://tests/limited"
             xmlns:direct="clr-namespace:Shared;assembly=Library.One" x:Class="Test.TestPage">
  <StackLayout><limited:Thing /><direct:Thing /></StackLayout>
</ContentPage>
""";
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(mappings)).AddReferences(
			CreateMauiCompilation("Library.One").AddSyntaxTrees(CSharpSyntaxTree.ParseText(one)).ToMetadataReference(),
			CreateMauiCompilation("Library.Two").AddSyntaxTrees(CSharpSyntaxTree.ParseText(two)).ToMetadataReference());
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Restricted.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		Assert.Empty(ConflictWarnings(result));
	}

	[Fact]
	public void HonorsFullAssemblyIdentities()
	{
		const string mappings = """
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://tests/mismatch", "Shared", AssemblyName = "Library.One, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")]
[assembly: XmlnsDefinition("http://tests/mismatch", "Shared", AssemblyName = "Library.Two, Version=0.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://tests/match", "Shared", AssemblyName = "Library.One, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")]
[assembly: XmlnsDefinition("http://tests/match", "Shared", AssemblyName = "Library.Two, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")]
""";
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:mismatch="http://tests/mismatch" xmlns:match="http://tests/match" x:Class="Test.TestPage">
  <StackLayout><mismatch:Thing /><match:Thing /></StackLayout>
</ContentPage>
""";
		const string type = "namespace Shared { public class Thing { } }";
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(mappings)).AddReferences(
			CreateMauiCompilation("Library.One").AddSyntaxTrees(CSharpSyntaxTree.ParseText(type)).ToMetadataReference(),
			CreateMauiCompilation("Library.Two").AddSyntaxTrees(CSharpSyntaxTree.ParseText(type)).ToMetadataReference());
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Identities.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		var warning = Assert.Single(ConflictWarnings(result));
		Assert.Equal("match:Thing", xaml.Substring(warning.Location.SourceSpan.Start, warning.Location.SourceSpan.Length));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void HonorsVisibilityAndFindsConflictsAcrossReferences(bool friend)
	{
		const string one = """
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://tests/cross", "Shared")]
[assembly: XmlnsDefinition("http://tests/cross", "Shared")]
namespace Shared { public class Thing : Label { } public class Hidden : Label { } }
""";
		var two = $$"""
using Microsoft.Maui.Controls;
using System.Runtime.CompilerServices;
[assembly: XmlnsDefinition("http://tests/cross", "Shared")]
{{(friend ? "[assembly: InternalsVisibleTo(\"SourceGeneratorDriver.Generated\")]" : "")}}
namespace Shared { public class Thing : Label { } internal class Hidden : Label { } }
""";
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/cross" x:Class="Test.TestPage">
  <StackLayout><shared:Thing /><shared:Hidden /></StackLayout>
</ContentPage>
""";
		var compilation = CreateMauiCompilation().AddReferences(
			CreateMauiCompilation("Library.One").AddSyntaxTrees(CSharpSyntaxTree.ParseText(one)).ToMetadataReference(),
			CreateMauiCompilation("Library.Two").AddSyntaxTrees(CSharpSyntaxTree.ParseText(two)).ToMetadataReference());
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("External.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		var warnings = ConflictWarnings(result);
		Assert.Equal(friend ? 2 : 1, warnings.Length);
		Assert.Contains("Library.One", warnings[0].GetMessage(), StringComparison.Ordinal);
		Assert.Contains("Library.Two", warnings[0].GetMessage(), StringComparison.Ordinal);
		if (friend)
			Assert.Equal("shared:Hidden", xaml.Substring(warnings[1].Location.SourceSpan.Start, warnings[1].Location.SourceSpan.Length));
	}

	[Fact]
	public void HonorsGenericArityAndExtensionPrecedence()
	{
		const string code = """
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://tests/arity", "First")]
[assembly: XmlnsDefinition("http://tests/arity", "Second")]
namespace First
{
	public class OnlyGeneric<T> { }
	public class Both<T> { }
	public class Widget : Label { }
	public class WidgetExtension { }
	public class Bar : Label { }
	public static class BarExtension { }
	public class Solo { public static string Value => ""; }
	public class SoloExtension { }
}
namespace Second
{
	public class OnlyGeneric { }
	public class Both<T> { }
	public class Widget : Label { }
	public class Bar : Label { }
	public static class BarExtension { }
	public class SoloExtension { }
}
""";
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/arity" x:Class="Test.TestPage">
  <StackLayout>
    <shared:OnlyGeneric x:TypeArguments="x:String" />
    <shared:Both x:TypeArguments="x:String" />
    <shared:Widget />
    <shared:Bar />
    <Label Text="{x:Static shared:Solo.Value}" />
  </StackLayout>
</ContentPage>
""";
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Arity.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		Assert.Equal(new[] { "shared:Both", "shared:Bar" },
			ConflictWarnings(result).Select(w => xaml.Substring(w.Location.SourceSpan.Start, w.Location.SourceSpan.Length)));
	}

	[Theory]
	[InlineData("type", false)]
	[InlineData("type", true)]
	[InlineData("mapping", false)]
	[InlineData("mapping", true)]
	public void UpdatesWarningWhenTypesOrMappingsChange(string change, bool removeConflict)
	{
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/incremental" x:Class="Test.TestPage">
  <shared:Conflict />
</ContentPage>
""";
		string Code(bool conflicting)
		{
			var secondMapping = change == "type" || conflicting
				? "[assembly: XmlnsDefinition(\"http://tests/incremental\", \"Second\")]"
				: "";
			var secondType = change == "mapping" || conflicting ? "public class Conflict : Label { }" : "";
			return $$"""
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://tests/incremental", "First")]
{{secondMapping}}
namespace First { public class Conflict : Label { } }
namespace Second { {{secondType}} }
""";
		}

		var before = CSharpSyntaxTree.ParseText(Code(removeConflict), path: "Types.cs");
		var after = CSharpSyntaxTree.ParseText(Code(!removeConflict), path: "Types.cs");
		var compilation = CreateMauiCompilation().AddSyntaxTrees(before);
		var (first, second) = RunGeneratorWithChanges<XamlGenerator>(compilation,
			(driver, current) => (driver, current.ReplaceSyntaxTree(before, after)),
			new AdditionalXamlFile("Incremental.xaml", xaml, Inflator: "Runtime"));
		Assert.Equal(removeConflict ? 1 : 0, ConflictWarnings(first).Length);
		Assert.Equal(removeConflict ? 0 : 1, ConflictWarnings(second).Length);
	}

	[Theory]
	[InlineData("property", false)]
	[InlineData("property", true)]
	[InlineData("constructor", false)]
	[InlineData("constructor", true)]
	[InlineData("attached", false)]
	[InlineData("attached", true)]
	[InlineData("content", false)]
	[InlineData("content", true)]
	public void UpdatesWarningWhenTypeValueMetadataChanges(string kind, bool removeConflict)
	{
		string Code(bool typeValue)
		{
			var converter = typeValue ? "[System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))]" : "";
			var content = typeValue ? "Target" : "Text";
			return $$"""
using System;
using Microsoft.Maui.Controls;
namespace Test
{
	public class PropertyHolder { {{converter}} public object Target { get; set; } }
	public class ConstructorHolder { public ConstructorHolder([Parameter("Target")] {{converter}} object target) { } }
	public class AttachedHolder
	{
		{{converter}} public static object GetTarget(BindableObject target) => null;
		public static void SetTarget(BindableObject target, object value) { }
	}
	[ContentProperty("{{content}}")]
	public class ContentHolderExtension : IMarkupExtension
	{
		public Type Target { get; set; }
		public string Text { get; set; }
		public object ProvideValue(IServiceProvider provider) => null;
	}
}
""";
		}

		var value = kind switch
		{
			"property" => """<local:PropertyHolder Target="shared:Conflict" />""",
			"constructor" => """<local:ConstructorHolder Target="shared:Conflict" />""",
			"attached" => """<Label local:AttachedHolder.Target="shared:Conflict" />""",
			_ => """<Label Text="{local:ContentHolder shared:Conflict}" />"""
		};
		var xaml = $$"""
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision" xmlns:local="clr-namespace:Test" x:Class="Test.TestPage">
  {{value}}
</ContentPage>
""";
		var before = CSharpSyntaxTree.ParseText(Code(removeConflict), path: "Holder.cs");
		var after = CSharpSyntaxTree.ParseText(Code(!removeConflict), path: "Holder.cs");
		var compilation = CreateMauiCompilation("Microsoft.Maui.Controls.Xaml.UnitTests")
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(CollisionTypes), before);
		var (first, second) = RunGeneratorWithChanges<XamlGenerator>(compilation,
			(driver, current) => (driver, current.ReplaceSyntaxTree(before, after)),
			new AdditionalXamlFile("Metadata.xaml", xaml, Inflator: "Runtime"));
		Assert.Equal(removeConflict ? 1 : 0, ConflictWarnings(first).Length);
		Assert.Equal(removeConflict ? 0 : 1, ConflictWarnings(second).Length);
	}

	[Fact]
	public void UsesOnlyConstructorParametersActuallySelectedByXaml()
	{
		const string code = """
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
namespace Test
{
	public class NeedsOther
	{
		public NeedsOther() { }
		public NeedsOther([Parameter("Target")][TypeConverter(typeof(TypeTypeConverter))] object target,
		                  [Parameter("Other")] string other) { }
		public string Target { get; set; }
	}
	public class StringConstructor
	{
		public StringConstructor([Parameter("Target")] string target) { }
		public Type Target { get; set; }
	}
}
""";
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision" xmlns:local="clr-namespace:Test" x:Class="Test.TestPage">
  <StackLayout>
    <local:NeedsOther Target="shared:Conflict" />
    <local:NeedsOther Target="shared:Conflict" Other="present" />
    <local:StringConstructor Target="shared:Conflict" />
  </StackLayout>
</ContentPage>
""";
		var compilation = CreateMauiCompilation("Microsoft.Maui.Controls.Xaml.UnitTests")
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(CollisionTypes), CSharpSyntaxTree.ParseText(code));
		var result = RunGenerator<XamlGenerator>(compilation,
			new AdditionalXamlFile("Constructors.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		var warning = Assert.Single(ConflictWarnings(result));
		Assert.Equal(xaml.IndexOf("Target=\"shared:Conflict\" Other=", StringComparison.Ordinal) + "Target=\"".Length,
			warning.Location.SourceSpan.Start);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void UpdatesWarningWhenPrivateNestedTypeChanges(bool removeConflict)
	{
		string Code(bool conflicting) => $$"""
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://tests/nested", "First")]
[assembly: XmlnsDefinition("http://tests/nested", "Second")]
namespace First { public class Outer { private class Inner { } } }
namespace Second { public class Outer { {{(conflicting ? "private class Inner { }" : "")}} } }
""";
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/nested" x:Class="Test.TestPage">
  <Label x:DataType="shared:Outer+Inner" />
</ContentPage>
""";
		var before = CSharpSyntaxTree.ParseText(Code(removeConflict), path: "Nested.cs");
		var after = CSharpSyntaxTree.ParseText(Code(!removeConflict), path: "Nested.cs");
		var compilation = CreateMauiCompilation().AddSyntaxTrees(before);
		var (first, second) = RunGeneratorWithChanges<XamlGenerator>(compilation,
			(driver, current) => (driver, current.ReplaceSyntaxTree(before, after)),
			new AdditionalXamlFile("Nested.xaml", xaml, Inflator: "Runtime"));
		Assert.Equal(removeConflict ? 1 : 0, ConflictWarnings(first).Length);
		Assert.Equal(removeConflict ? 0 : 1, ConflictWarnings(second).Length);
		var warning = Assert.Single(ConflictWarnings(removeConflict ? first : second));
		Assert.Equal("shared:Outer+Inner", xaml.Substring(warning.Location.SourceSpan.Start, warning.Location.SourceSpan.Length));
	}

	[Theory]
	[InlineData("{x:Type TypeName=}")]
	[InlineData("{x:Type TypeName='unfinished}")]
	[InlineData("{x:Type TypeName={x:Type}}")]
	public void IncompleteMarkupDoesNotCrashDiagnosticPipeline(string markup)
	{
		var xaml = $$"""
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml" x:Class="Test.TestPage">
  <Label BindingContext="{{markup}}" />
</ContentPage>
""";
		var result = RunGenerator<XamlGenerator>(CreateMauiCompilation(),
			new AdditionalXamlFile("Incomplete.xaml", xaml, Inflator: "Runtime"),
			assertNoCompilationErrors: false, disabledOutputs: IncrementalGeneratorOutputKind.Implementation);
		Assert.DoesNotContain(result.Diagnostics, d => d.Id is "CS8785" or "AD0001");
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void UpdatesWarningWhenCompilationReferenceChanges(bool removeConflict)
	{
		const string app = """
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://tests/incremental", "First")]
namespace First { public class Conflict : Label { } }
""";
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/incremental" x:Class="Test.TestPage">
  <shared:Conflict />
</ContentPage>
""";
		MetadataReference Reference(bool conflicting) => CreateMauiCompilation("Library.Incremental")
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText($$"""
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://tests/incremental", "Second")]
namespace Second { {{(conflicting ? "public class Conflict : Label { }" : "public class Other : Label { }")}} }
""")).ToMetadataReference();
		var before = Reference(removeConflict);
		var after = Reference(!removeConflict);
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(app)).AddReferences(before);
		var (first, second) = RunGeneratorWithChanges<XamlGenerator>(compilation,
			(driver, current) => (driver, current.ReplaceReference(before, after)),
			new AdditionalXamlFile("Incremental.xaml", xaml, Inflator: "Runtime"));
		Assert.Equal(removeConflict ? 1 : 0, ConflictWarnings(first).Length);
		Assert.Equal(removeConflict ? 0 : 1, ConflictWarnings(second).Length);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void UpdatesWarningWhenXamlReferenceChanges(bool removeConflict)
	{
		string Xaml(bool conflicting) => $$"""
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision" x:Class="Test.TestPage">
  {{(conflicting ? "<shared:Conflict />" : "<Label />")}}
</ContentPage>
""";
		var before = new AdditionalXamlFile("Incremental.xaml", Xaml(removeConflict), Inflator: "Runtime");
		var after = new AdditionalXamlFile("Incremental.xaml", Xaml(!removeConflict), Inflator: "Runtime");
		var (first, second) = RunGeneratorWithChanges<XamlGenerator>(CreateCollisionCompilation(),
			(driver, current) => (driver.ReplaceAdditionalText(before.Text, after.Text), current), before);
		Assert.Equal(removeConflict ? 1 : 0, ConflictWarnings(first).Length);
		Assert.Equal(removeConflict ? 0 : 1, ConflictWarnings(second).Length);
	}

	[Fact]
	public void MethodBodyEditKeepsWarningAnalysisCached()
	{
		const string xaml = """
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:shared="http://tests/collision" x:Class="Test.TestPage">
  <shared:Conflict />
</ContentPage>
""";
		var before = CSharpSyntaxTree.ParseText(CollisionTypes + "\nnamespace Test { public class Helper { public int Get() => 1; } }", path: "Types.cs");
		var after = CSharpSyntaxTree.ParseText(CollisionTypes + "\nnamespace Test { public class Helper { public int Get() => 2; } }", path: "Types.cs");
		var compilation = CreateMauiCompilation().AddSyntaxTrees(before);
		var (first, second) = RunGeneratorWithChanges<XamlGenerator>(compilation,
			(driver, current) => (driver, current.ReplaceSyntaxTree(before, after)),
			new AdditionalXamlFile("Incremental.xaml", xaml, Inflator: "Runtime"));
		Assert.Single(ConflictWarnings(first));
		Assert.Single(ConflictWarnings(second));
		var step = second.Results.Single().TrackedSteps["XamlTypeConflictDiagnostics"].Single();
		Assert.Equal(IncrementalStepRunReason.Cached, step.Outputs.Single().Reason);
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
}
