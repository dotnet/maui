using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class LineInfoTests : SourceGenXamlInitializeComponentTestBase
{
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
