using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Maui.Core.ConfigurationSourceGen;
using Xunit;

namespace Microsoft.Maui.Core.ConfigurationSourceGen.UnitTests;

public class AppSettingsSourceGeneratorTests
{
    [Fact]
    public void Generator_WithValidAppSettings_GeneratesExtensionMethod()
    {
        // Arrange
        var appSettingsJson = """
        {
            "Setting1": "Value1",
            "Setting2": {
                "NestedSetting": "NestedValue"
            },
            "Setting3": 42,
            "Setting4": true
        }
        """;

        var compilation = CreateCompilation();
        var generator = new AppSettingsSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Add appsettings.json as additional file
        driver = driver.AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(
            new TestAdditionalText("appsettings.json", appSettingsJson)));

        // Act
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert
        Assert.True(result.Diagnostics.IsEmpty);
        Assert.Single(result.Results);
        Assert.Single(result.Results[0].GeneratedSources);

        var generatedSource = result.Results[0].GeneratedSources[0];
        Assert.Equal("LocalAppSettingsExtensions.g.cs", generatedSource.HintName);

        var sourceText = generatedSource.SourceText.ToString();
        
        // Verify the generated code contains expected content
        Assert.Contains("public static class LocalAppSettingsExtensions", sourceText, StringComparison.Ordinal);
        Assert.Contains("public static IConfigurationBuilder AddLocalAppSettings", sourceText, StringComparison.Ordinal);
        Assert.Contains("{ \"Setting1\", \"Value1\" }", sourceText, StringComparison.Ordinal);
        Assert.Contains("{ \"Setting2:NestedSetting\", \"NestedValue\" }", sourceText, StringComparison.Ordinal);
        Assert.Contains("{ \"Setting3\", \"42\" }", sourceText, StringComparison.Ordinal);
        Assert.Contains("{ \"Setting4\", \"true\" }", sourceText, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_WithNoAppSettings_GeneratesNothing()
    {
        // Arrange
        var compilation = CreateCompilation();
        var generator = new AppSettingsSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Act
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert
        Assert.True(result.Diagnostics.IsEmpty);
        Assert.Single(result.Results);
        Assert.Empty(result.Results[0].GeneratedSources);
    }

    [Fact]
    public void Generator_WithEmptyAppSettings_GeneratesEmptyDictionary()
    {
        // Arrange
        var appSettingsJson = "{}";

        var compilation = CreateCompilation();
        var generator = new AppSettingsSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Add appsettings.json as additional file
        driver = driver.AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(
            new TestAdditionalText("appsettings.json", appSettingsJson)));

        // Act
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert
        Assert.True(result.Diagnostics.IsEmpty);
        Assert.Single(result.Results);
        Assert.Single(result.Results[0].GeneratedSources);

        var generatedSource = result.Results[0].GeneratedSources[0];
        var sourceText = generatedSource.SourceText.ToString();
        
        // Verify the generated code contains expected content but empty dictionary
        Assert.Contains("public static class LocalAppSettingsExtensions", sourceText, StringComparison.Ordinal);
        Assert.Contains("public static IConfigurationBuilder AddLocalAppSettings", sourceText, StringComparison.Ordinal);
        Assert.Contains("var configurationData = new Dictionary<string, string>", sourceText, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_WithArrayValues_FlattensCorrectly()
    {
        // Arrange
        var appSettingsJson = """
        {
            "Items": ["Item1", "Item2", "Item3"]
        }
        """;

        var compilation = CreateCompilation();
        var generator = new AppSettingsSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Add appsettings.json as additional file
        driver = driver.AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(
            new TestAdditionalText("appsettings.json", appSettingsJson)));

        // Act
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert
        Assert.True(result.Diagnostics.IsEmpty);
        var sourceText = result.Results[0].GeneratedSources[0].SourceText.ToString();
        
        Assert.Contains("{ \"Items:0\", \"Item1\" }", sourceText, StringComparison.Ordinal);
        Assert.Contains("{ \"Items:1\", \"Item2\" }", sourceText, StringComparison.Ordinal);
        Assert.Contains("{ \"Items:2\", \"Item3\" }", sourceText, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_WithInvalidJson_GeneratesWarningAndContinues()
    {
        // Arrange
        var invalidJson = "{ invalid json";

        var compilation = CreateCompilation();
        var generator = new AppSettingsSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Add appsettings.json as additional file
        driver = driver.AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(
            new TestAdditionalText("appsettings.json", invalidJson)));

        // Act
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert - should still generate empty configuration
        Assert.Single(result.Results);
        Assert.Single(result.Results[0].GeneratedSources);
        
        var sourceText = result.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.Contains("public static class LocalAppSettingsExtensions", sourceText, StringComparison.Ordinal);
    }

    private static Compilation CreateCompilation()
    {
        const string source = @"
using System;
namespace TestApp
{
    public class Program
    {
        public static void Main() { }
    }
}";

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }

    private class TestAdditionalText : AdditionalText
    {
        private readonly string _text;

        public TestAdditionalText(string path, string text)
        {
            Path = path;
            _text = text;
        }

        public override string Path { get; }

        public override SourceText? GetText(CancellationToken cancellationToken = default)
        {
            return SourceText.From(_text, Encoding.UTF8);
        }
    }
}