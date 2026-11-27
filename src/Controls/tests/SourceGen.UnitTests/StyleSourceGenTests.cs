using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class StyleSourceGenTests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null, string? TrimmableStyles = "true")
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn, TrimmableStyles: TrimmableStyles);

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
		Assert.Contains("new global::Microsoft.Maui.Controls.Style(typeof(global::Microsoft.Maui.Controls.Label))", generatedCode, StringComparison.Ordinal);
		Assert.Contains("Label.TextColorProperty", generatedCode, StringComparison.Ordinal);
		Assert.Contains("style.LazyInitialization = new global::System.Func<global::Microsoft.Maui.Controls.Style, global::Microsoft.Maui.Controls.BindableObject, bool>(lazyInitialization", generatedCode, StringComparison.Ordinal);
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
		Assert.Contains("return true;", generatedCode, StringComparison.Ordinal);
		Assert.Contains("return false;", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("InitializeStyle", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("NoInlining", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("KeepAlive", generatedCode, StringComparison.Ordinal);
		// A cached lambda would add a delegate cache field, which breaks later Hot Reload updates.
		Assert.Contains("bool lazyInitialization(global::Microsoft.Maui.Controls.Style __style, global::Microsoft.Maui.Controls.BindableObject __target)", generatedCode, StringComparison.Ordinal);
		Assert.Contains("style.LazyInitialization = new global::System.Func<global::Microsoft.Maui.Controls.Style, global::Microsoft.Maui.Controls.BindableObject, bool>(lazyInitialization);", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("(__style, __target) =>", generatedCode, StringComparison.Ordinal);

		var guardIndex = generatedCode.IndexOf("if (__target is global::Microsoft.Maui.Controls.Label target)", StringComparison.Ordinal);
		var setterIndex = generatedCode.IndexOf("var setter = new global::Microsoft.Maui.Controls.Setter", StringComparison.Ordinal);
		var additionIndex = generatedCode.IndexOf("__style.Setters", StringComparison.Ordinal);
		Assert.True(guardIndex < setterIndex);
		Assert.True(setterIndex < additionIndex);
	}

	[Fact]
	public void GenericImplicitStyleKeepsGuardedLazyInitializer()
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
		<Style TargetType="{x:Type local:GenericLabel(x:String)}">
			<Setter Property="TextColor" Value="Red"/>
		</Style>
	</ContentPage.Resources>
</ContentPage>
""";
		var target =
"""
namespace Test;

public class GenericLabel<T> : global::Microsoft.Maui.Controls.Label
{
}
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(target));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));
		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.Empty(errors);

		var generatedCode = GetGeneratedCode(result);
		Assert.Contains("new global::Microsoft.Maui.Controls.Style(typeof(global::Test.GenericLabel<string>))", generatedCode, StringComparison.Ordinal);
		Assert.Contains("AddFactory(typeof(global::Test.GenericLabel<string>)", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("Test.GenericLabel`1[[System.String", generatedCode, StringComparison.Ordinal);
		Assert.Contains("style.LazyInitialization = new global::System.Func<global::Microsoft.Maui.Controls.Style, global::Microsoft.Maui.Controls.BindableObject, bool>(lazyInitialization", generatedCode, StringComparison.Ordinal);

		var guardIndex = generatedCode.IndexOf("if (__target is global::Test.GenericLabel<string> target)", StringComparison.Ordinal);
		var setterIndex = generatedCode.IndexOf("Label.TextColorProperty", StringComparison.Ordinal);
		Assert.True(guardIndex >= 0, "Generic style setters must stay behind the positive target guard");
		Assert.True(guardIndex < setterIndex);
	}

	[Fact]
	public void NestedImplicitStyleUsesMetadataNameFactoryKey()
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
	</ContentPage.Resources>
</ContentPage>
""";
		var target =
"""
namespace Test;

public class StyleTargets
{
	public class NestedLabel : global::Microsoft.Maui.Controls.Label
	{
	}
}
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(target));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);

		var generatedCode = GetGeneratedCode(result);
		Assert.Contains("__root.Resources.AddFactory(\"Test.StyleTargets+NestedLabel\", () =>", generatedCode, StringComparison.Ordinal);
	}

	[Fact]
	public void NestedLazyStyleDoesNotProcessItsContentTwice()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<Style x:Key="Outer" TargetType="ContentView">
			<Setter Property="Content">
				<Label>
					<Label.Style>
						<Style TargetType="Label">
							<Setter Property="TextColor" Value="Red"/>
						</Style>
					</Label.Style>
				</Label>
			</Setter>
		</Style>
	</ContentPage.Resources>
