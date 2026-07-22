#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

[Collection("XamlHotReloadTests")]
public class HarnessCapabilityTests
{
	const string PageClass = "HarnessCapabilityApp.MainPage";

	const string PageStub = """
		namespace HarnessCapabilityApp;

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

	static XamlHotReloadTestHarness CreateHarness([CallerMemberName] string scenarioName = "") =>
		new(scenarioName, PageClass, PageStub);

	static XamlHotReloadTestHarness CreateHostedHarness(
		XamlHotReloadApplicationOptions options,
		[CallerMemberName] string scenarioName = "") =>
		new(scenarioName, PageClass, PageStub, options);

	[Fact]
	public void ApplicationHost_ResolvesAppResources_AndRestoresPreviousApplication()
	{
		const string xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="HarnessCapabilityApp.MainPage">
			  <Label Text="{DynamicResource AppKey}" />
			</ContentPage>
			""";
		var previousApplication = Application.Current;
		var options = new XamlHotReloadApplicationOptions(
			new Dictionary<string, object>
			{
				["AppKey"] = "FromApplication",
			});

		using (var harness = CreateHostedHarness(options))
		{
			Assert.NotSame(previousApplication, Application.Current);
			var generation = harness.Generate(xaml);

			harness.RunLive(generation, live =>
			{
				var page = live.GetInstance<ContentPage>();
				Assert.Equal("FromApplication", Assert.IsType<Label>(page.Content).Text);
			});
		}

		Assert.Same(previousApplication, Application.Current);
	}

	[MetadataUpdateFact]
	public void ApplicationHost_ThemeFlipIsSynchronous()
	{
		const string xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="HarnessCapabilityApp.MainPage">
			  <Label Text="{AppThemeBinding Light=LightBranch, Dark=DarkBranch}" />
			</ContentPage>
			""";
		var options = new XamlHotReloadApplicationOptions(InitialTheme: AppTheme.Light);

		using var harness = CreateHostedHarness(options);
		var generation = harness.Generate(xaml);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);
			Assert.Equal("LightBranch", label.Text);

			harness.ApplicationHost!.SetAppTheme(AppTheme.Dark);

			Assert.Equal("DarkBranch", label.Text);
		});
	}

	[Fact]
	public void MultiDocument_DictionaryOnlyEdit_TracksAllDocumentsAndCompilesPage()
	{
		const string page = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="HarnessCapabilityApp.MainPage">
			  <ContentPage.Resources>
			    <ResourceDictionary Source="Theme.xaml" />
			  </ContentPage.Resources>
			  <Label Text="Page" />
			</ContentPage>
			""";
		const string themeV1 = """
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			  <x:String x:Key="ThemeText">Before</x:String>
			</ResourceDictionary>
			""";
		const string themeV2 = """
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			  <x:String x:Key="ThemeText">After</x:String>
			</ResourceDictionary>
			""";

		using var harness = CreateHarness();
		var generation = harness.GenerateDocuments(
			CreateDocumentSnapshot(page, themeV1),
			CreateDocumentSnapshot(page, themeV2));

		Assert.Equal(themeV2, generation[1].Documents["Theme.xaml"].Text);
		Assert.Single(generation[1].GeneratedRoots);
		Assert.Equal(PageClass, generation[1].GeneratedRoots[0].TypeName);
		Assert.True(harness.Compile(generation[1]).PeImage.Length > 0);
	}

	[MetadataUpdateFact(Skip = "A ResourceDictionary loaded through Source= has no compiled resource payload in this in-memory generator/ALC harness, so the current runtime cannot reload the dictionary into an existing page without faking ResourceLoader behavior.")]
	public void MultiDocument_DictionaryOnlyEdit_RetainsPageAndLabelIdentity()
	{
		const string page = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="HarnessCapabilityApp.MainPage">
			  <ContentPage.Resources>
			    <ResourceDictionary Source="Theme.xaml" />
			  </ContentPage.Resources>
			  <Label Text="{StaticResource ThemeText}" />
			</ContentPage>
			""";
		const string themeV1 = """
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			  <x:String x:Key="ThemeText">Before</x:String>
			</ResourceDictionary>
			""";
		const string themeV2 = """
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			  <x:String x:Key="ThemeText">After</x:String>
			</ResourceDictionary>
			""";

		using var harness = CreateHarness();
		var generation = harness.GenerateDocuments(
			CreateDocumentSnapshot(page, themeV1),
			CreateDocumentSnapshot(page, themeV2));

		harness.RunLive(generation, live =>
		{
			var pageInstance = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(pageInstance.Content);

			Assert.Same(pageInstance, live.ApplyUpdate<ContentPage>(1));
			Assert.Same(label, pageInstance.Content);
			Assert.Equal("After", label.Text);
		});
	}

	[MetadataUpdateFact]
	public void MultipleInstances_RetainedRootsUpdateAndFreshRootStartsLatest()
	{
		const string xamlV1 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="HarnessCapabilityApp.MainPage">
			  <Label Text="Before" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="HarnessCapabilityApp.MainPage">
			  <Label Text="After" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);

		harness.RunLive(generation, live =>
		{
			var first = live.GetInstance<ContentPage>();
			var second = live.CreateInstance<ContentPage>();
			Assert.Equal("Before", Assert.IsType<Label>(first.Content).Text);
			Assert.Equal("Before", Assert.IsType<Label>(second.Content).Text);

			Assert.Same(first, live.ApplyUpdate<ContentPage>(1));
			Assert.Equal("After", Assert.IsType<Label>(first.Content).Text);
			Assert.Equal("After", Assert.IsType<Label>(second.Content).Text);

			var third = live.CreateInstance<ContentPage>();
			Assert.Equal("After", Assert.IsType<Label>(third.Content).Text);
		});
	}

	static IReadOnlyDictionary<string, XamlHotReloadDocument> CreateDocumentSnapshot(
		string page,
		string theme) =>
		new Dictionary<string, XamlHotReloadDocument>(StringComparer.Ordinal)
		{
			["MainPage.xaml"] = new XamlHotReloadDocument(page),
			["Theme.xaml"] = new XamlHotReloadDocument(theme),
		};
}
