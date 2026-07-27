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
}