</ContentPage>
""";

		var result = RunGenerator<XamlGenerator>(CreateMauiCompilation(), new AdditionalXamlFile("Test.xaml", xaml));
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);

		var generatedCode = GetGeneratedCode(result);
		Assert.Equal(1, CountOccurrences(generatedCode, "Label.TextColorProperty"));
		Assert.Equal(2, CountOccurrences(generatedCode, "LazyInitialization = new global::System.Func<"));
	}

	[Fact]
	public void StyleSetterMarkupIsOnlyCreatedInsideLazyInitializer()
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
		<Color x:Key="Accent">Red</Color>
		<Style TargetType="Button">
			<Setter Property="BackgroundColor" Value="{DynamicResource Accent}"/>
			<Setter Property="Text" Value="{x:Static local:StyleConstants.Greeting}"/>
		</Style>
	</ContentPage.Resources>
</ContentPage>
""";
		var constants =
"""
namespace Test;

public static class StyleConstants
{
	public static readonly string Greeting = "Hello";
}
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(constants));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);

		var generatedCode = GetGeneratedCode(result);
		var guardIndex = generatedCode.IndexOf("if (__target is global::Microsoft.Maui.Controls.Button target)", StringComparison.Ordinal);
		Assert.True(guardIndex >= 0, "Style content must be generated inside the positive target guard");
		Assert.True(
			generatedCode.IndexOf("new global::Microsoft.Maui.Controls.Setter", StringComparison.Ordinal) > guardIndex,
			"Setters must only be created inside the lazy initializer");
		Assert.Equal(1, CountOccurrences(generatedCode, "global::Test.StyleConstants.Greeting"));
	}

	[Theory]
	[InlineData("true")]
	[InlineData("false")]
	public void NamedElementInStyleContentDoesNotGetAField(string trimmableStyles)
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentView>
		<ContentView.Style>
			<Style TargetType="ContentView">
				<Setter Property="Content">
					<Label x:Name="StyledLabel" />
				</Setter>
			</Style>
		</ContentView.Style>
	</ContentView>
</ContentPage>
""";

		// Code-behind declares no field for names inside a Style, so none may be assigned.
		var result = RunGenerator<XamlGenerator>(CreateMauiCompilation(), new AdditionalXamlFile("Test.xaml", xaml, TrimmableStyles: trimmableStyles));
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);

		var generatedCode = GetGeneratedCode(result);
		Assert.Contains("RegisterName(\"StyledLabel\"", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("this.StyledLabel", generatedCode, StringComparison.Ordinal);
	}

	[Fact]
	public void InlineBasedOnStyleGetsItsOwnLazyInitializer()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<Style x:Key="Derived" TargetType="Label">
			<Style.BasedOn>
				<Style TargetType="Label">
					<Setter Property="FontSize" Value="20"/>
				</Style>
			</Style.BasedOn>
			<Setter Property="TextColor" Value="Red"/>
		</Style>
	</ContentPage.Resources>
</ContentPage>
""";

		var result = RunGenerator<XamlGenerator>(CreateMauiCompilation(), new AdditionalXamlFile("Test.xaml", xaml));
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);

		var generatedCode = GetGeneratedCode(result);
		Assert.DoesNotContain("StyleContent", generatedCode, StringComparison.Ordinal);
		Assert.Contains(".BasedOn = ", generatedCode, StringComparison.Ordinal);
		Assert.Equal(2, CountOccurrences(generatedCode, "LazyInitialization = new global::System.Func<"));
		Assert.Equal(1, CountOccurrences(generatedCode, "Label.FontSizeProperty"));
		Assert.True(
			generatedCode.IndexOf("Label.FontSizeProperty", StringComparison.Ordinal)
				> generatedCode.IndexOf("if (__target is global::Microsoft.Maui.Controls.Label target)", StringComparison.Ordinal),
			"The base style setters must be created inside a lazy initializer");
	}

	[Fact]
	public void StyleWithStaticTypeValuedTargetTypeUsesTypeConstructor()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Test"
	x:Class="Test.TestPage">
	<Label>
		<Label.Style>
			<Style TargetType="{x:Static local:StyleTypes.LabelType}">
				<Setter Property="Label.TextColor" Value="Red"/>
			</Style>
		</Label.Style>
	</Label>
</ContentPage>
""";
		var types =
