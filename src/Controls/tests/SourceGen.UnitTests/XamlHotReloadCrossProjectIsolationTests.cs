#nullable enable
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Regression tests for cross-project isolation of the incremental hot reload patch chain.
///
/// The generator's <c>XamlHotReloadState</c> is process-global mutable static state, and Roslyn
/// source generators run inside a long-lived VBCSCompiler shared across many project builds. If the
/// patch chain were keyed only on (AssemblyName, TargetFramework, RelativePath), two projects that
/// share an assembly name and a file name (e.g. two apps both named "MauiApp.1" with a "MainPage.xaml")
/// would leak patches into each other. A leaked patch can reference a type the other project doesn't
/// reference — e.g. a <c>BlazorWebView</c> (Microsoft.AspNetCore.Components.WebView.Maui) from a Blazor
/// app bleeding into a plain MAUI app — producing generated code that fails to compile (CS0234).
///
/// The state is therefore keyed on the XAML file's absolute path (ProjectItem.HotReloadStateKey),
/// which is unique per project on disk yet stable across incremental builds.
/// </summary>
public class XamlHotReloadCrossProjectIsolationTests
{
	const string PageWrap = """
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestApp.MainPage">
		    <VerticalStackLayout>
		        <Label Text="Hello" />
		        {0}
		    </VerticalStackLayout>
		</ContentPage>
		""";

	// Both "projects" use the SAME project-relative path ("MainPage.xaml") — as every MAUI app does —
	// but distinct absolute paths, exactly like two apps built in different directories. The old
	// (AssemblyName, TFM, RelativePath) key collided on this; the absolute-path key does not.
	static SourceGeneratorDriver.AdditionalFile FileAt(string absolutePath, string xaml) =>
		new(SourceGeneratorDriver.ToAdditionalText(absolutePath, xaml), "Xaml", "MainPage.xaml", absolutePath,
			null, "net11.0", null, EnableIncrementalHotReload: true);

	static string? FindUC(GeneratorDriverRunResult r)
	{
		foreach (var g in r.Results)
			foreach (var s in g.GeneratedSources)
				if (s.HintName.Contains("uc.xsg", StringComparison.Ordinal))
					return s.SourceText.ToString();
		return null;
	}

	static (GeneratorDriverRunResult, GeneratorDriverRunResult) TwoRuns(Compilation comp, SourceGeneratorDriver.AdditionalFile v1, SourceGeneratorDriver.AdditionalFile v2)
		=> SourceGeneratorDriver.RunGeneratorWithChanges<XamlGenerator>(
			comp, applyChanges: (driver, c) => (driver.ReplaceAdditionalText(v1.Text, v2.Text), c), v1);

	[Fact]
	public void BlazorPatchDoesNotLeakIntoPlainProjectWithSameAssemblyName()
	{
		XamlHotReloadState.Reset();
		// Same assembly name for both "projects" — only the file path differs, exactly as two
		// template apps sharing the default "MauiApp.1" assembly name would on a shared VBCSCompiler.
		var comp = SourceGeneratorDriver.CreateMauiCompilation("MauiApp.1");

		// Project A (/AppA/MainPage.xaml): an edit adds a BlazorWebView, so its patch body creates
		// `new global::Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebView()`.
		var aV1 = FileAt("/AppA/MainPage.xaml", string.Format(PageWrap, ""));
		var aV2 = FileAt("/AppA/MainPage.xaml", string.Format(PageWrap, "<BlazorWebView />"));
		var (_, aRun2) = TwoRuns(comp, aV1, aV2);
		var ucA = FindUC(aRun2);
		Assert.NotNull(ucA);
		Assert.Contains("AspNetCore", ucA!, StringComparison.Ordinal); // sanity: A legitimately references it

		// Project B (/AppB/MainPage.xaml): a plain page edit, SAME assembly name, no Reset in between.
		var bV1 = FileAt("/AppB/MainPage.xaml", string.Format(PageWrap, ""));
		var bV2 = FileAt("/AppB/MainPage.xaml", string.Format(PageWrap, "<Label Text=\"World\" />"));
		var (_, bRun2) = TwoRuns(comp, bV1, bV2);
		var ucB = FindUC(bRun2);

		// B must not inherit A's Blazor patch — otherwise a plain app that never references
		// AspNetCore would emit `new ...BlazorWebView()` and fail with CS0234.
		Assert.False(ucB?.Contains("AspNetCore", StringComparison.Ordinal) ?? false,
			"Project B (plain page, different path) leaked Project A's BlazorWebView patch. B UC:\n" + ucB);
	}
}
