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

[Collection("XamlHotReloadTests")]
public class BindingAndMarkupHotReloadTests
{
	const string PageClass = "TestAiAssisted.MainPage";

	const string PageStub = """
		namespace TestAiAssisted;

		public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
		{
			private partial void InitializeComponent();

			public MainPage()
			{
				InitializeComponent();
			}
		}
		""";

	const string BindingAndMarkupSource = """
		using System;
		using System.ComponentModel;
		using System.Globalization;
		using Microsoft.Maui.Controls;
		using Microsoft.Maui.Controls.Xaml;

		namespace TestAiAssisted;

		public sealed class TestViewModel
		{
			public string Text { get; set; } = "VM";
		}

		public sealed class Row : INotifyPropertyChanged
		{
			string _a = string.Empty;
			string _b = string.Empty;

			public string A
			{
				get => _a;
				set
				{
					if (_a == value)
						return;
					_a = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(A)));
				}
			}

			public string B
			{
				get => _b;
				set
				{
					if (_b == value)
						return;
					_b = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(B)));
				}
			}

			public event PropertyChangedEventHandler? PropertyChanged;
		}

		public sealed class JoinConverter : IMultiValueConverter
		{
			public int ConvertCount { get; private set; }

			public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
			{
				ConvertCount++;
				var result = $"{values.Length}:";
				for (var index = 0; index < values.Length; index++)
				{
					if (index > 0)
						result += "|";
					result += values[index]?.ToString() ?? "<null>";
				}
				return result;
			}

			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
				new object[targetTypes.Length];
		}

		public sealed class ShoutExtension : IMarkupExtension<string>
		{
			public string Text { get; set; } = string.Empty;

			public string ProvideValue(IServiceProvider serviceProvider) => Text.ToUpperInvariant();

			object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
		}
		""";

	static XamlHotReloadTestHarness CreateHarness([CallerMemberName] string scenarioName = "") =>
		new(scenarioName, PageClass, PageStub, BindingAndMarkupSource);

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

	// Wave2 · Binding & Markup · family-7 · BM-02
	// Provenance: MAUI §3.4 | family 7 custom markup
	// Faithfulness: reaches the IC ProvideValue pipeline for a same-compilation IMarkupExtension<string>; fails-for-bug: custom markup edits are not re-provided on the retained element.
	// Expected: GREEN
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact]
	public void CustomMarkupExtension_EditAndReverse_ReprovidesValue()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="{local:Shout Text=hi}" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="{local:Shout Text=bye}" />
			</ContentPage>
			""";
		const string xamlV3 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="{local:Shout Text=hi}" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);
			Assert.Equal("HI", label.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Same(label, page.Content);
			Assert.Equal("BYE", label.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
			Assert.Same(label, page.Content);
			Assert.Equal("HI", label.Text);
		});
	}

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

	// Wave2 · Binding & Markup · P0-01 · BM-04
	// Provenance: MAUI §3.4 | portfolio P0-01
	// Faithfulness: reaches DynamicResource-to-Binding teardown in the retained Label; fails-for-bug: a dormant DynamicResource registration still reacts after Binding wins.
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "Blocked by writer binding teardown gap — see #36732; green anchor: MultiBinding_ComplexProperty_EmitsSkipMarker")]
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

	static TextRegistration GetTextRegistration(Label label)
	{
		var propertiesField = typeof(BindableObject).GetField("_properties", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.NotNull(propertiesField);

		var properties = Assert.IsAssignableFrom<IDictionary>(propertiesField!.GetValue(label));
		foreach (DictionaryEntry entry in properties)
		{
			var context = entry.Value;
			if (context is null)
				continue;

			var contextType = context.GetType();
			var property = contextType.GetField("Property")?.GetValue(context);
			if (!ReferenceEquals(property, Label.TextProperty))
				continue;

			var attributes = contextType.GetField("Attributes")?.GetValue(context)?.ToString() ?? string.Empty;
			var bindings = contextType.GetField("Bindings")?.GetValue(context);
			Assert.NotNull(bindings);

			var bindingCount = (int)bindings!.GetType().GetProperty("Count")!.GetValue(bindings)!;
			var binding = bindings.GetType().GetMethod("GetValue")!.Invoke(bindings, null) as BindingBase;
			return new TextRegistration(binding, bindingCount, attributes.Contains("IsDynamicResource", StringComparison.Ordinal));
		}

		throw new Xunit.Sdk.XunitException("The Label.Text property had no bindable-property context.");
	}

	readonly record struct TextRegistration(BindingBase? Binding, int BindingCount, bool IsDynamicResource);
}
