using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class StyleSourceGenTests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn);

static string Normalize(string text)
{
	var normalized = text.Replace("\r\n", "\n", System.StringComparison.Ordinal);
	var lines = normalized.Split('\n');
	var nonEmptyLines = lines.Where(line => line.Trim().Length > 0);
	return string.Join("\n", nonEmptyLines).Trim('\n');
}

static string GetGeneratedCode(GeneratorDriverRunResult result)
{
	var tree = result.GeneratedTrees
		.FirstOrDefault(t => t.FilePath.EndsWith(".xsg.cs", System.StringComparison.Ordinal));
	Assert.NotNull(tree);
	return Normalize(tree.GetText().ToString());
}

static void AssertSnapshot(string expected, string actual)
{
	if (!string.Equals(expected, actual, System.StringComparison.Ordinal))
	{
		var index = 0;
		for (; index < expected.Length && index < actual.Length; index++)
		{
			if (expected[index] != actual[index])
				break;
		}
		System.Console.WriteLine($"Snapshot diff at {index}");
		System.Console.WriteLine($"Expected snippet: {EscapeSnippet(expected, index)}");
		System.Console.WriteLine($"Actual snippet: {EscapeSnippet(actual, index)}");
		System.Console.WriteLine($"Expected length: {expected.Length}");
		System.Console.WriteLine($"Actual length: {actual.Length}");
	}
	Assert.Equal(expected, actual);
}

static string EscapeSnippet(string text, int index)
{
	var length = System.Math.Min(120, text.Length - index);
	var snippet = text.Substring(index, length);
	return snippet.Replace("\n", "\\n", System.StringComparison.Ordinal);
}

	[Fact]
	public void SimpleStyleWithSetter()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<Style x:Key="TestStyle" TargetType="Label">
			<Setter Property="TextColor" Value="Red"/>
		</Style>
	</ContentPage.Resources>
</ContentPage>
""";

		var compilation = CreateMauiCompilation();
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		// Check for errors after output
		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.Empty(errors);

		// Find the actual generated code (the .xsg.cs file)
		var generatedCode = GetGeneratedCode(result);
		Assert.Contains("new global::Microsoft.Maui.Controls.Style(\"Microsoft.Maui.Controls.Label, Microsoft.Maui.Controls\")", generatedCode, StringComparison.Ordinal);
		Assert.Contains("Label.TextColorProperty", generatedCode, StringComparison.Ordinal);
		Assert.Contains("style.Initializer = styleInitializer", generatedCode, StringComparison.Ordinal);
		
		// Verify lazy behavior: Initializer is set but NOT called immediately
		// The old eager pattern had: styleInitializer(style, new Label()); style.Initializer = null!;
		// The new lazy pattern just sets the Initializer and lets it run when style is applied
		Assert.DoesNotContain("styleInitializer(style, new global::Microsoft.Maui.Controls.Label())", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("style.Initializer = null!", generatedCode, StringComparison.Ordinal);
	}

	[Fact]
	public void StyleWithMultipleSetters()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<Style x:Key="TestStyle" TargetType="Label">
			<Setter Property="TextColor" Value="Red"/>
			<Setter Property="FontSize" Value="24"/>
			<Setter Property="FontAttributes" Value="Bold"/>
		</Style>
	</ContentPage.Resources>
</ContentPage>
""";

		var compilation = CreateMauiCompilation();
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.Empty(errors);

		var generatedCode = GetGeneratedCode(result);
		Assert.Contains("new global::Microsoft.Maui.Controls.Style(\"Microsoft.Maui.Controls.Label, Microsoft.Maui.Controls\")", generatedCode, StringComparison.Ordinal);
		Assert.Contains("Label.TextColorProperty", generatedCode, StringComparison.Ordinal);
		Assert.Contains("Label.FontSizeProperty", generatedCode, StringComparison.Ordinal);
		Assert.Contains("Label.FontAttributesProperty", generatedCode, StringComparison.Ordinal);
		Assert.Contains("style.Initializer = styleInitializer", generatedCode, StringComparison.Ordinal);
		
		// Verify lazy behavior: Initializer is set but NOT called immediately
		Assert.DoesNotContain("styleInitializer(style, new global::Microsoft.Maui.Controls.Label())", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("style.Initializer = null!", generatedCode, StringComparison.Ordinal);
	}

	[Fact]
	public void StyleWithoutSetters()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<Style x:Key="EmptyStyle" TargetType="Label"/>
	</ContentPage.Resources>
