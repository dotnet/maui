#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// https://github.com/dotnet/maui/issues/36156: a XAML edit whose generated code does not compile
/// (a "rude" edit) is never applied to the running app. XIHR must keep diffing later edits against the
/// last generation that could have been applied, not against the broken one.
/// </summary>
[Collection("XamlHotReloadTests")]
public class XamlIncrementalHotReloadRudeEditTests : IDisposable
{
	public XamlIncrementalHotReloadRudeEditTests() => XamlHotReloadState.Reset();
	public void Dispose() => XamlHotReloadState.Reset();

	const string ViewModel = """
		namespace TestApp;
		public class ViewModel
		{
			public int Count { get; set; }
		}
		""";

	static string Page(string firstLabel, string secondLabelText, string extraChild = "") => $$"""
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:TestApp"
		             x:Class="TestApp.MainPage"
		             x:DataType="local:ViewModel">
		    <VerticalStackLayout>
		        <Label Text="{{firstLabel}}" />
		        <Label Text="{{secondLabelText}}" />{{extraChild}}
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// Applied baseline: compiles.
	static readonly string Applied = Page("{Count}", "One");
	// Rude edit: the expression generates invalid C# (CS1525), and the second label is edited too.
	static readonly string RudeSyntax = Page("{Count +}", "Two");
	// Rude edit reported by the generator itself (MAUIX2009), with the same second-label edit.
	static readonly string RudeUnresolved = Page("{NoSuchMember}", "Two");
	// The rude expression is fixed; the second-label edit is kept.
	static readonly string Fixed = Page("{Count + 1}", "Two");
	static readonly string FixedThenEdited = Page("{Count + 1}", "Three");

	const string AddedButton = "\n        <Button Text=\"Added\" />";

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void EditAfterRudeEdit_IsDiffedAgainstLastAppliedGeneration(bool generatorReportedError)
	{
		var rude = generatorReportedError ? RudeUnresolved : RudeSyntax;
		var runs = RunEdits(Applied, rude, Fixed, FixedThenEdited);

		Assert.False(runs[1].Compiles, "the rude edit is expected not to compile");
		Assert.True(runs[2].Compiles, runs[2].Errors);
		Assert.True(runs[3].Compiles, runs[3].Errors);

		// The rude edit was never applied, so the running app still shows "One": the patch for the fixed
		// XAML must also carry the second-label edit that was made together with the rude edit.
		Assert.Contains("\"Two\"", runs[2].UC, StringComparison.Ordinal);
		Assert.Contains("Count + 1", runs[2].UC, StringComparison.Ordinal);

		// Later edits are diffed against the fixed (applied) generation again.
		Assert.Contains("\"Three\"", runs[3].UC, StringComparison.Ordinal);
		Assert.DoesNotContain("Count + 1", runs[3].UC, StringComparison.Ordinal);
	}

	[Fact]
	public void ElementAddedWithRudeEdit_IsAddedOnceTheEditIsFixed()
	{
		var runs = RunEdits(
			Applied,
			Page("{Count +}", "One", AddedButton),
			Page("{Count + 1}", "One", AddedButton));

		Assert.False(runs[1].Compiles);
		Assert.True(runs[2].Compiles, runs[2].Errors);
		// The Button was added by an edit that never reached the app: the next patch must add it.
		Assert.Contains("\"Added\"", runs[2].UC, StringComparison.Ordinal);
	}

	[Fact]
	public void RevertAfterRudeEdit_EmitsEmptyPatch()
	{
		var runs = RunEdits(Applied, RudeSyntax, Applied);

		Assert.True(runs[2].Compiles, runs[2].Errors);
		// The app never left the applied state: reverting to it is not a XAML change.
		Assert.Equal(runs[0].UC, runs[2].UC);
	}

	[Fact]
	public void RepeatedEvaluationOfRudeEdit_IsDeterministic()
	{
		var compilation = CreateCompilation();
		var applied = MakeFile(Applied);
		var rude = MakeFile(RudeSyntax);
		var driver = CreateDriver(applied).RunGenerators(compilation);
		var edited = driver.ReplaceAdditionalText(applied.Text, rude.Text)
			.WithUpdatedAnalyzerConfigOptions(SourceGeneratorDriver.CreateAnalyzerConfigOptionsProvider(rude));

		var first = GetSource(edited.RunGenerators(compilation).GetRunResult(), ".uc.xsg.cs");
		var second = GetSource(edited.RunGenerators(compilation).GetRunResult(), ".uc.xsg.cs");

		Assert.Equal(first, second);
	}

	[Fact]
	public void ErrorDowngradedByTheUser_DoesNotRollBack()
	{
		// MAUIX2002 is an error by default; the user downgraded it, so the build (and every edit) succeeds.
		var compilation = CreateCompilation();
		compilation = compilation.WithOptions(compilation.Options.WithSpecificDiagnosticOptions(
			new Dictionary<string, ReportDiagnostic> { ["MAUIX2002"] = ReportDiagnostic.Warn }));
		var runs = RunEdits(compilation,
			Page("{Count}\" NoSuchProperty=\"x", "One"),
			Page("{Count}\" NoSuchProperty=\"x", "One", AddedButton),
			Page("{Count}\" NoSuchProperty=\"x", "Two", AddedButton));

		Assert.True(runs[1].Compiles, runs[1].Errors);
		Assert.Contains("\"Added\"", runs[1].UC, StringComparison.Ordinal);
		// The Button was applied with the previous edit: it must not be added a second time.
		Assert.True(runs[2].Compiles, runs[2].Errors);
		Assert.Contains("\"Two\"", runs[2].UC, StringComparison.Ordinal);
		Assert.DoesNotContain("\"Added\"", runs[2].UC, StringComparison.Ordinal);
	}

	sealed record Generation(string UC, bool Compiles, string Errors);

	static List<Generation> RunEdits(params string[] xamls) => RunEdits(CreateCompilation(), xamls);

	static List<Generation> RunEdits(Compilation compilation, params string[] xamls)
	{
		var files = xamls.Select(MakeFile).ToArray();
		var generations = new List<Generation>();
		GeneratorDriver driver = CreateDriver(files[0]);
		for (var i = 0; i < files.Length; i++)
		{
			if (i > 0)
				driver = driver.ReplaceAdditionalText(files[i - 1].Text, files[i].Text)
					.WithUpdatedAnalyzerConfigOptions(SourceGeneratorDriver.CreateAnalyzerConfigOptionsProvider(files[i]));
			driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out var generatorDiagnostics);
			var errors = generatorDiagnostics.Concat(updated.GetDiagnostics())
				.Where(d => d.Severity == DiagnosticSeverity.Error)
				.ToArray();
			generations.Add(new Generation(
				GetSource(driver.GetRunResult(), ".uc.xsg.cs") ?? throw new InvalidOperationException($"No UpdateComponent() for edit {i}"),
				errors.Length == 0,
				string.Join(Environment.NewLine, errors.Select(e => e.ToString()))));
		}
		return generations;
	}

	static Compilation CreateCompilation() =>
		SourceGeneratorDriver.CreateMauiCompilation("TestApp").AddSyntaxTrees(CSharpSyntaxTree.ParseText(ViewModel));

	static SourceGeneratorDriver.AdditionalFile MakeFile(string xaml) =>
		new(SourceGeneratorDriver.ToAdditionalText("MainPage.xaml", xaml),
			Kind: "Xaml",
			RelativePath: "MainPage.xaml",
			TargetPath: "TestApp/MainPage.xaml",
			ManifestResourceName: null,
			TargetFramework: "net11.0",
			NoWarn: null,
			EnableIncrementalHotReload: true);

	static GeneratorDriver CreateDriver(SourceGeneratorDriver.AdditionalFile file) =>
		CSharpGeneratorDriver.Create([new XamlGenerator().AsSourceGenerator()])
			.AddAdditionalTexts(ImmutableArray.Create(file.Text))
			.WithUpdatedAnalyzerConfigOptions(SourceGeneratorDriver.CreateAnalyzerConfigOptionsProvider(file));

	static string? GetSource(GeneratorDriverRunResult result, string hintSuffix) =>
		result.Results.SelectMany(r => r.GeneratedSources)
			.FirstOrDefault(s => s.HintName.EndsWith(hintSuffix, StringComparison.OrdinalIgnoreCase))
			.SourceText?.ToString();
}
