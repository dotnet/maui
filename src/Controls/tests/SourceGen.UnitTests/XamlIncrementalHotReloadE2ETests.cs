#nullable enable

using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// True end-to-end tests: XAML -> SourceGen -> Compile -> Load -> Hot Reload -> Verify.
/// Uses <see cref="MetadataUpdater.ApplyUpdate"/> to apply deltas to a live assembly.
/// </summary>
[Collection("XamlHotReloadTests")]
public class XamlIncrementalHotReloadE2ETests
{
	const string PageClass = "TestE2EApp.MainPage";

	const string PageStub = """
		namespace TestE2EApp;

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

	[MetadataUpdateFact]
	public void PropertyChange_AppliedViaHotReload()
	{
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			    </VerticalStackLayout>
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="World" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		Assert.NotNull(generation[1].UpdateComponentSource);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var layout = page.Content as Layout;
			Assert.NotNull(layout);
			var label = layout!.Children.OfType<Label>().FirstOrDefault();
			Assert.NotNull(label);
			Assert.Equal("Hello", label!.Text);

			var updatedPage = live.ApplyUpdate<ContentPage>(1);
			Assert.Same(page, updatedPage);
			var updatedLabel = ((Layout)page.Content!).Children.OfType<Label>().FirstOrDefault();
			Assert.NotNull(updatedLabel);
			Assert.Same(label, updatedLabel);
			Assert.Equal("World", updatedLabel!.Text);
		});
	}

	[MetadataUpdateFact]
	public void MultiplePropertyChanges_ChainedPatches()
	{
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage"
			             Title="V1">
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage"
			             Title="V2">
			    <Label Text="World" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;
		Assert.NotNull(updateComponentSource);
		Assert.Contains("__version == 0", updateComponentSource!, StringComparison.Ordinal);
		Assert.Contains("__version = 1", updateComponentSource!, StringComparison.Ordinal);
		Assert.Contains("\"World\"", updateComponentSource!, StringComparison.Ordinal);
		Assert.Contains("\"V2\"", updateComponentSource!, StringComparison.Ordinal);
	}

	[MetadataUpdateFact]
	public void SuccessiveUpdates_AppliedToSameLiveInstance()
	{
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="First" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="Second" />
			</ContentPage>
			""";
		const string xamlV3 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="Third" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);
		Assert.Contains("__version == 0", generation[2].UpdateComponentSource!, StringComparison.Ordinal);
		Assert.Contains("__version == 1", generation[2].UpdateComponentSource!, StringComparison.Ordinal);
		Assert.Contains("__version = 2", generation[2].UpdateComponentSource!, StringComparison.Ordinal);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);
			Assert.Equal("First", label.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Same(label, page.Content);
			Assert.Equal("Second", label.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
			Assert.Same(label, page.Content);
			Assert.Equal("Third", label.Text);
		});
	}

	[Fact]
	public void IdenticalXaml_NoUCGenerated()
	{
		const string xaml = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="Hello" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xaml, xaml);
		Assert.Null(generation[1].UpdateComponentSource);
	}

	[Fact]
	public void GeneratedIC_CompilesCleanly()
	{
		const string xaml = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="Hello" />
			        <Button Text="Click me" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xaml);
		var compilation = harness.Compile(generation[0]);
		Assert.True(compilation.PeImage.Length > 0, "Compiled assembly should not be empty");
	}

	[Fact]
	public void GeneratedUC_CompilesCleanly()
	{
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="World" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		Assert.NotNull(generation[1].UpdateComponentSource);
		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Compiled assembly should not be empty");
	}

	[Fact]
	public void AdditionalCSharpSource_CompilesWithGeneratedXaml()
	{
		const string customControlSource = """
			namespace TestE2EApp;

			public class CustomLabel : global::Microsoft.Maui.Controls.Label
			{
			}
			""";
		const string xaml = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestE2EApp"
			             x:Class="TestE2EApp.MainPage">
			    <local:CustomLabel Text="Hello" />
			</ContentPage>
			""";

		using var harness = new XamlHotReloadTestHarness(
			nameof(AdditionalCSharpSource_CompilesWithGeneratedXaml),
			PageClass,
			PageStub,
			customControlSource);
		var generation = harness.Generate(xaml);
		var compilation = harness.Compile(generation[0]);
		Assert.True(compilation.PeImage.Length > 0, "Compiled assembly should not be empty");
	}

	[Fact]
	public void RootContentReplaced_CompilesCleanly()
	{
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <CollectionView />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		var updateComponentSource = generation[1].UpdateComponentSource;
		Assert.NotNull(updateComponentSource);
		Assert.DoesNotContain(
			".Content = (global::Microsoft.Maui.IView)",
			updateComponentSource!,
			StringComparison.Ordinal);
		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Compiled assembly should not be empty");
	}

	[Fact]
	public void ResourceAdded_CompilesCleanly()
	{
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <Color x:Key="SecondaryColor">Red</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		Assert.NotNull(generation[1].UpdateComponentSource);
		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Compiled assembly should not be empty");
	}

	[Fact]
	public void ResourceRemoved_CompilesCleanly()
	{
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <Color x:Key="SecondaryColor">Red</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		Assert.NotNull(generation[1].UpdateComponentSource);
		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Compiled assembly should not be empty");
	}

	[MetadataUpdateFact]
	public void ResourceAdded_AppliedViaHotReload()
	{
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <Color x:Key="SecondaryColor">Red</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		Assert.NotNull(generation[1].UpdateComponentSource);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			Assert.True(page.Resources.ContainsKey("AccentColor"));
			Assert.False(page.Resources.ContainsKey("SecondaryColor"));

			var updatedPage = live.ApplyUpdate<ContentPage>(1);
			Assert.Same(page, updatedPage);
			Assert.True(page.Resources.ContainsKey("AccentColor"), "AccentColor should still exist");
			Assert.True(page.Resources.ContainsKey("SecondaryColor"), "SecondaryColor should be added");
			Assert.Equal(Microsoft.Maui.Graphics.Colors.Red, page.Resources["SecondaryColor"]);
		});
	}

	[MetadataUpdateFact]
	public void ResourceRemoved_AppliedViaHotReload()
	{
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			        <Color x:Key="SecondaryColor">Red</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		Assert.NotNull(generation[1].UpdateComponentSource);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			Assert.True(page.Resources.ContainsKey("AccentColor"));
			Assert.True(page.Resources.ContainsKey("SecondaryColor"));

			var updatedPage = live.ApplyUpdate<ContentPage>(1);
			Assert.Same(page, updatedPage);
			Assert.True(page.Resources.ContainsKey("AccentColor"), "AccentColor should still exist");
			Assert.False(page.Resources.ContainsKey("SecondaryColor"), "SecondaryColor should be removed");
		});
	}

	[MetadataUpdateFact]
	public void ResourceValueChanged_AppliedViaHotReload()
	{
		const string xamlV1 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">DarkBlue</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";
		const string xamlV2 = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <ContentPage.Resources>
			        <Color x:Key="AccentColor">Red</Color>
			    </ContentPage.Resources>
			    <Label Text="Hello" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		Assert.NotNull(generation[1].UpdateComponentSource);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			Assert.Equal(Microsoft.Maui.Graphics.Colors.DarkBlue, page.Resources["AccentColor"]);

			var updatedPage = live.ApplyUpdate<ContentPage>(1);
			Assert.Same(page, updatedPage);
			Assert.Equal(Microsoft.Maui.Graphics.Colors.Red, page.Resources["AccentColor"]);
		});
	}
}
