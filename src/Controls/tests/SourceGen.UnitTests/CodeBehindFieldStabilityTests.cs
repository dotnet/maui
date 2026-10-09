using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

// This collection is also used by the XAML Incremental Hot Reload test harness: it disables
// parallelization because XamlHotReloadState is a process-wide static cache keyed on
// (AssemblyName, TargetFramework, RelativePath). Running these tests concurrently with other
// tests touching the same cache keys would make the assertions flaky.
[Collection("XamlHotReloadTests")]
public class CodeBehindFieldStabilityTests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, bool EnableIncrementalHotReload = true)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: Path, TargetPath: null, ManifestResourceName: null, TargetFramework: null, NoWarn: null, EnableIncrementalHotReload: EnableIncrementalHotReload);

	public CodeBehindFieldStabilityTests() => XamlHotReloadState.Reset();

	// Regression test for https://github.com/dotnet/maui/issues/38993:
	// renaming x:Name during a live Hot Reload session must not remove the previously generated
	// field, or Roslyn's EnC analyzer reports ENC0020 ("Renaming field 'x' requires restarting
	// the application") even though the user never renamed a C# field directly.
	[Fact]
	public void RenamingXName_PreservesOriginalGeneratedField()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="Test.RenamingXName_PreservesOriginalGeneratedField">
				<Label x:Name="count" Text="0" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="Test.RenamingXName_PreservesOriginalGeneratedField">
				<Label x:Name="total" Text="0" />
			</ContentPage>
			""";

		var xamlFile = new AdditionalXamlFile("RenamingXName_PreservesOriginalGeneratedField.xaml", xamlV1);
		var compilation = CreateMauiCompilation();
		var result = RunGeneratorWithChanges<XamlGenerator>(compilation, ApplyChanges, xamlFile);

		var output1 = GetCbOutput(result.result1);
		var output2 = GetCbOutput(result.result2);

		Assert.Contains("Microsoft.Maui.Controls.Label count", output1, StringComparison.Ordinal);

		// The field generated for the original name must still be declared after the rename:
		// removing it is what makes Roslyn's EnC engine see a field deletion/rename.
		Assert.Contains("count", output2, StringComparison.Ordinal);
		// And the newly-named field must be added alongside it (additive-only change).
		Assert.Contains("Microsoft.Maui.Controls.Label total", output2, StringComparison.Ordinal);

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			var newXamlFile = new AdditionalXamlFile(xamlFile.Path, xamlV2);
			driver = driver.ReplaceAdditionalText(xamlFile.Text, newXamlFile.Text);
			return (driver, compilation);
		}
	}

	// A removed field must not disappear either: removing an x:Name altogether is the same
	// "field deletion" shape from Roslyn's point of view as a rename.
	[Fact]
	public void RemovingXName_PreservesOriginalGeneratedField()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="Test.RemovingXName_PreservesOriginalGeneratedField">
				<Label x:Name="count" Text="0" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="Test.RemovingXName_PreservesOriginalGeneratedField">
				<Label Text="0" />
			</ContentPage>
			""";

		var xamlFile = new AdditionalXamlFile("RemovingXName_PreservesOriginalGeneratedField.xaml", xamlV1);
		var compilation = CreateMauiCompilation();
		var result = RunGeneratorWithChanges<XamlGenerator>(compilation, ApplyChanges, xamlFile);

		var output2 = GetCbOutput(result.result2);

		Assert.Contains("count", output2, StringComparison.Ordinal);

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			var newXamlFile = new AdditionalXamlFile(xamlFile.Path, xamlV2);
			driver = driver.ReplaceAdditionalText(xamlFile.Text, newXamlFile.Text);
			return (driver, compilation);
		}
	}

	// Reusing a previously-generated field name for an incompatible element type must not change
	// the declared type of the existing field (a type change is itself an EnC rude edit); the
	// original field must be kept as-is and simply left unassigned.
	[Fact]
	public void ReusingXNameWithIncompatibleType_KeepsOriginalFieldTypeAndDoesNotCrashOnReplace()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="Test.ReusingXNameWithIncompatibleType">
				<Label x:Name="NamedElement" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="Test.ReusingXNameWithIncompatibleType">
				<Button x:Name="NamedElement" />
			</ContentPage>
			""";

		var xamlFile = new AdditionalXamlFile("ReusingXNameWithIncompatibleType.xaml", xamlV1);
		var compilation = CreateMauiCompilation();
		var result = RunGeneratorWithChanges<XamlGenerator>(compilation, ApplyChanges, xamlFile);

		var output2 = GetCbOutput(result.result2);

		// The originally-declared type (Label) must be preserved - not replaced with Button.
		Assert.Contains("Microsoft.Maui.Controls.Label NamedElement", output2, StringComparison.Ordinal);
		Assert.DoesNotContain("Microsoft.Maui.Controls.Button NamedElement", output2, StringComparison.Ordinal);

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			var newXamlFile = new AdditionalXamlFile(xamlFile.Path, xamlV2);
			driver = driver.ReplaceAdditionalText(xamlFile.Text, newXamlFile.Text);
			return (driver, compilation);
		}
	}

	// Reusing an x:Name on a type that's assignable to the originally-generated field's type (e.g.
	// a derived control) must keep the field assigned - not ghosted - since reassigning it is safe
	// and compiles fine. This exercises the real IsAssignableToGhostField check (not just string
	// equality) and validates the regenerated code against a live Compilation, so a regression in
	// either the CB reconciliation or the IC assignment-skip guard would surface as a real
	// compilation error or a wrongly-skipped assignment, not just a text mismatch.
	[Fact]
	public void ReusingXNameWithAssignableDerivedType_KeepsFieldAssigned_AndGeneratedCodeCompiles()
	{
		const string codeBehind = """
			using Microsoft.Maui.Controls;
			using Microsoft.Maui.Controls.Xaml;

			namespace Test;

			[XamlProcessing(XamlInflator.SourceGen)]
			public partial class ReusingXNameWithAssignableDerivedType : ContentPage
			{
				public ReusingXNameWithAssignableDerivedType()
				{
					InitializeComponent();
				}
			}

			public class MyLabel : Label
			{
			}
			""";
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:Test"
			             x:Class="Test.ReusingXNameWithAssignableDerivedType">
				<Label x:Name="NamedElement" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:Test"
			             x:Class="Test.ReusingXNameWithAssignableDerivedType">
				<local:MyLabel x:Name="NamedElement" />
			</ContentPage>
			""";

		var xamlFile = new AdditionalXamlFile("ReusingXNameWithAssignableDerivedType.xaml", xamlV1);
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(codeBehind));

		var (_, _, updatedCompilation2, diagnostics2) = RunGeneratorWithChangesAndUpdateCompilation<XamlGenerator>(compilation, ApplyChanges, xamlFile);

		AssertNoErrors(diagnostics2, updatedCompilation2);

		var icOutput2 = GetIcOutput(updatedCompilation2);
		// The field must still be declared as the preserved type (Label), but assigned - the
		// assignability check allows reusing it for a derived (assignable) type.
		Assert.Contains("this.NamedElement = ", icOutput2, StringComparison.Ordinal);

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			var newXamlFile = new AdditionalXamlFile(xamlFile.Path, xamlV2);
			driver = driver.ReplaceAdditionalText(xamlFile.Text, newXamlFile.Text);
			return (driver, compilation);
		}
	}

	// Strengthens ReusingXNameWithIncompatibleType_KeepsOriginalFieldTypeAndDoesNotCrashOnReplace:
	// beyond string-matching the generated .sg.cs text, this validates the regenerated code
	// (including the IC assignment-skip guard) against a real Compilation, proving the skipped
	// assignment doesn't just avoid a crash in this test harness but actually compiles cleanly.
	[Fact]
	public void ReusingXNameWithIncompatibleType_SkipsAssignment_AndGeneratedCodeCompiles()
	{
		const string codeBehind = """
			using Microsoft.Maui.Controls;
			using Microsoft.Maui.Controls.Xaml;

			namespace Test;

			[XamlProcessing(XamlInflator.SourceGen)]
			public partial class ReusingXNameWithIncompatibleType_Compiles : ContentPage
			{
				public ReusingXNameWithIncompatibleType_Compiles()
				{
					InitializeComponent();
				}
			}
			""";
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="Test.ReusingXNameWithIncompatibleType_Compiles">
				<Label x:Name="NamedElement" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="Test.ReusingXNameWithIncompatibleType_Compiles">
				<Button x:Name="NamedElement" />
			</ContentPage>
			""";

		var xamlFile = new AdditionalXamlFile("ReusingXNameWithIncompatibleType_Compiles.xaml", xamlV1);
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(codeBehind));

		var (_, _, updatedCompilation2, diagnostics2) = RunGeneratorWithChangesAndUpdateCompilation<XamlGenerator>(compilation, ApplyChanges, xamlFile);

		AssertNoErrors(diagnostics2, updatedCompilation2);

		var icOutput2 = GetIcOutput(updatedCompilation2);
		// The ghosted field must be left unassigned - assigning a Button to a Label-typed field
		// would be a compile error, which AssertNoErrors above would have caught.
		Assert.DoesNotContain("this.NamedElement = ", icOutput2, StringComparison.Ordinal);

		(GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation compilation)
		{
			var newXamlFile = new AdditionalXamlFile(xamlFile.Path, xamlV2);
			driver = driver.ReplaceAdditionalText(xamlFile.Text, newXamlFile.Text);
			return (driver, compilation);
		}
	}

	static void AssertNoErrors(System.Collections.Immutable.ImmutableArray<Diagnostic> generatorDiagnostics, Compilation updatedCompilation)
	{
		var generatorErrors = generatorDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
		Assert.True(generatorErrors.Length == 0, "Generator diagnostics:\n" + string.Join("\n", generatorErrors.Select(d => d.ToString())));

		var compileErrors = updatedCompilation.GetDiagnostics()
			.Where(d => d.Severity == DiagnosticSeverity.Error)
			.Where(d => d.Location.SourceTree?.FilePath?.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase) == true)
			.ToArray();
		Assert.True(compileErrors.Length == 0, "Compilation errors:\n" + string.Join("\n", compileErrors.Select(d => d.ToString())));
	}

	// The InitializeComponent output only - exercises SetFieldsForXNamesVisitor's ghost-field
	// assignment-skip guard (what this fix changes). Excludes the separate UpdateComponent
	// (.uc.xsg.cs) output, which already had its own independent, symbol-based assignability
	// check (UpdateComponentCodeWriter.IsAssignableTo) and would otherwise mask a regression here.
	static string GetIcOutput(Compilation compilation) =>
		string.Join("\n", compilation.SyntaxTrees
			.Where(t => t.FilePath.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase)
				&& !t.FilePath.EndsWith(".uc.xsg.cs", StringComparison.OrdinalIgnoreCase))
			.Select(t => t.ToString()));

	static string GetCbOutput(GeneratorDriverRunResult result) =>
		result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs", StringComparison.OrdinalIgnoreCase)).SourceText.ToString();
}
