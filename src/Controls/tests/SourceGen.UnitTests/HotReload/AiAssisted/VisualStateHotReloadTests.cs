#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

[Collection("XamlHotReloadTests")]
public class VisualStateHotReloadTests
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

	const string CountingBehaviorStub = """
		namespace TestAiAssisted;

		public sealed class CountingBehavior : global::Microsoft.Maui.Controls.Behavior<global::Microsoft.Maui.Controls.VisualElement>
		{
			public static int AttachCount { get; private set; }

			public static int DetachCount { get; private set; }

			public static int CallbackCount { get; private set; }

			protected override void OnAttachedTo(global::Microsoft.Maui.Controls.VisualElement bindable)
			{
				base.OnAttachedTo(bindable);
				AttachCount++;
				bindable.PropertyChanged += OnPropertyChanged;
			}

			protected override void OnDetachingFrom(global::Microsoft.Maui.Controls.VisualElement bindable)
			{
				bindable.PropertyChanged -= OnPropertyChanged;
				DetachCount++;
				base.OnDetachingFrom(bindable);
			}

			static void OnPropertyChanged(object? sender, global::System.ComponentModel.PropertyChangedEventArgs args)
			{
				if (args.PropertyName == nameof(global::Microsoft.Maui.Controls.VisualElement.IsEnabled))
					CallbackCount++;
			}
		}
		""";

	static XamlHotReloadTestHarness CreateHarness([CallerMemberName] string scenarioName = "") =>
		new(scenarioName, PageClass, PageStub);

	static XamlHotReloadTestHarness CreateBehaviorHarness([CallerMemberName] string scenarioName = "") =>
		new(scenarioName, PageClass, PageStub, CountingBehaviorStub);

	static XamlHotReloadTestHarness CreateHostedHarness(
		XamlHotReloadApplicationOptions options,
		[CallerMemberName] string scenarioName = "") =>
		new(scenarioName, PageClass, PageStub, options);

	// Wave2 · Visual State · P0-03 · VS-01
	// Provenance: MAUI §2.1; public-app T13/T14; PoolMath CAT-02
	// Faithfulness: reaches writer L1319; fails-for-bug: active VSM setter edits are explicitly declined.
	// Expected: DOC-SKIP-GUARD
	// Issue: https://github.com/dotnet/maui/issues/36732
	[Fact]
	public void ActiveVsmSetter_ComplexAttachedProperty_EmitsSkipMarker()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(
			CreateActiveStateXaml("BackgroundColor", "Red"),
			CreateActiveStateXaml("BackgroundColor", "Blue"));
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains(
			"Complex attached property 'VisualStateManager.VisualStateGroups' — skipped",
			updateComponentSource!,
			StringComparison.Ordinal);

		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Generated VSM skip path should compile.");
	}

	// Wave2 · Visual State · P0-03 · VS-02
	// Provenance: MAUI §2.1; public-app T13/T14; PoolMath CAT-02
	// Faithfulness: reaches writer L1319; fails-for-bug: the active state retains stale setter values.
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[Theory(Skip = "RED-PROBE #36732: active VSM setter reconciliation is not implemented.")]
	[InlineData("BackgroundColor", "Red", "Blue")]
	[InlineData("Opacity", ".25", ".75")]
	[InlineData("Scale", "1", "2")]
	public void ActiveVsmSetter_EditAndReverse_ReappliesImmediately(
		string propertyName,
		string valueV1,
		string valueV2)
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(
			CreateActiveStateXaml(propertyName, valueV1),
			CreateActiveStateXaml(propertyName, valueV2),
			CreateActiveStateXaml(propertyName, valueV1));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);

			Assert.True(VisualStateManager.GoToState(label, "Active"));
			AssertActiveState(label);
			AssertVisualProperty(label, propertyName, valueV1);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Same(label, page.Content);
			AssertActiveState(label);
			AssertVisualProperty(label, propertyName, valueV2);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
			Assert.Same(label, page.Content);
			AssertActiveState(label);
			AssertVisualProperty(label, propertyName, valueV1);
		});
	}

	// Wave2 · Behavior · P0-04 · VS-03
	// Provenance: MAUI §3.4; toolkit T13/T15/T17; public-app T17
	// Faithfulness: reaches writer L1167/L1194; fails-for-bug: Behavior re-add is explicitly declined.
	// Expected: DOC-SKIP-GUARD
	// Issue: https://github.com/dotnet/maui/issues/36732
	[Fact]
	public void Behavior_ClearAndComplexProperty_EmitsSkipMarker()
	{
		using var harness = CreateBehaviorHarness();
		var generation = harness.Generate(
			CreateBehaviorXaml("V1", includeBehavior: true),
			CreateBehaviorXaml("V2", includeBehavior: false),
			CreateBehaviorXaml("V3", includeBehavior: true));
		var removalSource = generation[1].UpdateComponentSource;
		var reAddSource = generation[2].UpdateComponentSource;

		Assert.NotNull(removalSource);
		Assert.Contains(
			"RemoveBinding(global::Microsoft.Maui.Controls.Entry.BehaviorsProperty)",
			removalSource!,
			StringComparison.Ordinal);
		Assert.Contains(
			"ClearValue(global::Microsoft.Maui.Controls.Entry.BehaviorsProperty)",
			removalSource!,
			StringComparison.Ordinal);
		Assert.NotNull(reAddSource);
		Assert.Contains(
			"Complex property 'Behaviors' (ElementNode) — skipped (not yet supported)",
			reAddSource!,
			StringComparison.Ordinal);

		Assert.True(harness.Compile(generation[1]).PeImage.Length > 0, "Generated Behavior clear path should compile.");
		Assert.True(harness.Compile(generation[2]).PeImage.Length > 0, "Generated Behavior re-add skip path should compile.");
	}

	// Wave2 · Behavior · P0-04 · VS-04
	// Provenance: MAUI §3.4; toolkit T13/T15/T17; public-app T17
	// Faithfulness: reaches writer L1167/L1194; fails-for-bug: detach/attach lifecycle is not reconciled.
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "RED-PROBE #36732: Behavior collection lifecycle reconciliation is not implemented.")]
	public void BehaviorCollection_RemoveReAdd_DetachesAndAttachesOnce()
	{
		using var harness = CreateBehaviorHarness();
		var generation = harness.Generate(
			CreateBehaviorXaml("V1", includeBehavior: true),
			CreateBehaviorXaml("V2", includeBehavior: false),
			CreateBehaviorXaml("V3", includeBehavior: true));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var entry = Assert.IsType<Entry>(page.Content);
			var oldBehavior = Assert.Single(entry.Behaviors);

			AssertBehaviorCounts(oldBehavior, attach: 1, detach: 0, callbacks: 0);
			entry.IsEnabled = false;
			AssertBehaviorCounts(oldBehavior, attach: 1, detach: 0, callbacks: 1);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Same(entry, page.Content);
			Assert.Empty(entry.Behaviors);
			AssertBehaviorCounts(oldBehavior, attach: 1, detach: 1, callbacks: 1);
			entry.IsEnabled = true;
			AssertBehaviorCounts(oldBehavior, attach: 1, detach: 1, callbacks: 1);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
			Assert.Same(entry, page.Content);
			var newBehavior = Assert.Single(entry.Behaviors);
			Assert.NotSame(oldBehavior, newBehavior);
			AssertBehaviorCounts(newBehavior, attach: 2, detach: 1, callbacks: 1);
			entry.IsEnabled = false;
			AssertBehaviorCounts(newBehavior, attach: 2, detach: 1, callbacks: 2);
			Assert.Single(entry.Behaviors);
		});
	}

	// Wave2 · Visual State · family 2 core · VS-05
	// Provenance: MAUI §2.1; public-app T13/T14; minimal add/remove/re-add extension
	// Faithfulness: reaches writer L1319; fails-for-bug: state removal cannot apply fallback or re-add a live state.
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "RED-PROBE #36732: VSM state graph reconciliation and fallback are not implemented.")]
	public void VsmState_AddRemoveReAdd_And_FallbackReversion()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(
			CreateStateGraphXaml(includeActive: true, activeColor: "Red"),
			CreateStateGraphXaml(includeActive: false, activeColor: null),
			CreateStateGraphXaml(includeActive: true, activeColor: "Green"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var label = Assert.IsType<Label>(page.Content);
			var initialGroup = Assert.Single(VisualStateManager.GetVisualStateGroups(label));
			var oldActiveState = Assert.Single(initialGroup.States, static state => state.Name == "Active");

			Assert.True(VisualStateManager.GoToState(label, "Active"));
			Assert.Equal("Active", initialGroup.CurrentState.Name);
			Assert.Equal(Colors.Red, label.BackgroundColor);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Same(label, page.Content);
			var fallbackGroup = Assert.Single(VisualStateManager.GetVisualStateGroups(label));
			Assert.DoesNotContain(fallbackGroup.States, static state => state.Name == "Active");
			Assert.Equal("Normal", fallbackGroup.CurrentState.Name);
			Assert.Equal(Colors.Blue, label.BackgroundColor);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
			Assert.Same(label, page.Content);
			var reAddedGroup = Assert.Single(VisualStateManager.GetVisualStateGroups(label));
			var newActiveState = Assert.Single(reAddedGroup.States, static state => state.Name == "Active");
			Assert.NotSame(oldActiveState, newActiveState);
			Assert.Equal("Normal", reAddedGroup.CurrentState.Name);
			Assert.Equal(Colors.Blue, label.BackgroundColor);

			Assert.True(VisualStateManager.GoToState(label, "Active"));
			Assert.Equal("Active", reAddedGroup.CurrentState.Name);
			Assert.Equal(Colors.Green, label.BackgroundColor);
		});
	}

	// Wave2 · Visual State · family 2 combined · VS-06
	// Provenance: MAUI §2.1; public-app T13/T14; cap-app-host/theme extension
	// Faithfulness: reaches writer L1319 with nested AppThemeBinding/StaticResource values;
	// fails-for-bug: generated VisualStateGroups replacement is explicitly declined.
	// Expected: DOC-SKIP-GUARD
	// Issue: https://github.com/dotnet/maui/issues/36732
	[Fact]
	public void ActiveVsmThemeResourceSetter_ComplexAttachedProperty_EmitsSkipMarker()
	{
		using var harness = CreateHostedHarness(CreateThemeOptions());
		var generation = harness.Generate(
			CreateThemeResourceStateXaml("V1"),
			CreateThemeResourceStateXaml("V2"));
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains(
			"Complex attached property 'VisualStateManager.VisualStateGroups' — skipped",
			updateComponentSource!,
			StringComparison.Ordinal);
		Assert.True(
			harness.Compile(generation[1]).PeImage.Length > 0,
			"Generated VSM theme/resource skip path should compile.");
	}

	// Wave2 · Visual State · family 2 combined · VS-06
	// Provenance: MAUI §2.1; public-app T13/T14; cap-app-host/theme extension
	// Faithfulness: combines an active VSM setter with AppThemeBinding and app-level
	// StaticResource lookup; fails-for-bug: the active setter remains on its V1 resource keys.
	// Expected: RED-PROBE(#36732)
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "RED-PROBE #36732: active VSM theme/resource setter reconciliation is not implemented.")]
	public void ActiveVsmThemeResourceSetter_EditAndReverse_PreservesStateAndThemeSemantics()
	{
		var previousApplication = Application.Current;

		using (var harness = CreateHostedHarness(CreateThemeOptions()))
		{
			var generation = harness.Generate(
				CreateThemeResourceStateXaml("V1"),
				CreateThemeResourceStateXaml("V2"),
				CreateThemeResourceStateXaml("V1"));

			harness.RunLive(generation, live =>
			{
				var page = live.GetInstance<ContentPage>();
				var label = Assert.IsType<Label>(page.Content);

				Assert.True(VisualStateManager.GoToState(label, "Active"));
				AssertActiveThemeState(label, harness, AppTheme.Light, Colors.Red);

				harness.ApplicationHost!.SetAppTheme(AppTheme.Dark);
				AssertActiveThemeState(label, harness, AppTheme.Dark, Colors.Maroon);

				Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
				Assert.Same(label, page.Content);
				AssertActiveThemeState(label, harness, AppTheme.Dark, Colors.Blue);

				harness.ApplicationHost.SetAppTheme(AppTheme.Light);
				AssertActiveThemeState(label, harness, AppTheme.Light, Colors.Green);

				Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
				Assert.Same(label, page.Content);
				AssertActiveThemeState(label, harness, AppTheme.Light, Colors.Red);

				harness.ApplicationHost.SetAppTheme(AppTheme.Dark);
				AssertActiveThemeState(label, harness, AppTheme.Dark, Colors.Maroon);
			});
		}

		Assert.Same(previousApplication, Application.Current);
	}

	static string CreateActiveStateXaml(string propertyName, string value) => $$"""
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestAiAssisted.MainPage">
		  <Label Text="Target">
		    <VisualStateManager.VisualStateGroups>
		      <VisualStateGroup Name="CommonStates">
		        <VisualState Name="Active">
		          <VisualState.Setters>
		            <Setter Property="{{propertyName}}" Value="{{value}}" />
		          </VisualState.Setters>
		        </VisualState>
		      </VisualStateGroup>
		    </VisualStateManager.VisualStateGroups>
		  </Label>
		</ContentPage>
		""";

	static string CreateBehaviorXaml(string text, bool includeBehavior)
	{
		var behavior = includeBehavior
			? """
				  <Entry.Behaviors>
				    <local:CountingBehavior />
				  </Entry.Behaviors>
				"""
			: string.Empty;

		return $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestAiAssisted"
			             x:Class="TestAiAssisted.MainPage">
			  <Entry Text="{{text}}">
			{{behavior}}
			  </Entry>
			</ContentPage>
			""";
	}

	static string CreateStateGraphXaml(bool includeActive, string? activeColor)
	{
		var activeState = includeActive
			? $$"""
				        <VisualState Name="Active">
				          <VisualState.Setters>
				            <Setter Property="BackgroundColor" Value="{{activeColor}}" />
				          </VisualState.Setters>
				        </VisualState>
				"""
			: string.Empty;

		return $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="Target">
			    <VisualStateManager.VisualStateGroups>
			      <VisualStateGroup Name="CommonStates">
			        <VisualState Name="Normal">
			          <VisualState.Setters>
			            <Setter Property="BackgroundColor" Value="Blue" />
			          </VisualState.Setters>
			        </VisualState>
			{{activeState}}
			      </VisualStateGroup>
			    </VisualStateManager.VisualStateGroups>
			  </Label>
			</ContentPage>
			""";
	}

	static string CreateThemeResourceStateXaml(string version)
	{
		var setterValue =
			$"{{AppThemeBinding Light={{StaticResource ActiveLight{version}}}, Dark={{StaticResource ActiveDark{version}}}}}";

		return $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestAiAssisted.MainPage">
			  <Label Text="Target">
			    <VisualStateManager.VisualStateGroups>
			      <VisualStateGroup Name="CommonStates">
			        <VisualState Name="Active">
			          <VisualState.Setters>
			            <Setter Property="BackgroundColor" Value="{{setterValue}}" />
			          </VisualState.Setters>
			        </VisualState>
			      </VisualStateGroup>
			    </VisualStateManager.VisualStateGroups>
			  </Label>
			</ContentPage>
			""";
	}

	static XamlHotReloadApplicationOptions CreateThemeOptions() =>
		new(
			new Dictionary<string, object>
			{
				["ActiveLightV1"] = Colors.Red,
				["ActiveDarkV1"] = Colors.Maroon,
				["ActiveLightV2"] = Colors.Green,
				["ActiveDarkV2"] = Colors.Blue,
			},
			AppTheme.Light);

	static void AssertActiveState(Label label)
	{
		var groups = VisualStateManager.GetVisualStateGroups(label);
		var group = Assert.Single(groups);
		Assert.Equal("Active", group.CurrentState.Name);
		var state = Assert.Single(group.States);
		Assert.Equal("Active", state.Name);
		Assert.Single(state.Setters);
	}

	static void AssertActiveThemeState(
		Label label,
		XamlHotReloadTestHarness harness,
		AppTheme theme,
		Color expectedColor)
	{
		AssertActiveState(label);
		Assert.Equal(theme, harness.ApplicationHost!.Application.UserAppTheme);
		Assert.Equal(expectedColor, label.BackgroundColor);
	}

	static void AssertVisualProperty(Label label, string propertyName, string value)
	{
		switch (propertyName)
		{
			case "BackgroundColor":
				Assert.Equal(value == "Red" ? Colors.Red : Colors.Blue, label.BackgroundColor);
				break;
			case "Opacity":
				Assert.Equal(double.Parse(value, CultureInfo.InvariantCulture), label.Opacity);
				break;
			case "Scale":
				Assert.Equal(double.Parse(value, CultureInfo.InvariantCulture), label.Scale);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(propertyName), propertyName, "Unknown VSM property.");
		}
	}

	static void AssertBehaviorCounts(
		Behavior behavior,
		int attach,
		int detach,
		int callbacks)
	{
		Assert.Equal(attach, GetStaticCounter(behavior, "AttachCount"));
		Assert.Equal(detach, GetStaticCounter(behavior, "DetachCount"));
		Assert.Equal(callbacks, GetStaticCounter(behavior, "CallbackCount"));
	}

	static int GetStaticCounter(Behavior behavior, string propertyName)
	{
		var property = behavior.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
		Assert.NotNull(property);
		return Assert.IsType<int>(property!.GetValue(null));
	}
}
