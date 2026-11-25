using System;
using System.Linq;
using Microsoft.CodeAnalysis;
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

		var (result, generated) = RunGenerator(xaml, code, enableIncrementalHotReload: true);
		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		Assert.NotNull(generated);
		Assert.DoesNotContain(", );", generated, StringComparison.Ordinal);
	}
}
