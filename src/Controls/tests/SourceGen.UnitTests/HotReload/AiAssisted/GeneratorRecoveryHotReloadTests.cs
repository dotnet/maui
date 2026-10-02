#nullable enable

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

/// <summary>
/// XIHR-02 generator-boundary diagnostic recomputation probe. Each test in this class drives the
/// incremental generator directly through <see cref="XamlHotReloadTestHarness"/>
/// (<c>GeneratorDriver</c> -&gt; diagnostics/UpdateComponent source -&gt; Roslyn compile). It does
/// <b>not</b> exercise the IDE, <c>dotnet watch</c>, Roslyn's real edit classifier, MUH dispatch, a
/// device, native handlers, or rendered output.
/// </summary>
[Collection("XamlHotReloadTests")]
public class GeneratorRecoveryHotReloadTests
{
	const string PageClass = "TestAiAssisted.MainPage";

	const string PageStub = """
		namespace TestAiAssisted;

		public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
		{
			private partial void InitializeComponent();

			public bool IsActive { get; } = true;

			public MainPage()
			{
				InitializeComponent();
			}
		}
		""";

	static XamlHotReloadTestHarness CreateHarness([CallerMemberName] string scenarioName = "") =>
		new(scenarioName, PageClass, PageStub);

	[Fact]
	public void MalformedExpression_ThenRepair_RecomputesGeneratorDiagnostics()
	{
		// Issue: https://github.com/dotnet/maui/issues/36157
		// This generator-boundary probe drives valid -> malformed -> repaired GeneratorDriver
		// runs and verifies that each run recomputes its own diagnostics and output. It does not
		// exercise or make claims about IDE Error List or dotnet-watch recovery.
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			    <VerticalStackLayout>
			        <Label Text="{IsActive ? 'Status: Active' : 'Status: Idle'}" />
			        <Label Text="Before" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			    <VerticalStackLayout>
			        <Label Text="{IsActive ? 'Status: Active' : 'Status: Idle'" />
			        <Label Text="Before" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV3 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			    <VerticalStackLayout>
			        <Label Text="{IsActive ? 'Status: Active' : 'Status: Idle'}" />
			        <Label Text="AfterRepair" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.GenerateAllowingDiagnostics(xamlV1, xamlV2, xamlV3);

		// V2 (malformed): exactly one MAUIG1003; the property change is gracefully skipped
		// (not silently mis-applied) rather than the UpdateComponent source being suppressed.
		Assert.Single(generation[1].GeneratorResult.Diagnostics.Where(static d => d.Id == "MAUIG1003"));
		var malformedUpdateComponentSource = generation[1].UpdateComponentSource;
		Assert.NotNull(malformedUpdateComponentSource);
		Assert.Contains("skipped", malformedUpdateComponentSource!, StringComparison.Ordinal);
		Assert.DoesNotContain("AfterRepair", malformedUpdateComponentSource!, StringComparison.Ordinal);

		// V3 (repaired): the generator recomputes a clean diagnostic set and emits repaired text.
		Assert.DoesNotContain(generation[2].GeneratorResult.Diagnostics, static d => d.Id == "MAUIG1003");
		var updateComponentSource = generation[2].UpdateComponentSource;
		Assert.NotNull(updateComponentSource);
		Assert.Contains("AfterRepair", updateComponentSource!, StringComparison.Ordinal);

		// V3 generated code must compile cleanly.
		var compiled = harness.Compile(generation[2]);
		Assert.True(compiled.PeImage.Length > 0, "Repaired version should compile to a non-empty assembly");
	}
}
