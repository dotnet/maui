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
}
