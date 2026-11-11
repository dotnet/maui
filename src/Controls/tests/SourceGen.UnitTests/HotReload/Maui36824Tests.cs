#nullable enable

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

[Collection("XamlHotReloadTests")]
public class Maui36824Tests
{
	const string PageClass = "Maui36824.MainPage";

	static string PageSource(int codeVersion) => $$"""
		namespace Maui36824;

		public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
		{
			private partial void InitializeComponent();

			public MainPage()
			{
				InitializeComponent();
			}

			public int GetCodeVersion() => {{codeVersion}};
		}
		""";

	static string PageXaml(string text, string content = """<Label Text="{0}" />""") => $$"""
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="Maui36824.MainPage">
		    {{string.Format(content, text)}}
		</ContentPage>
		""";

	static XamlHotReloadTestHarness CreateHarness(
		string pageSource,
		[CallerMemberName] string scenarioName = "") =>
		new(scenarioName, PageClass, pageSource);

	[MetadataUpdateFact]
	public void XamlThenCSharpThenXaml_AppliesEveryDeltaWithoutENC1002()
	{
		var xamlV1 = PageXaml("First");
		var xamlV2 = PageXaml("Second");
		var xamlV3 = PageXaml("Third");
		var pageSourceV1 = PageSource(1);
		var pageSourceV2 = PageSource(2);

		using var harness = CreateHarness(pageSourceV1);
		var generation = harness.GenerateWithPageSources(
			new(Xaml: xamlV1, PageSource: pageSourceV1),
			new(Xaml: xamlV2, PageSource: pageSourceV1),
			new(Xaml: xamlV2, PageSource: pageSourceV2),
			new(Xaml: xamlV3, PageSource: pageSourceV2));

		Assert.All(generation.Versions, version => Assert.NotNull(version.UpdateComponentSource));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			Assert.Equal("First", Assert.IsType<Label>(page.Content).Text);
			Assert.Equal(1, InvokeCodeVersion(page));

			live.ApplyUpdate<ContentPage>(1);
			Assert.Equal("Second", Assert.IsType<Label>(page.Content).Text);
			Assert.Equal(1, InvokeCodeVersion(page));

			live.ApplyUpdate<ContentPage>(2, "GetCodeVersion");
			Assert.Equal("Second", Assert.IsType<Label>(page.Content).Text);
			Assert.Equal(2, InvokeCodeVersion(page));

			live.ApplyUpdate<ContentPage>(3);
			Assert.Equal("Third", Assert.IsType<Label>(page.Content).Text);
			Assert.Equal(2, InvokeCodeVersion(page));
		});
	}

	[Fact]
	public void UpdateComponent_IsPresentFromBaselineThroughNoOpAndStructuralGenerations()
	{
		const string stack = """
			<VerticalStackLayout>
			    <Label Text="{0}" />
			    <Button Text="Added" />
			</VerticalStackLayout>
			""";
		var pageSource = PageSource(1);

		using var harness = CreateHarness(pageSource);
		var generation = harness.Generate(
			PageXaml("First"),
			PageXaml("Second"),
			PageXaml("Second"),
			PageXaml("Third", stack));

		Assert.All(generation.Versions, version =>
		{
			Assert.NotNull(version.UpdateComponentSource);
			Assert.Contains(
				"internal void UpdateComponent()",
				version.UpdateComponentSource!,
				StringComparison.Ordinal);
		});
	}

	[Fact]
	public void InvalidIntermediateXaml_RecoveryRestoresStableUpdateComponentIdentity()
	{
		const string invalidXaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="Maui36824.MainPage">
			    <Label Text="Broken">
			</ContentPage>
			""";
		var pageSource = PageSource(1);

		using var harness = CreateHarness(pageSource);
		var generation = harness.GenerateAllowingDiagnostics(
			PageXaml("Stable"),
			invalidXaml,
			PageXaml("Stable"));

		Assert.NotNull(generation[0].UpdateComponentSource);
		Assert.Contains(
			generation[1].GeneratorResult.Diagnostics,
			diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
		Assert.NotNull(generation[2].UpdateComponentSource);
		Assert.Contains(
			"internal void UpdateComponent()",
			generation[2].UpdateComponentSource!,
			StringComparison.Ordinal);
		Assert.Equal(generation[0].UpdateComponentSource, generation[2].UpdateComponentSource);
		Assert.True(harness.Compile(generation[2]).PeImage.Length > 0);
	}

	[Fact]
	public void SeparateCompilations_ProduceIdenticalBaselineUpdateComponent()
	{
		var xaml = PageXaml("Stable");
		var pageSource = PageSource(1);
		string firstSource;

		using (var firstHarness = CreateHarness(pageSource))
			firstSource = firstHarness.Generate(xaml)[0].UpdateComponentSource!;

		using var secondHarness = CreateHarness(pageSource);
		var secondSource = secondHarness.Generate(xaml)[0].UpdateComponentSource;

		Assert.Equal(firstSource, secondSource);
	}

	static int InvokeCodeVersion(ContentPage page)
	{
		var method = page.GetType().GetMethod(
			"GetCodeVersion",
			BindingFlags.Instance | BindingFlags.Public);
		Assert.NotNull(method);
		return Assert.IsType<int>(method!.Invoke(page, null));
	}
}
