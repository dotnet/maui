using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

[Collection("XamlHotReloadTests")]
public class LazyStyleHotReload : SourceGenXamlInitializeComponentTestBase, IDisposable
{
	public LazyStyleHotReload() => XamlHotReloadState.Reset();
	public void Dispose() => XamlHotReloadState.Reset();

	[Fact]
	public void InlineStyleWithLiteralSetterCompilesWithIncrementalHotReload()
	{
		var xaml = """
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			 xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			 x:Class="Test.TestPage">
	<Label>
		<Label.Style>
			<Style TargetType="Label">
				<Setter Property="Text" Value="hello" />
			</Style>
		</Label.Style>
	</Label>
</ContentPage>
""";

		var code = """
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

		var compilation = SourceGeneratorDriver.CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken));
		var xamlFile = new AdditionalXamlFile(Path.Combine(Environment.CurrentDirectory, "Test.xaml"), xaml, RelativePath: "Test.xaml",
			ManifestResourceName: $"{compilation.AssemblyName}.Test.xaml", EnableIncrementalHotReload: true, TrimmableStyles: "true");
		var result = SourceGeneratorDriver.RunGenerator<XamlGenerator>(compilation, xamlFile);
		var generated = result.Results.Single().GeneratedSources
			.Single(source => source.HintName.EndsWith(".xsg.cs", StringComparison.Ordinal) && !source.HintName.EndsWith(".handler.xsg.cs", StringComparison.Ordinal) && !source.HintName.Contains(".uc.xsg", StringComparison.Ordinal))
			.SourceText.ToString();
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		Assert.NotNull(generated);
		Assert.Contains("LazyInitialization", generated, StringComparison.Ordinal);
		Assert.DoesNotContain(", );", generated, StringComparison.Ordinal);
	}
}
