using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class Maui13856Tests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn);

	[Fact]
	public void DictionaryWithEnumKeyBindingDoesNotCauseErrors()
	{
		// https://github.com/dotnet/maui/issues/13856
		// Binding to Dictionary<CustomEnum, object> with x:DataType should not cause generator errors
		// Note: SourceGen currently falls back to runtime binding for dictionary indexers (both string and enum keys)
		// This test verifies that enum key bindings don't cause errors in the generator

		var codeBehind =
"""
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Test
{
	public enum UserSetting
	{
		BrowserInvisible,
		GlobalWaitForElementsInBrowserInSek,
		TBD,
	}

	public partial class TestPage : ContentPage
	{
		public TestPage()
		{
			UserSettings = new Dictionary<UserSetting, object>
			{
				{ UserSetting.TBD, "test value" }
			};
			InitializeComponent();
		}

		public Dictionary<UserSetting, object> UserSettings { get; set; }
	}
}
""";

		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="local:TestPage">
	<Entry x:Name="entry" Text="{Binding UserSettings[TBD]}" />
</ContentPage>
""";

		var compilation = CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(codeBehind));

		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		// The generator should not produce any errors
		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.Empty(errors);

		// Verify that a source file was generated
		var generatedSource = result.Results
			.SelectMany(r => r.GeneratedSources)
			.FirstOrDefault(s => s.HintName.Contains("xsg.cs", System.StringComparison.Ordinal));

		Assert.True(generatedSource.SourceText != null, "Expected generated source file with xsg.cs extension");
		var generatedCode = generatedSource.SourceText.ToString();

		// Verify the binding path is in the generated code (even if using runtime binding fallback)
		Assert.Contains("UserSettings[TBD]", generatedCode, System.StringComparison.Ordinal);
	}
}
