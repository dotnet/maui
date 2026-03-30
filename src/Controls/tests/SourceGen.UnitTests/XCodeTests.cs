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

	[Fact]
	public void XCode_UsingDirectives_ArePromotedToFileTop()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.UsingsPage">
    <x:Code><![CDATA[
        using System.Net.Http;
        using System.Threading.Tasks;

        async Task<string> FetchAsync()
        {
            using var client = new HttpClient();
            return await client.GetStringAsync("https://example.com");
        }
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
public partial class UsingsPage : ContentPage
{
    public UsingsPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);
		var xcode = GetXCodeOutput(result);

		Assert.NotNull(xcode);

		// using directives should appear before the namespace declaration
		var namespaceIndex = xcode!.IndexOf("namespace TestApp", StringComparison.Ordinal);
		var usingHttpIndex = xcode.IndexOf("using System.Net.Http;", StringComparison.Ordinal);
		var usingTasksIndex = xcode.IndexOf("using System.Threading.Tasks;", StringComparison.Ordinal);
		Assert.True(usingHttpIndex >= 0, "using System.Net.Http should be in output");
		Assert.True(usingTasksIndex >= 0, "using System.Threading.Tasks should be in output");
		Assert.True(usingHttpIndex < namespaceIndex, "using directive should appear before namespace");
		Assert.True(usingTasksIndex < namespaceIndex, "using directive should appear before namespace");

		// "using var client" is a using statement, not a directive — it stays inside the class
		var classIndex = xcode.IndexOf("partial class UsingsPage", StringComparison.Ordinal);
		var usingVarIndex = xcode.IndexOf("using var client", StringComparison.Ordinal);
		Assert.True(usingVarIndex > classIndex, "using statement should remain inside the class body");

		// Method body should still be present
		Assert.Contains("FetchAsync", xcode, StringComparison.Ordinal);
	}

	[Fact]
	public void XCode_DuplicateUsings_AreDeduped()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.DedupePage">
    <x:Code><![CDATA[
        using System.Net.Http;
    ]]></x:Code>
    <x:Code><![CDATA[
        using System.Net.Http;
        string Name = "test";
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
public partial class DedupePage : ContentPage
{
    public DedupePage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);
		var xcode = GetXCodeOutput(result);

		Assert.NotNull(xcode);
		// Should appear exactly once
		var count = xcode!.Split("using System.Net.Http;").Length - 1;
		Assert.Equal(1, count);
		Assert.Contains("Name", xcode, StringComparison.Ordinal);
	}

	[Fact]
	public void XCode_UsingStatic_IsPromoted()
	{
		var xaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TestApp.StaticUsingPage">
    <x:Code><![CDATA[
        using static System.Math;

        double Compute() => Abs(-42);
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
public partial class StaticUsingPage : ContentPage
{
    public StaticUsingPage() => InitializeComponent();
}
""";

		var (result, _) = RunGenerator(xaml, codeBehind);
		var xcode = GetXCodeOutput(result);

		Assert.NotNull(xcode);
		var namespaceIndex = xcode!.IndexOf("namespace TestApp", StringComparison.Ordinal);
		var usingStaticIndex = xcode.IndexOf("using static System.Math;", StringComparison.Ordinal);
		Assert.True(usingStaticIndex >= 0, "using static should be in output");
		Assert.True(usingStaticIndex < namespaceIndex, "using static should appear before namespace");
		Assert.Contains("Compute", xcode, StringComparison.Ordinal);
	}
}
