using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class LineInfoTests : SourceGenXamlInitializeComponentTestBase
{
	[Theory]
	[InlineData("Source/Test.xaml")]
	[InlineData("Linked/Test.xaml")]
	public void EventHandlerWarningMapsToPhysicalXamlFile(string targetPath)
	{
		var xaml =
"""
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Test.TestPage">
    <Button Clicked="OnCounterClicked" />
</ContentPage>
""";
		var code =
"""
using System;
using Microsoft.Maui.Controls;

namespace Test;

public partial class TestPage : ContentPage
{
    public TestPage() => InitializeComponent();
    void OnCounterClicked(object sender, EventArgs e) { }
}
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation()
			.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable))
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
		var physicalPath = Path.Combine(Environment.CurrentDirectory, "Source", "Test.xaml");
		var xamlFile = new AdditionalXamlFile(physicalPath, xaml, RelativePath: targetPath, TargetPath: targetPath);
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, xamlFile);
		var diagnostics = compilation.AddSyntaxTrees(result.GeneratedTrees).GetDiagnostics();
		var warning = Assert.Single(diagnostics.Where(d => d.Id == "CS8622"));
		var location = warning.Location.GetMappedLineSpan();

		Assert.Equal(physicalPath, location.Path);
		Assert.Equal(3, location.StartLinePosition.Line);
	}

    [Fact]
    public void DiagnosticShowsLocationInInputXamlFile()
    {
        var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    x:Class="Test.TestPage">
    <ContentPage.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Resources/Styles\Colors.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </ContentPage.Resources>
</ContentPage>
""";

        var (result, _) = RunGenerator(xaml, string.Empty);

        var generatedCode = result.GeneratedTrees.Single(tree => Path.GetFileName(tree.FilePath) == "Test.xaml.xsg.cs").ToString();
        var expectedFilePath = Path.Combine(Environment.CurrentDirectory, "Test.xaml");
        Assert.True(generatedCode.Contains(@$"#line 9 ""{expectedFilePath}""", StringComparison.Ordinal));
    }

	[Fact]
	public void LineInfoDisabledDefault()
    {
        var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    x:Class="Test.TestPage">
    <ContentPage.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Resources/Styles\Colors.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </ContentPage.Resources>
</ContentPage>
""";

        var (result, _) = RunGenerator(xaml, string.Empty, lineinfo: "default");

        var generatedCode = result.GeneratedTrees.Single(tree => Path.GetFileName(tree.FilePath) == "Test.xaml.xsg.cs").ToString();
        var expectedFilePath = Path.Combine(Environment.CurrentDirectory, "Test.xaml");
        Assert.False(generatedCode.Contains(@$"#line 9 ""{expectedFilePath}""", StringComparison.Ordinal));
    }
}
