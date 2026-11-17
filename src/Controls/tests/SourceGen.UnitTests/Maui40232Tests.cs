#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.SourceGen.TypeConverters;
using Microsoft.Maui.Graphics;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

/// <summary>
/// https://github.com/dotnet/maui/issues/40232: <c>&lt;StyleSheet Source="..." /&gt;</c> must not generate calls
/// to internal Controls APIs, since app assemblies are not friends of Microsoft.Maui.Controls.
/// </summary>
public class Maui40232Tests : SourceGenTestsBase
{
	// Must not match any InternalsVisibleTo entry of Microsoft.Maui.Controls.
	const string ConsumerAssemblyName = "Maui40232.ConsumerApp";

	record AdditionalXamlFile(string Path, string Content, string? RelativePath = null)
		: AdditionalFile(Text: ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: RelativePath ?? Path, ManifestResourceName: ManifestNameFor(RelativePath ?? Path), TargetFramework: null, NoWarn: null);

	record AdditionalCssFile(string Path, string Content)
		: AdditionalFile(Text: ToAdditionalText(Path, Content), Kind: "Css", RelativePath: Path, TargetPath: Path, ManifestResourceName: ManifestNameFor(Path), TargetFramework: null, NoWarn: "");

	static string ManifestNameFor(string path) => $"{ConsumerAssemblyName}.{path.Replace('\\', '.').Replace('/', '.')}";

