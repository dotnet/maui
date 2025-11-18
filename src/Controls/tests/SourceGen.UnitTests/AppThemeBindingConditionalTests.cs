using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class AppThemeBindingConditionalTests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn);

#if NET11_0_OR_GREATER
	[Fact]
	public void AppThemeBindingExtension_IsRegistered_InNet11OrGreater()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="{AppThemeBinding Light='Light Mode', Dark='Dark Mode'}" />
</ContentPage>
""";
		var compilation = CreateMauiCompilation();
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		// Should not have diagnostics - AppThemeBindingExtension is registered
		Assert.False(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));

		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		// Verify AppThemeBinding is used from Internals namespace
		Assert.Contains("Microsoft.Maui.Controls.Internals.AppThemeBinding", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void AppThemeBinding_IsPublic_InNet11OrGreater()
	{
		var code =
		"""
using Microsoft.Maui.Controls.Internals;

namespace Test
{
	public class TestClass
	{
		public void TestMethod()
		{
			// AppThemeBinding should be accessible as public in NET11_0_OR_GREATER
			var binding = new AppThemeBinding
			{
				Light = "Light",
				Dark = "Dark"
			};
		}
	}
}
""";
		var compilation = CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));

		var diagnostics = compilation.GetDiagnostics();
		var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

		// Should compile without errors - AppThemeBinding is public
		Assert.Empty(errors);
	}
#else
	[Fact]
	public void AppThemeBindingExtension_IsNotRegistered_BeforeNet11()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="{AppThemeBinding Light='Light Mode', Dark='Dark Mode'}" />
</ContentPage>
""";
		var compilation = CreateMauiCompilation();
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml));

		// AppThemeBindingExtension should not be registered in early markup extensions
		// It will fall back to runtime resolution via IProvideValueTarget
		var generated = result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();

		// Should NOT contain the Internals.AppThemeBinding initialization
		// Instead it should use the markup extension at runtime
		Assert.DoesNotContain("Microsoft.Maui.Controls.Internals.AppThemeBinding", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void AppThemeBinding_IsInternal_BeforeNet11()
	{
		var code =
		"""
using Microsoft.Maui.Controls.Internals;

namespace Test
{
	public class TestClass
	{
		public void TestMethod()
		{
			// AppThemeBinding should NOT be accessible from external code before NET11_0_OR_GREATER
			var binding = new AppThemeBinding
			{
				Light = "Light",
				Dark = "Dark"
			};
		}
	}
}
""";
		var compilation = CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));

		var diagnostics = compilation.GetDiagnostics();
		var errors = diagnostics.Where(d => 
			d.Severity == DiagnosticSeverity.Error && 
			d.Id == "CS0122" // 'type' is inaccessible due to its protection level
		).ToList();

		// Should have accessibility error - AppThemeBinding is internal
		Assert.NotEmpty(errors);
	}
#endif
}
