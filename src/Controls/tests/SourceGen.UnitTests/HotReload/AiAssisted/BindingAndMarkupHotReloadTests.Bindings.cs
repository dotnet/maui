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
	[MetadataUpdateFact]
	public void XReferenceBinding_StringFormatEdit_RetainsSourceAcrossUpdates()
	{
		const string pageStub = """
			namespace TestAiAssisted;

			public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
			{
				internal global::Microsoft.Maui.Controls.Label CaptionLabel = null!;

				private partial void InitializeComponent();

				public MainPage()
				{
					InitializeComponent();
				}
			}
			""";
		const string xamlV0 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <VerticalStackLayout>
			    <Label x:Name="CaptionLabel" Text="Instance caption V0" />
			    <Label Text="{Binding Source={x:Reference CaptionLabel}, Path=Text, StringFormat='echo {0}'}" />
			  </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <VerticalStackLayout>
			    <Label x:Name="CaptionLabel" Text="Instance caption V1" />
			    <Label Text="{Binding Source={x:Reference CaptionLabel}, Path=Text, StringFormat='echo {0}'}" />
			  </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <VerticalStackLayout>
			    <Label x:Name="CaptionLabel" Text="Instance caption V1" />
			    <Label Text="{Binding Source={x:Reference CaptionLabel}, Path=Text, StringFormat='mirror {0}'}" />
			  </VerticalStackLayout>
			</ContentPage>
			""";

		using var harness = new XamlHotReloadTestHarness(
			nameof(XReferenceBinding_StringFormatEdit_RetainsSourceAcrossUpdates),
			PageClass,
			pageStub,
			BindingAndMarkupSource);
		var generation = harness.Generate(xamlV0, xamlV1, xamlV2);

		harness.RunLive(generation, live =>
		{
			var firstPage = live.GetInstance<ContentPage>();
			var secondPage = live.CreateInstance<ContentPage>();

			AssertPage(firstPage, "Instance caption V0", "echo Instance caption V0");
			AssertPage(secondPage, "Instance caption V0", "echo Instance caption V0");

			Assert.Same(firstPage, live.ApplyUpdate<ContentPage>(1));
			AssertPage(firstPage, "Instance caption V1", "echo Instance caption V1");
			AssertPage(secondPage, "Instance caption V1", "echo Instance caption V1");

			Assert.Same(firstPage, live.ApplyUpdate<ContentPage>(2));
			AssertPage(firstPage, "Instance caption V1", "mirror Instance caption V1");
			AssertPage(secondPage, "Instance caption V1", "mirror Instance caption V1");

			SetCaption(firstPage, "Instance caption V2", "mirror Instance caption V2");
			SetCaption(secondPage, "Instance caption V2", "mirror Instance caption V2");
		});

		static void AssertPage(ContentPage page, string expectedCaption, string expectedFormattedText)
		{
			var layout = Assert.IsType<VerticalStackLayout>(page.Content);
			var caption = Assert.IsType<Label>(layout.Children[0]);
			var formatted = Assert.IsType<Label>(layout.Children[1]);

			Assert.Equal(expectedCaption, caption.Text);
			Assert.Equal(expectedFormattedText, formatted.Text);

			var registration = GetTextRegistration(formatted);
			var binding = Assert.IsAssignableFrom<BindingBase>(registration.Binding);
			var source = binding.GetType().GetProperty("Source")!.GetValue(binding);
			Assert.Same(caption, source);
		}

		static void SetCaption(ContentPage page, string captionText, string expectedFormattedText)
		{
			var layout = Assert.IsType<VerticalStackLayout>(page.Content);
			var caption = Assert.IsType<Label>(layout.Children[0]);
			var formatted = Assert.IsType<Label>(layout.Children[1]);

			caption.Text = captionText;
			Assert.Equal(expectedFormattedText, formatted.Text);
		}
	}

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
