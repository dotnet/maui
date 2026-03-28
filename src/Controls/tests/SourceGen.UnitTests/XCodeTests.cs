using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class XCodeTests : SourceGenXamlInitializeComponentTestBase
{
	string? GetXCodeOutput(GeneratorDriverRunResult result)
		=> result.Results.SingleOrDefault().GeneratedSources
			.SingleOrDefault(gs => gs.HintName.EndsWith(".xcode.cs"))
			.SourceText?.ToString();

	[Fact]
	public void XCode_GeneratesPartialClassWithMethod()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.XCodePage">
    <x:Code><![CDATA[
        string GetGreeting() => "Hello from x:Code!";
    ]]></x:Code>
    <Label Text="Test" />
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class XCodePage : ContentPage
{
    public XCodePage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);
		var xcode = GetXCodeOutput(result);

		Assert.NotNull(xcode);
		Assert.Contains("partial class XCodePage", xcode, StringComparison.Ordinal);
		Assert.Contains("GetGreeting", xcode, StringComparison.Ordinal);
		Assert.Contains("namespace TestApp", xcode, StringComparison.Ordinal);
		// Verify no using directives in generated code (excluding auto-generated comment lines)
		var nonCommentLines = string.Join("\n", xcode!.Split('\n').Where(l => !l.TrimStart().StartsWith("//")));
		Assert.DoesNotContain("using ", nonCommentLines, StringComparison.Ordinal);
	}

	[Fact]
	public void XCode_MultipleBlocks_AreConcatenated()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.MultiBlockPage">
    <x:Code><![CDATA[
        int _count;
    ]]></x:Code>
    <Label Text="Test" />
    <x:Code><![CDATA[
        void Increment() => _count++;
    ]]></x:Code>
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class MultiBlockPage : ContentPage
{
    public MultiBlockPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);
		var xcode = GetXCodeOutput(result);

		Assert.NotNull(xcode);
		Assert.Contains("_count", xcode, StringComparison.Ordinal);
		Assert.Contains("Increment", xcode, StringComparison.Ordinal);
	}

	[Fact]
	public void XCode_WithoutCDATA_Works()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.NoCDataPage">
    <x:Code>
        string Name = "Test";
    </x:Code>
    <Label Text="Test" />
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class NoCDataPage : ContentPage
{
    public NoCDataPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);
		var xcode = GetXCodeOutput(result);

		Assert.NotNull(xcode);
		Assert.Contains("Name", xcode, StringComparison.Ordinal);
	}

	[Fact]
	public void XCode_NoXClass_ReportsDiagnostic()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    <x:Code><![CDATA[
        string Unused = "test";
    ]]></x:Code>
</ResourceDictionary>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
""";

		var (result, _) = RunGenerator(xaml, codeBehind, assertNoCompilationErrors: false);
		Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2016");
	}

	[Fact]
	public void XCode_WithoutPreviewFeatures_ReportsDiagnostic()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.NoPreviewPage">
    <x:Code><![CDATA[
        string Unused = "test";
    ]]></x:Code>
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class NoPreviewPage : ContentPage
{
    public NoPreviewPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind, enablePreviewFeatures: false, assertNoCompilationErrors: false);
		Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2012");
	}

	[Fact]
	public void XCode_NoCodeBlocks_NoOutput()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.NoCodePage">
    <Label Text="No x:Code here" />
</ContentPage>
""";

		var codeBehind =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TestApp;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class NoCodePage : ContentPage
{
    public NoCodePage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);
		var xcode = GetXCodeOutput(result);

		Assert.Null(xcode);
	}
}
