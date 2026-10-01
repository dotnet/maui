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
		Assert.Contains("style.LazyInitialization = (__style, __target) =>", generatedCode, StringComparison.Ordinal);
	}

	[Fact]
	public void ImplicitStyleUsesMetadataNameFactoryKeyAndInlinePositiveGuard()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<Style TargetType="Label">
			<Setter Property="TextColor" Value="Red"/>
		</Style>
	</ContentPage.Resources>
</ContentPage>
""";

		var result = RunGenerator<XamlGenerator>(CreateMauiCompilation(), new AdditionalXamlFile("Test.xaml", xaml));
		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.Empty(errors);

		var generatedCode = GetGeneratedCode(result);
		Assert.Contains("__root.Resources.AddFactory(\"Microsoft.Maui.Controls.Label\", () =>", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("AddFactory(typeof(global::Microsoft.Maui.Controls.Label)", generatedCode, StringComparison.Ordinal);
		Assert.Contains("if (__target is global::Microsoft.Maui.Controls.Label target)", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("InitializeStyle", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("NoInlining", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("KeepAlive", generatedCode, StringComparison.Ordinal);

		var guardIndex = generatedCode.IndexOf("if (__target is global::Microsoft.Maui.Controls.Label target)", StringComparison.Ordinal);
		var setterIndex = generatedCode.IndexOf("var setter = new global::Microsoft.Maui.Controls.Setter", StringComparison.Ordinal);
		var additionIndex = generatedCode.IndexOf("__style.Setters", StringComparison.Ordinal);
		Assert.True(guardIndex < setterIndex);
		Assert.True(setterIndex < additionIndex);
	}

	[Fact]
	public void ImplicitStyleUsesMetadataNameForNestedTargetAndTypeFallbackForGenericTarget()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Test"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<Style TargetType="local:StyleTargets+NestedLabel"/>
		<Style TargetType="{x:Type local:GenericLabel(x:String)}"/>
	</ContentPage.Resources>
</ContentPage>
""";
		var targets =
"""
namespace Test;

public class StyleTargets
{
	public class NestedLabel : global::Microsoft.Maui.Controls.Label
	{
	}
}

public class GenericLabel<T> : global::Microsoft.Maui.Controls.Label
{
}
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(targets));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));
		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.Empty(errors);

		var generatedCode = GetGeneratedCode(result);
		Assert.Contains("__root.Resources.AddFactory(\"Test.StyleTargets+NestedLabel\", () =>", generatedCode, StringComparison.Ordinal);
		Assert.Contains("typeof(global::Test.GenericLabel", generatedCode, StringComparison.Ordinal);
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
		Assert.Contains("style.LazyInitialization = (__style, __target) =>", generatedCode, StringComparison.Ordinal);
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
		Assert.Contains("__root.Resources.AddFactory(\"EmptyStyle\", () =>", generatedCode, StringComparison.Ordinal);
		Assert.Contains("return style;", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("style.LazyInitialization", generatedCode, StringComparison.Ordinal);
	}

	[Fact]
	public void StyleInitializerIsSetBeforeStyleIsAppliedToElement()
	{
		// This test verifies that when a Style with Setters is applied to an element,
		// the Initializer is assigned BEFORE the SetValue(StyleProperty, style) call.
		// This is critical because IStyle.Apply runs the lazy initializer which needs
		// to be set, otherwise Setters won't be populated.
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
		Assert.Contains("style.LazyInitialization = (__style, __target) =>", generatedCode, StringComparison.Ordinal);
		Assert.Contains("label.SetValue(global::Microsoft.Maui.Controls.VisualElement.StyleProperty, style)", generatedCode, StringComparison.Ordinal);
		Assert.Contains("Label.TextColorProperty", generatedCode, StringComparison.Ordinal);

		// CRITICAL: Verify the ORDER - Initializer must be set BEFORE SetValue(StyleProperty)
		var initializerSetIndex = generatedCode.IndexOf("style.LazyInitialization = (__style, __target) =>", StringComparison.Ordinal);
		var setValueIndex = generatedCode.IndexOf("label.SetValue(global::Microsoft.Maui.Controls.VisualElement.StyleProperty, style)", StringComparison.Ordinal);

		Assert.True(initializerSetIndex >= 0, "style.LazyInitialization assignment not found in generated code");
		Assert.True(setValueIndex >= 0, "label.SetValue(StyleProperty) not found in generated code");
		Assert.True(initializerSetIndex < setValueIndex,
			$"style.LazyInitialization must be set BEFORE label.SetValue(StyleProperty, style).\n" +
			$"Initializer set at index {initializerSetIndex}, SetValue at index {setValueIndex}.\n" +
			$"Generated code:\n{generatedCode}");
	}

	[Fact]
	public void ExplicitKeyedStyleKeepsExplicitKeyFactory()
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
		Assert.Contains("__root.Resources.AddFactory(\"TestStyle\", () =>", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("__root.Resources.AddFactory(\"Microsoft.Maui.Controls.Label\", () =>", generatedCode, StringComparison.Ordinal);
		Assert.Contains("if (__target is global::Microsoft.Maui.Controls.Label target)", generatedCode, StringComparison.Ordinal);
	}
}