</ContentPage>
""";

		var compilation = CreateMauiCompilation();
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.Empty(errors);

		// Find the actual generated code (the .xsg.cs file)
		var generatedCode = GetGeneratedCode(result);
		var expected = Normalize("""
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a .NET MAUI source generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
#pragma warning disable CS0219 // Variable is assigned but its value is never used
namespace Test;
[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Maui.Controls.SourceGen, Version=10.0.0.0, Culture=neutral, PublicKeyToken=null", "10.0.0.0")]
public partial class TestPage
{
	private partial void InitializeComponent()
	{
		// Fallback to Runtime inflation if the page was updated by HotReload
		static string? getPathForType(global::System.Type type)
		{
			var assembly = type.Assembly;
			foreach (var xria in global::System.Reflection.CustomAttributeExtensions.GetCustomAttributes<global::Microsoft.Maui.Controls.Xaml.XamlResourceIdAttribute>(assembly))
			{
				if (xria.Type == type)
					return xria.Path;
			}
			return null;
		}
		var rlr = global::Microsoft.Maui.Controls.Internals.ResourceLoader.ResourceProvider2?.Invoke(new global::Microsoft.Maui.Controls.Internals.ResourceLoader.ResourceLoadingQuery
		{
			AssemblyName = typeof(global::Test.TestPage).Assembly.GetName(),
			ResourcePath = getPathForType(typeof(global::Test.TestPage)),
			Instance = this,
		});
		if (rlr?.ResourceContent != null)
		{
			this.InitializeComponentRuntime();
			return;
		}
		var style = new global::Microsoft.Maui.Controls.Style("Microsoft.Maui.Controls.Label, Microsoft.Maui.Controls");
		global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo(style!, new global::System.Uri(@"Test.xaml;assembly=SourceGeneratorDriver.Generated", global::System.UriKind.Relative), 7, 4);
		var __root = this;
		global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo(__root!, new global::System.Uri(@"Test.xaml;assembly=SourceGeneratorDriver.Generated", global::System.UriKind.Relative), 2, 2);
#if !_MAUIXAML_SG_NAMESCOPE_DISABLE
		global::Microsoft.Maui.Controls.Internals.INameScope iNameScope = global::Microsoft.Maui.Controls.Internals.NameScope.GetNameScope(__root) ?? new global::Microsoft.Maui.Controls.Internals.NameScope();
#endif
#if !_MAUIXAML_SG_NAMESCOPE_DISABLE
		global::Microsoft.Maui.Controls.Internals.NameScope.SetNameScope(__root, iNameScope);
#endif
		__root.Resources["EmptyStyle"] = style;
	}
}
""");
		AssertSnapshot(expected, generatedCode);
	}

	[Fact]
	public void StyleInitializerIsSetBeforeStyleIsAppliedToElement()
	{
		// This test verifies that when a Style with Setters is applied to an element,
		// the Initializer is assigned BEFORE the SetValue(StyleProperty, style) call.
		// This is critical because IStyle.Apply calls EnsureInitialized which needs
		// the _initializer to be set, otherwise Setters won't be populated.
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label x:Name="label">
		<Label.Style>
			<Style TargetType="Label">
				<Setter Property="TextColor" Value="Red"/>
			</Style>
		</Label.Style>
	</Label>
</ContentPage>
""";

		var compilation = CreateMauiCompilation();
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.Empty(errors);

		var generatedCode = GetGeneratedCode(result);
		
		// Verify key elements exist
		Assert.Contains("new global::Microsoft.Maui.Controls.Style(\"Microsoft.Maui.Controls.Label, Microsoft.Maui.Controls\")", generatedCode, StringComparison.Ordinal);
		Assert.Contains("style.Initializer = styleInitializer", generatedCode, StringComparison.Ordinal);
		Assert.Contains("label.SetValue(global::Microsoft.Maui.Controls.VisualElement.StyleProperty, style)", generatedCode, StringComparison.Ordinal);
		Assert.Contains("Label.TextColorProperty", generatedCode, StringComparison.Ordinal);
		
		// CRITICAL: Verify the ORDER - Initializer must be set BEFORE SetValue(StyleProperty)
		var initializerSetIndex = generatedCode.IndexOf("style.Initializer = styleInitializer", StringComparison.Ordinal);
		var setValueIndex = generatedCode.IndexOf("label.SetValue(global::Microsoft.Maui.Controls.VisualElement.StyleProperty, style)", StringComparison.Ordinal);
		
		Assert.True(initializerSetIndex >= 0, "style.Initializer assignment not found in generated code");
		Assert.True(setValueIndex >= 0, "label.SetValue(StyleProperty) not found in generated code");
		Assert.True(initializerSetIndex < setValueIndex,
			$"style.Initializer must be set BEFORE label.SetValue(StyleProperty, style).\n" +
			$"Initializer set at index {initializerSetIndex}, SetValue at index {setValueIndex}.\n" +
			$"Generated code:\n{generatedCode}");
		
		// Verify lazy behavior: Initializer is NOT called immediately
		Assert.DoesNotContain("styleInitializer(style, new global::Microsoft.Maui.Controls.Label())", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("style.Initializer = null!", generatedCode, StringComparison.Ordinal);
	}

	[Fact]
	public void SimpleStyleWithSetterFullSnapshot()
	{
		// Full snapshot test to verify the complete lazy style pattern
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<Style x:Key="TestStyle" TargetType="Label">
			<Setter Property="TextColor" Value="Red"/>
		</Style>
	</ContentPage.Resources>
</ContentPage>
""";

		var compilation = CreateMauiCompilation();
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.Empty(errors);

		var generatedCode = GetGeneratedCode(result);
		var expected = Normalize("""
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a .NET MAUI source generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
#pragma warning disable CS0219 // Variable is assigned but its value is never used
namespace Test;
[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Maui.Controls.SourceGen, Version=10.0.0.0, Culture=neutral, PublicKeyToken=null", "10.0.0.0")]
public partial class TestPage
{
	private partial void InitializeComponent()
	{
		// Fallback to Runtime inflation if the page was updated by HotReload
		static string? getPathForType(global::System.Type type)
		{
			var assembly = type.Assembly;
			foreach (var xria in global::System.Reflection.CustomAttributeExtensions.GetCustomAttributes<global::Microsoft.Maui.Controls.Xaml.XamlResourceIdAttribute>(assembly))
			{
				if (xria.Type == type)
					return xria.Path;
			}
			return null;
		}
		var rlr = global::Microsoft.Maui.Controls.Internals.ResourceLoader.ResourceProvider2?.Invoke(new global::Microsoft.Maui.Controls.Internals.ResourceLoader.ResourceLoadingQuery
		{
			AssemblyName = typeof(global::Test.TestPage).Assembly.GetName(),
			ResourcePath = getPathForType(typeof(global::Test.TestPage)),
			Instance = this,
		});
		if (rlr?.ResourceContent != null)
		{
			this.InitializeComponentRuntime();
			return;
		}
		var style = new global::Microsoft.Maui.Controls.Style("Microsoft.Maui.Controls.Label, Microsoft.Maui.Controls");
		global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo(style!, new global::System.Uri(@"Test.xaml;assembly=SourceGeneratorDriver.Generated", global::System.UriKind.Relative), 7, 4);
		var __root = this;
		global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo(__root!, new global::System.Uri(@"Test.xaml;assembly=SourceGeneratorDriver.Generated", global::System.UriKind.Relative), 2, 2);
#if !_MAUIXAML_SG_NAMESCOPE_DISABLE
		global::Microsoft.Maui.Controls.Internals.INameScope iNameScope = global::Microsoft.Maui.Controls.Internals.NameScope.GetNameScope(__root) ?? new global::Microsoft.Maui.Controls.Internals.NameScope();
#endif
#if !_MAUIXAML_SG_NAMESCOPE_DISABLE
		global::Microsoft.Maui.Controls.Internals.NameScope.SetNameScope(__root, iNameScope);
#endif
		global::System.Action<global::Microsoft.Maui.Controls.Style, global::Microsoft.Maui.Controls.BindableObject> styleInitializer = (__style, __target) =>
		{
#if !_MAUIXAML_SG_NAMESCOPE_DISABLE
			global::Microsoft.Maui.Controls.Internals.INameScope iNameScope1 = new global::Microsoft.Maui.Controls.Internals.NameScope();
#endif
#if !_MAUIXAML_SG_NAMESCOPE_DISABLE
			global::Microsoft.Maui.Controls.Internals.INameScope iNameScope2 = new global::Microsoft.Maui.Controls.Internals.NameScope();
#endif
			var setter = new global::Microsoft.Maui.Controls.Setter {Property = global::Microsoft.Maui.Controls.Label.TextColorProperty, Value = global::Microsoft.Maui.Graphics.Colors.Red};
			if (global::Microsoft.Maui.VisualDiagnostics.GetSourceInfo(setter!) == null)
				global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo(setter!, new global::System.Uri(@"Test.xaml;assembly=SourceGeneratorDriver.Generated", global::System.UriKind.Relative), 8, 5);
#line 8 "Test.xaml"
			((global::System.Collections.Generic.ICollection<global::Microsoft.Maui.Controls.Setter>)__style.Setters).Add((global::Microsoft.Maui.Controls.Setter)setter);
#line default
		};
		style.Initializer = styleInitializer;
		__root.Resources["TestStyle"] = style;
	}
}
""");
		AssertSnapshot(expected, generatedCode);
	}
}