"""
namespace Test;

public static class StyleTypes
{
	public static readonly global::System.Type LabelType = typeof(global::Microsoft.Maui.Controls.Label);
}
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(types));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);

		var generatedCode = GetGeneratedCode(result);
		Assert.Contains("global::Test.StyleTypes.LabelType", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("typeof(object)", generatedCode, StringComparison.Ordinal);
		Assert.Contains("Label.TextColorProperty", generatedCode, StringComparison.Ordinal);
	}

	[Fact]
	public void StyleWithoutTargetTypeReportsDiagnostic()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<Style x:Key="MissingTarget">
			<Setter Property="TextColor" Value="Red"/>
		</Style>
	</ContentPage.Resources>
</ContentPage>
""";

		var result = RunGenerator<XamlGenerator>(CreateMauiCompilation(), new AdditionalXamlFile("Test.xaml", xaml));
		Assert.Contains(result.Diagnostics, d => d.Id == "MAUIG1001" && d.GetMessage().Contains("Style TargetType could not be resolved", StringComparison.Ordinal));
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIG1001" && d.GetMessage().Contains("The given key", StringComparison.Ordinal));
		Assert.DoesNotContain(GetGeneratedCode(result), "return null!;", StringComparison.Ordinal);
	}

	[Fact]
	public void InlineStyleWithoutTargetTypeDoesNotCrashGenerator()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label>
		<Label.Style>
			<Style>
				<Setter Property="TextColor" Value="Red"/>
			</Style>
		</Label.Style>
	</Label>
