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
	// Wave2 · Binding & Markup · P0-01 · BM-01
	// Provenance: MAUI §3.4 | portfolio P0-01
	// Faithfulness: reaches writer L1548 for DynamicResource and Binding markup nodes; fails-for-bug: markup swap does not replace the prior value source.
	// Expected: GREEN
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact]
	public void DynamicResourceToBinding_SwapAndReverse_UpdatesVisibleValue()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <ResourceDictionary>
			      <x:String x:Key="K1">Resource-V1</x:String>
			    </ResourceDictionary>
			  </ContentPage.Resources>
			  <ContentPage.BindingContext>
			    <local:TestViewModel Text="VM" />
			  </ContentPage.BindingContext>
			  <Label Text="{DynamicResource K1}" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <ResourceDictionary>
			      <x:String x:Key="K1">Resource-V1</x:String>
			    </ResourceDictionary>
			  </ContentPage.Resources>
			  <ContentPage.BindingContext>
			    <local:TestViewModel Text="VM" />
			  </ContentPage.BindingContext>
			  <Label Text="{Binding Text}" />
			</ContentPage>
			""";
		const string xamlV3 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <ResourceDictionary>
			      <x:String x:Key="K1">Resource-V3</x:String>
			    </ResourceDictionary>
			  </ContentPage.Resources>
			  <ContentPage.BindingContext>
			    <local:TestViewModel Text="VM" />
			  </ContentPage.BindingContext>
			  <Label Text="{Binding Text}" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);
			Assert.Equal("Resource-V1", label.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Same(label, page.Content);
			Assert.Equal("VM", label.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
			Assert.Same(label, page.Content);
			Assert.Equal("VM", label.Text);
		});
	}

	// Wave2 · Binding & Markup · P0-01 · BM-04
	// Provenance: MAUI §3.4 | portfolio P0-01
	// Faithfulness: reaches DynamicResource-to-Binding teardown in the retained Label; fails-for-bug: a dormant DynamicResource registration still reacts after Binding wins.
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "Blocked by writer binding teardown gap — see #36732; green anchor: DynamicResourceToBinding_SwapAndReverse_UpdatesVisibleValue")]
	public void DynamicResourceToBinding_RemovesDormantRegistration()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <x:String x:Key="K1">Resource-V1</x:String>
			  </ContentPage.Resources>
			  <ContentPage.BindingContext>
			    <local:TestViewModel Text="VM" />
			  </ContentPage.BindingContext>
			  <Label Text="{DynamicResource K1}" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <x:String x:Key="K1">Resource-V2</x:String>
			  </ContentPage.Resources>
			  <ContentPage.BindingContext>
			    <local:TestViewModel Text="VM" />
			  </ContentPage.BindingContext>
			  <Label Text="{Binding Text}" />
			</ContentPage>
			""";
		const string xamlV3 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <x:String x:Key="K1">Resource-V3</x:String>
			  </ContentPage.Resources>
			  <ContentPage.BindingContext>
			    <local:TestViewModel Text="VM" />
			  </ContentPage.BindingContext>
			  <Label Text="{Binding Text}" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);
			Assert.Equal("Resource-V1", label.Text);

			live.ApplyUpdate<ContentPage>(1);
			Assert.Equal("VM", label.Text);
			live.ApplyUpdate<ContentPage>(2);

			var registration = GetTextRegistration(label);
			Assert.IsType<Binding>(registration.Binding);
			Assert.Equal(1, registration.BindingCount);
			Assert.False(registration.IsDynamicResource);

			var viewModel = page.BindingContext!;
			viewModel.GetType().GetProperty("Text")!.SetValue(viewModel, "VM-Changed");
			Assert.Equal("VM-Changed", label.Text);

			page.Resources["K1"] = "Resource-after-swap";
			Assert.Equal("VM-Changed", label.Text);
		});
	}
}
