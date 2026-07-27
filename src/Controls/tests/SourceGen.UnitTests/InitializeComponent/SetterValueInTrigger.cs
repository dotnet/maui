using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class SetterValueInTrigger : SourceGenXamlInitializeComponentTestBase
{
	const string TestXaml = """
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage x:Class="Test.TestPage">
	<Button>
		<Button.Style>
			<Style TargetType="Button">
				<Style.Triggers>
					<Trigger TargetType="Button" Property="IsEnabled" Value="True">
						<Setter Property="ImageSource">
							<Setter.Value>
								<FontImageSource FontFamily="Arial" Glyph="A" Size="16" />
							</Setter.Value>
						</Setter>
					</Trigger>
				</Style.Triggers>
			</Style>
		</Button.Style>
	</Button>
</ContentPage>
""";

	const string TestCode = """
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

	[Fact]
	public void SetterWithComplexValueInTriggerIsAdded()
	{
		// Reproduction from https://github.com/dotnet/maui/issues/34039
		// When <Setter.Value> is a property element, GetValueNode() must find it
		// regardless of the namespace URI on the property element.
		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(TestCode))
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText("[assembly: global::Microsoft.Maui.Controls.Xaml.Internals.AllowImplicitXmlnsDeclaration]"));

		var workingDirectory = Environment.CurrentDirectory;
		var xamlFile = new AdditionalXamlFile(
			System.IO.Path.Combine(workingDirectory, "Test.xaml"), TestXaml,
			RelativePath: "Test.xaml",
			ManifestResourceName: $"{compilation.AssemblyName}.Test.xaml");
		var result = RunGenerator<XamlGenerator>(compilation, xamlFile);
		var generated = result.Results.SingleOrDefault().GeneratedSources
			.SingleOrDefault(gs => gs.HintName.EndsWith(".xsg.cs")).SourceText?.ToString();

		Assert.NotNull(generated);
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error),
			$"Generator produced errors: {string.Join(", ", result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))}");

		// The setter must be added to the trigger's Setters collection.
		// Without the fix, GetValueNode() fails to find the Value property element,
		// causing the setter to be removed from Variables and the .Add() call to be skipped.
		Assert.Contains("Setters).Add(", generated, StringComparison.Ordinal);
	}
}