</ContentPage>
""";

		var result = RunGenerator<XamlGenerator>(CreateMauiCompilation(), new AdditionalXamlFile("Test.xaml", xaml));
		Assert.Contains(result.Diagnostics, d => d.Id == "MAUIG1001" && d.GetMessage().Contains("Style TargetType could not be resolved", StringComparison.Ordinal));
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
		Assert.Contains("new global::Microsoft.Maui.Controls.Style(typeof(global::Microsoft.Maui.Controls.Label))", generatedCode, StringComparison.Ordinal);
		Assert.Contains("Label.TextColorProperty", generatedCode, StringComparison.Ordinal);
		Assert.Contains("Label.FontSizeProperty", generatedCode, StringComparison.Ordinal);
		Assert.Contains("Label.FontAttributesProperty", generatedCode, StringComparison.Ordinal);
		Assert.Contains("style.LazyInitialization = new global::System.Func<global::Microsoft.Maui.Controls.Style, global::Microsoft.Maui.Controls.BindableObject, bool>(lazyInitialization", generatedCode, StringComparison.Ordinal);
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
		Assert.Contains("new global::Microsoft.Maui.Controls.Style(typeof(global::Microsoft.Maui.Controls.Label))", generatedCode, StringComparison.Ordinal);
		Assert.Contains("style.LazyInitialization = new global::System.Func<global::Microsoft.Maui.Controls.Style, global::Microsoft.Maui.Controls.BindableObject, bool>(lazyInitialization", generatedCode, StringComparison.Ordinal);
		Assert.Contains("label.SetValue(global::Microsoft.Maui.Controls.VisualElement.StyleProperty, style)", generatedCode, StringComparison.Ordinal);
		Assert.Contains("Label.TextColorProperty", generatedCode, StringComparison.Ordinal);

		// CRITICAL: Verify the ORDER - Initializer must be set BEFORE SetValue(StyleProperty)
		var initializerSetIndex = generatedCode.IndexOf("style.LazyInitialization = new global::System.Func<global::Microsoft.Maui.Controls.Style, global::Microsoft.Maui.Controls.BindableObject, bool>(lazyInitialization", StringComparison.Ordinal);
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

	[Fact]
	public void StaticResourceStyleWithIncompatibleTargetTypeReportsWarning()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<Style x:Key="EntryStyle" TargetType="Entry">
			<Setter Property="TextColor" Value="Red"/>
		</Style>
	</ContentPage.Resources>
	<VerticalStackLayout>
		<Editor Style="{StaticResource EntryStyle}"/>
		<Button>
			<Button.Style>
				<StaticResource Key="EntryStyle"/>
			</Button.Style>
		</Button>
	</VerticalStackLayout>
</ContentPage>
""";

		var result = RunGenerator<XamlGenerator>(CreateMauiCompilation(), new AdditionalXamlFile("Test.xaml", xaml));

		var warnings = result.Diagnostics.Where(d => d.Id == "MAUIX2024").ToList();
		Assert.Equal(2, warnings.Count);
		Assert.All(warnings, warning => Assert.Equal(DiagnosticSeverity.Warning, warning.Severity));
		Assert.Contains(warnings, w => w.GetMessage().Contains("'EntryStyle'", StringComparison.Ordinal)
			&& w.GetMessage().Contains("Microsoft.Maui.Controls.Entry", StringComparison.Ordinal)
			&& w.GetMessage().Contains("Microsoft.Maui.Controls.Editor", StringComparison.Ordinal)
			&& w.Location.GetLineSpan().StartLinePosition.Line + 1 == 12);
		Assert.Contains(warnings, w => w.GetMessage().Contains("Microsoft.Maui.Controls.Button", StringComparison.Ordinal));
	}

	[Fact]
	public void StaticResourceStyleWithCompatibleOrUnknownTargetTypeDoesNotReportWarning()
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
		<Style x:Key="LabelStyle" TargetType="Label">
			<Setter Property="TextColor" Value="Red"/>
		</Style>
		<Style x:Key="ViewStyle" TargetType="{x:Type View}">
			<Setter Property="Margin" Value="4"/>
		</Style>
		<Style x:Key="RuntimeTypeStyle" TargetType="{x:Static local:StyleTypes.EntryType}">
			<Setter Property="Entry.TextColor" Value="Red"/>
		</Style>
	</ContentPage.Resources>
	<VerticalStackLayout>
		<Label Style="{StaticResource LabelStyle}"/>
		<Label Style="{StaticResource ViewStyle}"/>
		<Label Style="{StaticResource RuntimeTypeStyle}"/>
		<Label Style="{StaticResource StyleFromApplicationResources}"/>
	</VerticalStackLayout>
</ContentPage>
""";
		var types =
"""
namespace Test;

public static class StyleTypes
{
	public static readonly global::System.Type EntryType = typeof(global::Microsoft.Maui.Controls.Entry);
}
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(types));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2024");
	}

	// MSBuild sets MauiXamlTrimmableStyles from PublishAot; without it, styles are eager.
	[Theory]
	[InlineData("false")]
	[InlineData(null)]
	public void StylesAreEagerWhenTrimmableStylesAreNotEnabled(string? trimmableStyles)
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
		<Style x:Key="EntryStyle" TargetType="Entry">
			<Setter Property="Placeholder" Value="Name"/>
		</Style>
	</ContentPage.Resources>
	<Editor Style="{StaticResource EntryStyle}"/>
</ContentPage>
""";

		var result = RunGenerator<XamlGenerator>(CreateMauiCompilation(), new AdditionalXamlFile("Test.xaml", xaml, TrimmableStyles: trimmableStyles));
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		// Eagerly created styles still apply to mismatched elements, so the build warning would be wrong.
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2024");

		var generatedCode = GetGeneratedCode(result);
		Assert.Contains("new global::Microsoft.Maui.Controls.Style(typeof(global::Microsoft.Maui.Controls.Label))", generatedCode, StringComparison.Ordinal);
		Assert.Contains("new global::Microsoft.Maui.Controls.Style(typeof(global::Microsoft.Maui.Controls.Entry))", generatedCode, StringComparison.Ordinal);
		Assert.Contains("Label.TextColorProperty", generatedCode, StringComparison.Ordinal);
		Assert.DoesNotContain("LazyInitialization", generatedCode, StringComparison.Ordinal);
	}

	static int CountOccurrences(string text, string value)
	{
		var count = 0;
		var index = 0;
		while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
		{
			count++;
			index += value.Length;
		}
		return count;
	}
}
