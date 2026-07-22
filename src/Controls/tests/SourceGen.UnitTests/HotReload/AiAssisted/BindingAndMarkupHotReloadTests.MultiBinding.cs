// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

public partial class BindingAndMarkupHotReloadTests
{
	// Wave2 · Binding & Markup · P0-02 · BM-03
	// Provenance: MAUI §3.4 | portfolio P0-02
	// Faithfulness: reaches writer L1194 for MultiBinding's ElementNode mutation; fails-for-bug: the documented unsupported complex-property path changes without a paired live probe.
	// Expected: DOC-SKIP-GUARD
	// Issue: https://github.com/dotnet/maui/issues/36732
	[Fact]
	public void MultiBinding_ComplexProperty_EmitsSkipMarker()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <local:JoinConverter x:Key="JoinConverter" />
			  </ContentPage.Resources>
			  <Label Text="Before" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <local:JoinConverter x:Key="JoinConverter" />
			  </ContentPage.Resources>
			  <Label>
			    <Label.Text>
			      <MultiBinding Converter="{StaticResource JoinConverter}">
			        <Binding Path="A" />
			        <Binding Path="B" />
			      </MultiBinding>
			    </Label.Text>
			  </Label>
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains(
			"Complex property 'Text' (ElementNode) — skipped (not yet supported)",
			updateComponentSource!,
			StringComparison.Ordinal);
	}

	// Wave2 · Binding & Markup · P0-02 · BM-05
	// Provenance: MAUI §3.4 | portfolio P0-02
	// Faithfulness: reaches MultiBinding mutation and inspects the live BindingBase and child proxy state; fails-for-bug: remove/re-add appends duplicate child expressions or subscriptions.
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[Theory(Skip = "Blocked by writer MultiBinding reconciliation gap — see #36732; green anchor: MultiBinding_ComplexProperty_EmitsSkipMarker")]
	[InlineData(false)]
	[InlineData(true)]
	public void MultiBindingChildren_RemoveReAdd_NoDuplicateExpressions(bool useXDataType)
	{
		var firstBinding = useXDataType
			? """<Binding x:DataType="local:Row" Path="A" />"""
			: """<Binding Path="A" />""";
		var secondBinding = useXDataType
			? """<Binding x:DataType="local:Row" Path="B" />"""
			: """<Binding Path="B" />""";

		string CreateXaml(string bindings) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <local:JoinConverter x:Key="JoinConverter" />
			  </ContentPage.Resources>
			  <ContentPage.BindingContext>
			    <local:Row A="A" B="B" />
			  </ContentPage.BindingContext>
			  <Label>
			    <Label.Text>
			      <MultiBinding Converter="{StaticResource JoinConverter}">
			        {{bindings}}
			      </MultiBinding>
			    </Label.Text>
			  </Label>
			</ContentPage>
			""";

		var xamlV1 = CreateXaml($"{firstBinding}{Environment.NewLine}{secondBinding}");
		var xamlV2 = CreateXaml(firstBinding);
		var xamlV3 = CreateXaml($"{firstBinding}{Environment.NewLine}{secondBinding}");

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);
			Assert.Equal("2:A|B", label.Text);

			live.ApplyUpdate<ContentPage>(1);
			Assert.Equal("1:A", label.Text);

			live.ApplyUpdate<ContentPage>(2);
			Assert.Equal("2:A|B", label.Text);

			var binding = Assert.IsType<MultiBinding>(GetTextRegistration(label).Binding);
			Assert.Equal(2, binding.Bindings.Count);

			var converter = page.Resources["JoinConverter"]!;
			var countBeforeChange = (int)converter.GetType().GetProperty("ConvertCount")!.GetValue(converter)!;
			var row = page.BindingContext!;
			row.GetType().GetProperty("B")!.SetValue(row, "C");

			Assert.Equal("2:A|C", label.Text);
			var countAfterChange = (int)converter.GetType().GetProperty("ConvertCount")!.GetValue(converter)!;
			Assert.Equal(1, countAfterChange - countBeforeChange);
			Assert.Equal(2, binding.Bindings.Count);
		});
	}
}
