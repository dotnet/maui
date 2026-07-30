#nullable enable

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

[Collection("XamlHotReloadTests")]
public class StructuralHotReloadTests
{
	const string PageClass = "TestAiAssisted.MainPage";

	const string PageStub = """
		namespace TestAiAssisted;

		public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
		{
			private partial void InitializeComponent();

			public void InitializeComponentRuntime() { }

			public MainPage()
			{
				InitializeComponent();
			}
		}
		""";

	const string ConverterStub = """
		namespace TestAiAssisted;

		public sealed class TestConverter : global::Microsoft.Maui.Controls.IValueConverter
		{
			public object? Convert(object? value, global::System.Type targetType, object? parameter, global::System.Globalization.CultureInfo culture) => value;

			public object? ConvertBack(object? value, global::System.Type targetType, object? parameter, global::System.Globalization.CultureInfo culture) => value;
		}
		""";

	static XamlHotReloadTestHarness CreateHarness([CallerMemberName] string scenarioName = "") =>
		new(scenarioName, PageClass, PageStub);

	[Fact]
	public void RootComplexElementProperty_IsNotSilentlyDropped()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="Before" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <CollectionView>
			    <CollectionView.ItemTemplate>
			      <DataTemplate>
			        <Label Text="After" />
			      </DataTemplate>
			    </CollectionView.ItemTemplate>
			  </CollectionView>
			</ContentPage>
			""";

		// https://github.com/dotnet/maui/issues/36256 originally emitted an invalid root-content cast;
		// its replacement-root path also skipped the complex ItemTemplate property, so assert its generated shape.
		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.DoesNotContain("skipped (not yet supported)", updateComponentSource!, StringComparison.Ordinal);
		Assert.Contains("new global::Microsoft.Maui.Controls.DataTemplate", updateComponentSource!, StringComparison.Ordinal);
		Assert.Contains(".ItemTemplateProperty", updateComponentSource!, StringComparison.Ordinal);

		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Generated V2 UpdateComponent should compile.");
	}

	[MetadataUpdateFact]
	public void RootComplexElementProperty_AppliesToExistingPage()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="Before" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <CollectionView>
			    <CollectionView.ItemTemplate>
			      <DataTemplate>
			        <Label Text="After" />
			      </DataTemplate>
			    </CollectionView.ItemTemplate>
			  </CollectionView>
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			Assert.Equal("Before", Assert.IsType<Label>(page.Content).Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			var collectionView = Assert.IsType<CollectionView>(page.Content);
			Assert.NotNull(collectionView.ItemTemplate);
			Assert.Equal("After", Assert.IsType<Label>(collectionView.ItemTemplate!.CreateContent()).Text);
		});
	}

	[Fact]
	public void ComplexPropertyWithNestedResources_IsExplicitlySkipped()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="Before" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <CollectionView>
			    <CollectionView.ItemTemplate>
			      <DataTemplate>
			        <VerticalStackLayout>
			          <VerticalStackLayout.Resources>
			            <ResourceDictionary>
			              <Style x:Key="AfterStyle" TargetType="Label">
			                <Setter Property="Text" Value="After" />
			              </Style>
			            </ResourceDictionary>
			          </VerticalStackLayout.Resources>
			          <Label Style="{StaticResource AfterStyle}" />
			        </VerticalStackLayout>
			      </DataTemplate>
			    </CollectionView.ItemTemplate>
			  </CollectionView>
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains(
			"Complex property 'ItemTemplate' (ElementNode) — skipped (not yet supported)",
			updateComponentSource!,
			StringComparison.Ordinal);

		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Generated fallback UpdateComponent should compile.");
	}

	[Fact]
	public void ComplexPropertyWithAncestorStaticResource_IsExplicitlySkipped()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <Style x:Key="PageLabelStyle" TargetType="Label">
			      <Setter Property="Text" Value="After" />
			    </Style>
			  </ContentPage.Resources>
			  <Label Text="Before" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <Style x:Key="PageLabelStyle" TargetType="Label">
			      <Setter Property="Text" Value="After" />
			    </Style>
			  </ContentPage.Resources>
			  <CollectionView>
			    <CollectionView.ItemTemplate>
			      <DataTemplate>
			        <Label Style="{StaticResource PageLabelStyle}" />
			      </DataTemplate>
			    </CollectionView.ItemTemplate>
			  </CollectionView>
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains("new global::Microsoft.Maui.Controls.CollectionView", updateComponentSource!, StringComparison.Ordinal);
		Assert.Contains(
			"Complex property 'ItemTemplate' (ElementNode) — skipped (not yet supported)",
			updateComponentSource!,
			StringComparison.Ordinal);
		Assert.DoesNotContain(".ItemTemplateProperty", updateComponentSource!, StringComparison.Ordinal);

		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Root replacement with skipped ItemTemplate should compile.");
	}

	[Theory]
	[InlineData("""<Label Text="{Binding Converter={StaticResource TestResource}}" />""")]
	[InlineData("""
		<Label>
		  <Label.BindingContext>
		    <StaticResource Key="TestResource" />
		  </Label.BindingContext>
		</Label>
		""")]
	public void ComplexPropertyWithNestedStaticResourceShape_IsExplicitlySkipped(string templateContent)
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <local:TestConverter x:Key="TestResource" />
			  </ContentPage.Resources>
			  <Label Text="Before" />
			</ContentPage>
			""";
		var xamlV2 = $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <local:TestConverter x:Key="TestResource" />
			  </ContentPage.Resources>
			  <CollectionView>
			    <CollectionView.ItemTemplate>
			      <DataTemplate>
			        {{templateContent}}
			      </DataTemplate>
			    </CollectionView.ItemTemplate>
			  </CollectionView>
			</ContentPage>
			""";

		using var harness = new XamlHotReloadTestHarness(
			nameof(ComplexPropertyWithNestedStaticResourceShape_IsExplicitlySkipped),
			PageClass,
			PageStub,
			ConverterStub);
		var generation = harness.Generate(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains("new global::Microsoft.Maui.Controls.CollectionView", updateComponentSource!, StringComparison.Ordinal);
		Assert.Contains(
			"Complex property 'ItemTemplate' (ElementNode) — skipped (not yet supported)",
			updateComponentSource!,
			StringComparison.Ordinal);
		Assert.DoesNotContain(".ItemTemplateProperty", updateComponentSource!, StringComparison.Ordinal);

		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Root replacement with skipped ItemTemplate should compile.");
	}

	[Fact]
	public void StaticResourcePreflight_DiscardsParserDiagnostics()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <Style x:Key="PageLabelStyle" TargetType="Label" />
			  </ContentPage.Resources>
			  <Label Text="Before" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <ContentPage.Resources>
			    <Style x:Key="PageLabelStyle" TargetType="Label" />
			  </ContentPage.Resources>
			  <CollectionView>
			    <CollectionView.ItemTemplate>
			      <DataTemplate>
			        <VerticalStackLayout>
			          <Label Text="{Binding Converter={StaticResource Key=}}" />
			          <Label Style="{StaticResource PageLabelStyle}" />
			        </VerticalStackLayout>
			      </DataTemplate>
			    </CollectionView.ItemTemplate>
			  </CollectionView>
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.GenerateAllowingDiagnostics(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;

		// InitializeComponent reports the malformed markup once. The discarded preflight must
		// not add a duplicate before the later StaticResource causes ItemTemplate to be skipped.
		Assert.Single(generation[1].GeneratorResult.Diagnostics.Where(static diagnostic => diagnostic.Id == "MAUIG1001"));
		Assert.NotNull(updateComponentSource);
		Assert.Contains(
			"Complex property 'ItemTemplate' (ElementNode) — skipped (not yet supported)",
			updateComponentSource!,
			StringComparison.Ordinal);
		Assert.DoesNotContain(".ItemTemplateProperty", updateComponentSource!, StringComparison.Ordinal);

		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Skipped malformed ItemTemplate should compile.");
	}
}
