#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
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
		Assert.DoesNotContain("if (__version ==", updateComponentSource!, StringComparison.Ordinal);
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
		Assert.All(generation.Versions, version => Assert.NotNull(version.UpdateComponentSource));
		Assert.DoesNotContain("if (__version ==", generation[2].UpdateComponentSource!, StringComparison.Ordinal);

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

	/// <summary>
	/// Regression for the XIHR versioning determinism bug (Tomas Matousek). Editing a property to an
	/// INVALID value and then reverting it must leave the generator in a state where the generated
	/// output for the (now identical to baseline) XAML compiles cleanly and does not retain the
	/// invalid intermediate value. Previously, accumulated versioned patches retained every edit, so
	/// the invalid "Level22" block lingered in UpdateComponent() and the output failed to compile.
	/// </summary>
	[Fact]
	public void RevertToOriginal_ProducesCompilableOutput_WithoutStalePatch()
	{
		string Page(string headingLevel) => $$"""
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="Hi" SemanticProperties.HeadingLevel="{{headingLevel}}" />
			</ContentPage>
			""";

		using var harness = CreateHarness();
		var generation = harness.Generate(Page("Level2"), Page("Level22"), Page("Level2"));
		var reverted = generation[2];

		Assert.DoesNotContain("Level22", reverted.InitializeComponentSource!, StringComparison.Ordinal);
		Assert.NotNull(reverted.UpdateComponentSource);
		Assert.DoesNotContain("Level22", reverted.UpdateComponentSource!, StringComparison.Ordinal);

		var compilation = harness.Compile(reverted);
		Assert.True(compilation.PeImage.Length > 0, "Compiled assembly should not be empty");
	}

	/// <summary>
	/// Reverse-transition (Kirill's revert requirement): editing a property forward then reverting must
	/// produce a <c>UpdateComponent()</c> that restores the baseline value on live instances, without
	/// retaining the intermediate value. (Determinism of the generated output is covered separately and
	/// exhaustively by <see cref="RevertedGeneration_InitializeComponent_IsByteIdentical_ToInitialGeneration"/>.)
	/// </summary>
	[Fact]
	public void ReverseEdit_UC_RestoresBaselineValue_WithoutRetainingIntermediate()
	{
		string Page(string text) => $$"""
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="{{text}}" />
			</ContentPage>
			""";

		var v1 = Page("Hello");
		var v2 = Page("World");

		// V1 -> V2 -> V1 (revert).
		using var harness = CreateHarness();
		var generation = harness.Generate(v1, v2, v1);
		var ucV2 = generation[1].UpdateComponentSource;
		var ucV3 = generation[2].UpdateComponentSource;

		// Forward edit's UC applies the new value (its non-empty body is also the XAML-change signal)...
		Assert.NotNull(ucV2);
		Assert.Contains("\"World\"", ucV2!, StringComparison.Ordinal);

		// ...and the reverse edit's UC brings it back to the V1 value (reverse transition), without
		// retaining the intermediate "World".
		Assert.NotNull(ucV3);
		Assert.Contains("\"Hello\"", ucV3!, StringComparison.Ordinal);
		Assert.DoesNotContain("\"World\"", ucV3!, StringComparison.Ordinal);
	}

	/// <summary>
	/// The strongest determinism guarantee (Tomas Matousek's purity principle): the generated
	/// <c>InitializeComponent</c> for a given XAML must be BYTE-IDENTICAL regardless of how that
	/// content was reached. Both sources here come from the SAME generator driver/options, so edit
	/// history is the only variable: phase 1 (<c>icV1</c>) generates V1 from a clean state; phase 3
	/// (<c>icV3</c>) reaches byte-identical V1 content by reverting an edit (V1→V2→V1). If any generated
	/// value, such as registry node IDs, depended on edit history, the two would differ; they must not.
	/// Covers a property-only edit AND a structural edit (added child).
	/// </summary>
	[Theory]
	[InlineData("<Label Text=\"Hi\" />", "<Label Text=\"Bye\" />")]          // property-only edit
	[InlineData("<Label Text=\"Hi\" />", "<Label Text=\"Hi\" /><Button Text=\"New\" />")] // structural edit (added child)
	public void RevertedGeneration_InitializeComponent_IsByteIdentical_ToInitialGeneration(string bodyV1, string bodyV2)
	{
		string Page(string body) => $$"""
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="Anchor" />
			        {{body}}
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var v1 = Page(bodyV1);
		var v2 = Page(bodyV2);

		using var harness = CreateHarness();
		var generation = harness.Generate(v1, v2, v1);

		// InitializeComponent for identical content must be identical regardless of edit history —
		// every value it embeds is a pure function of the current content, not of the path taken.
		Assert.Equal(generation[0].InitializeComponentSource, generation[2].InitializeComponentSource);
	}

	/// <summary>
	/// Regression for the inherited-XAML build break introduced by always-emitting UpdateComponent():
	/// a XAML class that derives from another class exposing an <c>internal UpdateComponent()</c> emits
	/// its own <c>UpdateComponent()</c>, which hides the base's (CS0108). Since UpdateComponent() is now
	/// emitted on every generation — not just on edit — this must compile cleanly. The generated UC file
	/// suppresses CS0108 (the hiding is intentional: each level patches its own XAML tree).
	/// </summary>
	[Fact]
	public void UpdateComponent_OnInheritedXamlClass_CompilesWithoutHidingWarning()
	{
		const string xaml = """
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <Label Text="Hi" />
			</ContentPage>
			""";

		// Code-behind: MainPage derives from a base that ALSO declares an internal UpdateComponent().
		const string stub = """
			namespace TestE2EApp;

			public partial class BaseXamlPage : global::Microsoft.Maui.Controls.ContentPage
			{
				internal void UpdateComponent() { }
			}

			public partial class MainPage : BaseXamlPage
			{
				private partial void InitializeComponent();
				public void InitializeComponentRuntime() { }
				public MainPage() { InitializeComponent(); }
			}
			""";

		using var harness = new XamlHotReloadTestHarness(
			nameof(UpdateComponent_OnInheritedXamlClass_CompilesWithoutHidingWarning),
			PageClass,
			stub);
		var generation = harness.Generate(xaml);
		Assert.NotNull(generation[0].UpdateComponentSource);
		var compilation = harness.Compile(generation[0]).Compilation;

		// CS0108 (member hides inherited member) must NOT appear at ANY severity — the pragma in the
		// generated UC file must suppress it. (Checked independently of general errors so the test
		// would fail if the pragma were missing, regardless of warnings-as-errors configuration.)
		var cs0108 = compilation.GetDiagnostics().Where(d => d.Id == "CS0108").ToArray();
		Assert.Empty(cs0108);
	}

	/// <summary>
	/// Diagnostics classification (Kirill Ovchinnikov's requirement), the "empty UpdateComponent" design.
	/// <c>UpdateComponent()</c> is always present (member stability / no EnC churn), but its BODY is empty
	/// when a generation carries no XAML change and non-empty (a patch) when the XAML changed. The runtime
	/// classifies a delta as a XAML change — SYNCHRONOUSLY, pre-dispatch, on
	/// <c>HotReloadRequestedEventArgs.HandledTypes</c>, exactly where XamlTools reads it today — by
	/// inspecting whether <c>UpdateComponent()</c>'s body is non-empty. An empty UC (a page whose XAML did
	/// not change, e.g. a pure C#/code-behind edit) is correctly NOT reported as a XAML change. No live
	/// instance or main-thread dispatcher is needed: classification is a property of the type's UC body.
	/// </summary>
	[MetadataUpdateFact]
	public void UpdateRequested_ReportsXamlChange_WhenUpdateComponentBodyIsNonEmpty()
	{
		const string switchName = "Microsoft.Maui.RuntimeFeature.IsIncrementalHotReloadEnabled";
		AppContext.TryGetSwitch(switchName, out var previousSwitch);
		AppContext.SetSwitch(switchName, true);

		string Page(string text) => $$"""
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="{{text}}" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var xamlV1 = Page("Hello");
		var xamlV2 = Page("World");

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2);
		Assert.NotNull(generation[0].UpdateComponentSource);
		Assert.NotNull(generation[1].UpdateComponentSource);

		IReadOnlyList<Type>? lastHandled = null;
		EventHandler<HotReloadRequestedEventArgs> capture = (_, e) => lastHandled = e.HandledTypes;
		HotReloadDiagnostics.UpdateRequested += capture;
		try
		{
			harness.RunLive(generation, live =>
			{
				var page = live.GetInstance<ContentPage>();
				var pageType = page.GetType();
				XamlComponentRegistry.Unregister(page);

				// The loaded type's UpdateComponent() is EMPTY → the update is NOT a XAML change.
				XamlIncrementalHotReloadHandler.UpdateApplication([pageType]);
				Assert.NotNull(lastHandled);
				Assert.DoesNotContain(pageType, lastHandled!);

				// Apply the V1→V2 delta: UpdateComponent()'s body becomes non-empty. The same notification
				// now classifies the type as a XAML change.
				live.ApplyUpdate<ContentPage>(1);
				XamlIncrementalHotReloadHandler.UpdateApplication([pageType]);
				Assert.NotNull(lastHandled);
				Assert.Contains(pageType, lastHandled!);
			});
		}
		finally
		{
			HotReloadDiagnostics.UpdateRequested -= capture;
			AppContext.SetSwitch(switchName, previousSwitch);
		}
	}

	/// <summary>
	/// The crown-jewel runtime proof of reverse version transitions (Kirill's requirement): a live
	/// instance created at V1 (Text="Hello"), hot-reloaded forward to V2 (Text="World"), then
	/// hot-reloaded BACKWARD to V1 (Text="Hello") — the live object's property must return to the
	/// baseline value. Because UpdateComponent() exists from the first generation, every transition is
	/// a method-body UPDATE (no member churn), and because patches are absolute the reverse patch
	/// deterministically restores the earlier value.
	/// </summary>
	[MetadataUpdateFact]
	public void PropertyRevert_AppliedViaHotReload_ReturnsToBaseline()
	{
		string Page(string text) => $$"""
			<?xml version="1.0" encoding="utf-8" ?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
			    <VerticalStackLayout>
			        <Label Text="{{text}}" />
			    </VerticalStackLayout>
			</ContentPage>
			""";

		var xamlV1 = Page("Hello");
		var xamlV2 = Page("World");
		var xamlV3 = Page("Hello"); // revert — byte-identical to V1

		using var harness = CreateHarness();
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);
		Assert.All(generation.Versions, version => Assert.NotNull(version.UpdateComponentSource));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = ((Layout)page.Content!).Children.OfType<Label>().First();
			Assert.Equal("Hello", label.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Same(label, ((Layout)page.Content!).Children.OfType<Label>().First());
			Assert.Equal("World", label.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
			Assert.Same(label, ((Layout)page.Content!).Children.OfType<Label>().First());
			Assert.Equal("Hello", label.Text);
		});
	}

	[Fact]
	public void IdenticalXaml_EmitsEmptyUC()
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
		var updateComponentSource = generation[1].UpdateComponentSource;
		Assert.NotNull(updateComponentSource);
		Assert.Contains("internal void UpdateComponent()", updateComponentSource!, StringComparison.Ordinal);
		Assert.DoesNotContain("XamlComponentRegistry", updateComponentSource!, StringComparison.Ordinal);
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