	static string PageXaml(string source) =>
$"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<StyleSheet Source="{source}" />
	</ContentPage.Resources>
	<Label x:Name="label" Text="Hello" />
</ContentPage>
""";

	const string PageCode =
"""
namespace Test;

public partial class TestPage : Microsoft.Maui.Controls.ContentPage
{
	public TestPage() => InitializeComponent();
}
""";

	const string RedLabelCss = "label { color: red; }";

	static Compilation CreateConsumerCompilation() =>
		CreateMauiCompilation(ConsumerAssemblyName).AddSyntaxTrees(CSharpSyntaxTree.ParseText(PageCode, path: "TestPage.xaml.cs"));

	static string GetInitializeComponentSource(GeneratorDriverRunResult result) =>
		result.Results.Single().GeneratedSources
			.Single(s => s.HintName.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase) && !s.HintName.Contains("uc.xsg", StringComparison.OrdinalIgnoreCase))
			.SourceText.ToString();

	static string RuntimeGetResourcePath(string source, string rootTargetPath)
	{
		var converter = typeof(ResourceDictionary).GetNestedType("RDSourceTypeConverter", BindingFlags.Public | BindingFlags.NonPublic)!;
		var method = converter.GetMethod("GetResourcePath", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;
		return (string)method.Invoke(null, [new Uri(source, UriKind.Relative), rootTargetPath])!;
	}

	[Theory]
	[InlineData("MainPage.xaml", "style.css")]
	[InlineData("MainPage.xaml", "Styles/app.css")]
	[InlineData("MainPage.xaml", "/Styles/app.css")]
	[InlineData("MainPage.xaml", "./style.css")]
	[InlineData("Pages/MainPage.xaml", "page.css")]
	[InlineData("Pages/MainPage.xaml", "../Styles/app.css")]
	[InlineData("Pages/MainPage.xaml", "/Styles/app.css")]
	[InlineData("Pages/Sub/MainPage.xaml", "../../root.css")]
	[InlineData("Pages/Sub/MainPage.xaml", "./nested/deep.css")]
	[InlineData("Pages/MainPage.xaml", "../../../outside.css")]
	[InlineData("Pages/MainPage.xaml", "my style.css")]
	[InlineData("Pages/MainPage.xaml", "it's.css")]
	[InlineData("Pages/MainPage.xaml", "q\"uote.css")]
	[InlineData("Pages/MainPage.xaml", "back\\slash.css")]
	[InlineData("Pages/MainPage.xaml", "style.css?v=1#frag")]
	[InlineData("Pages/MainPage.xaml", "caf\u00e9.css")]
	public void GenerationTimeResourcePathMatchesRuntime(string rootTargetPath, string source)
	{
		Assert.Equal(RuntimeGetResourcePath(source, rootTargetPath), RDSourceConverter.GetResourcePath(source, rootTargetPath));
	}

	[Theory]
	[InlineData("TestPage.xaml", "style.css", "style.css")]
	[InlineData("TestPage.xaml", "/Styles/app.css", "Styles/app.css")]
	[InlineData("Pages/TestPage.xaml", "page.css", "Pages/page.css")]
	[InlineData("Pages/TestPage.xaml", "../Styles/app.css", "Styles/app.css")]
	[InlineData("Pages/TestPage.xaml", "./nested/page.css", "Pages/nested/page.css")]
	[InlineData("Pages/TestPage.xaml", "/Styles/app.css", "Styles/app.css")]
	[InlineData("Pages\\Sub\\TestPage.xaml", "../shared.css", "Pages/shared.css")]
	public void StyleSheetSourceCompilesInNonFriendAssembly(string xamlRelativePath, string source, string expectedResourcePath)
	{
		var result = RunGenerator<XamlGenerator>(CreateConsumerCompilation(), new AdditionalXamlFile("TestPage.xaml", PageXaml(source), xamlRelativePath));

		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		var generated = GetInitializeComponentSource(result);
		Assert.DoesNotContain("RDSourceTypeConverter", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("GetPathForType", generated, StringComparison.Ordinal);
		Assert.Contains($"global::Microsoft.Maui.Controls.StyleSheets.StyleSheet.FromResource(\"{expectedResourcePath}\", typeof(global::Test.TestPage).Assembly, new global::Microsoft.Maui.Controls.Xaml.XmlLineInfo(7, 4))", generated, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("q&quot;uote.css", "q\"uote.css")]
	[InlineData("it&apos;s.css", "it's.css")]
	[InlineData("my style.css", "my style.css")]
	[InlineData("back\\slash.css", "back\\slash.css")]
	[InlineData("{}{braces}.css", "{braces}.css")]
	[InlineData("caf\u00e9.css", "caf\u00e9.css")]
	public void StyleSheetSourceWithSpecialCharactersProducesValidStringLiteral(string xmlSource, string source)
	{
		var result = RunGenerator<XamlGenerator>(CreateConsumerCompilation(), new AdditionalXamlFile("TestPage.xaml", PageXaml(xmlSource), "Pages/TestPage.xaml"));

		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		var expectedLiteral = SymbolDisplay.FormatLiteral(RuntimeGetResourcePath(source, "Pages/TestPage.xaml"), quote: true);
		Assert.Contains($"FromResource({expectedLiteral},", GetInitializeComponentSource(result), StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("http://example.com/style.css")]
	[InlineData("file:///tmp/style.css")]
	public void AbsoluteStyleSheetSourceReportsDiagnostic(string source)
	{
		var result = RunGenerator<XamlGenerator>(CreateConsumerCompilation(), new AdditionalXamlFile("TestPage.xaml", PageXaml(source)), assertNoCompilationErrors: false);

		var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == "MAUIG1001");
		Assert.Contains(source, diagnostic.GetMessage(), StringComparison.Ordinal);
		Assert.Equal(7, diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1);
		Assert.DoesNotContain("RDSourceTypeConverter", GetInitializeComponentSource(result), StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("TestPage.xaml", "style.css", "style.css")]
	[InlineData("TestPage.xaml", "/Styles/app.css", "Styles/app.css")]
	[InlineData("Pages/TestPage.xaml", "page.css", "Pages/page.css")]
	[InlineData("Pages/TestPage.xaml", "../Styles/app.css", "Styles/app.css")]
	[InlineData("Pages/Sub/TestPage.xaml", "/Styles/app.css", "Styles/app.css")]
	public void StyleSheetSourceLoadsEmbeddedCssAtRuntime(string xamlRelativePath, string source, string cssPath)
	{
		var page = EmitAndCreatePage(xamlRelativePath, source, cssPath, RedLabelCss);

		var label = Assert.IsType<Label>(page.FindByName<Label>("label"));
		Assert.Equal(Colors.Red, label.TextColor);
	}

	[Fact]
	public void MissingStyleSheetResourceThrowsWithLineInfo()
	{
		var ex = Assert.ThrowsAny<Exception>(() => EmitAndCreatePage("Pages/TestPage.xaml", "missing.css", "Pages/other.css", RedLabelCss));

		var inner = ex is TargetInvocationException tie ? tie.InnerException! : ex;
		Assert.Equal("Microsoft.Maui.Controls.Xaml.XamlParseException", inner.GetType().FullName);
		Assert.Contains("Position 7:4.", inner.Message, StringComparison.Ordinal);
		Assert.Contains("Resource 'Pages/missing.css' not found.", inner.Message, StringComparison.Ordinal);
	}

	static ContentPage EmitAndCreatePage(string xamlRelativePath, string source, string cssPath, string css)
	{
		var compilation = CreateConsumerCompilation();
		var xamlFile = new AdditionalXamlFile("TestPage.xaml", PageXaml(source), xamlRelativePath);
		var cssFile = new AdditionalCssFile(cssPath, css);

		var generator = new XamlGenerator().AsSourceGenerator();
		var driver = CSharpGeneratorDriver.Create([generator])
			.AddAdditionalTexts([xamlFile.Text, cssFile.Text])
			.WithUpdatedAnalyzerConfigOptions(CreateAnalyzerConfigOptionsProvider(xamlFile, cssFile));
		driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out var generatorDiagnostics);
		Assert.DoesNotContain(generatorDiagnostics, d => d.Severity == DiagnosticSeverity.Error);

		using var pe = new MemoryStream();
		var cssBytes = Encoding.UTF8.GetBytes(css);
		var emit = updated.Emit(pe, manifestResources: [new ResourceDescription(cssFile.ManifestResourceName!, () => new MemoryStream(cssBytes), isPublic: true)]);
		Assert.True(emit.Success, string.Join(Environment.NewLine, emit.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));

		pe.Position = 0;
		var assembly = new AssemblyLoadContext($"Maui40232_{Guid.NewGuid():N}", isCollectible: true).LoadFromStream(pe);
		return Assert.IsAssignableFrom<ContentPage>(Activator.CreateInstance(assembly.GetType("Test.TestPage")!));
	}
}
