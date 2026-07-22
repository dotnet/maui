#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

// Wave-2 · Templates & Selectors · TS-01..TS-06 (PR #36730)
//
// Mechanism — established EMPIRICALLY against this harness (by dumping the generated sources), not assumed:
//   * A keyed DataTemplate/ControlTemplate/DataTemplateSelector is emitted as a lazy resource factory:
//         __root.Resources.AddFactory("<Key>", () => { var t = new DataTemplate(); ...
//                                                      t.LoadTemplate = LoadTemplate_{line}_{pos}; return t; }, shared: true)
//     The stably-named local function LoadTemplate_{line}_{pos} lives lexically INSIDE that anonymous
//     AddFactory lambda (SetPropertiesVisitor.cs L245-296, #36482).
//   * The IHR apply path runs UpdateComponent (it does NOT re-run InitializeComponent). For a keyed template
//     resource, the dumped V2 UpdateComponent REPLACES the entry with a bare, factory-less template:
//         this.Resources["<Key>"] = new global::Microsoft.Maui.Controls.DataTemplate();   // LoadTemplate == null
//     The original template object that callers already hold keeps its construction-time (V1) factory
//     delegate, and that delegate is NOT refreshed by the InitializeComponent EnC edit.
//   * NET RESULT (empirically confirmed — a held template's CreateContent() returned "V1" after the edit):
//     after an update an ALREADY-REALIZED subtree is unchanged AND a FUTURE CreateContent()/SelectTemplate()
//     still serves the construction-time version; it does NOT reflect the edit. The plan's
//     "future realization uses the new factory" claim therefore does NOT hold on this harness. Per the
//     robustness rule it is RECLASSIFIED (not weakened) into skip-gated RED-PROBEs paired with each family's
//     GREEN anchor, referencing #36482 (DataTemplate edits under IHR) / #36732 (complex/attached reconcile).
//
// GREEN anchors assert only what is empirically true and faithful: (a) construction-time realization is
// correct (per-realization namescope isolation, selector branch selection, compiled binding, x:Reference,
// VSM TargetName isolation); (b) an already-realized subtree is stable across an update; (c) the generated
// source reflects the edit (the new factory in InitializeComponent, and the exact resource-replacement /
// skip markers in UpdateComponent). RED-PROBEs pin the future-realization gap and are skip-gated on the
// tracking issues.
//
// Item/selector types (TS-02/TS-03) compile into the collectible test ALC as additional sources, so host
// code reaches them through base types (DataTemplateSelector) + reflection (Assembly.GetType + Activator).
[Collection("XamlHotReloadTests")]
public class TemplateAndSelectorHotReloadTests
{
	const string PageClass = "TestTemplates.MainPage";

	const string PageStub = """
		namespace TestTemplates;

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

	// Odd/even DataTemplateSelector for TS-02. Odd items -> OddTemplate, even items -> EvenTemplate.
	const string SelectorSource = """
		namespace TestTemplates;

		public sealed class OddEvenSelector : global::Microsoft.Maui.Controls.DataTemplateSelector
		{
			public global::Microsoft.Maui.Controls.DataTemplate? OddTemplate { get; set; }
			public global::Microsoft.Maui.Controls.DataTemplate? EvenTemplate { get; set; }

			protected override global::Microsoft.Maui.Controls.DataTemplate OnSelectTemplate(object item, global::Microsoft.Maui.Controls.BindableObject container)
				=> (item is int i && (i % 2 != 0)) ? OddTemplate! : EvenTemplate!;
		}
		""";

	// TS-03 item types. Property names are deliberately collision-free (Caption/Heading, not Name/Title)
	// so the generated-source assertions can distinguish "new referenced type's member" from "stale member"
	// without matching incidental substrings (x:Name, TypeName, ...). This is a faithful stand-in for the
	// plan's `ItemB.Title` / `ItemA.Name` intent (see TS-03 provenance).
	const string ItemTypesSource = """
		namespace TestTemplates;

		public sealed class ItemA { public string? Caption { get; set; } }
		public sealed class ItemB { public string? Heading { get; set; } }
		""";

	static XamlHotReloadTestHarness CreateHarness([CallerMemberName] string scenarioName = "", params string[] additionalSources) =>
		new(scenarioName, PageClass, PageStub, additionalSources);

	// Reflectively build a BindingContext instance of a type that only exists inside the live ALC.
	static object MakeLiveItem(object anyLiveInstance, string typeName, string property, string value)
	{
		var type = anyLiveInstance.GetType().Assembly.GetType(typeName)
			?? throw new InvalidOperationException($"Type '{typeName}' not found in the live assembly.");
		var instance = Activator.CreateInstance(type)!;
		type.GetProperty(property)!.SetValue(instance, value);
		return instance;
	}

	static string KeyedDataTemplateXaml(string leaf) => $$"""
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestTemplates.MainPage">
		  <ContentPage.Resources>
		    <DataTemplate x:Key="Row">
		      <Label Text="{{leaf}}" />
		    </DataTemplate>
		  </ContentPage.Resources>
		  <VerticalStackLayout>
		    <Label Text="Host" />
		  </VerticalStackLayout>
		</ContentPage>
		""";

	// Wave-2 · Templates · TS-01 (GREEN anchor)
	// Provenance: MAUI §hot-reload DataTemplate realization; SetPropertiesVisitor.cs L245-296 (#36482);
	//             UpdateComponent resource-replacement verified by dumping the generated V2 source.
	// Faithfulness: reaches CreateContent (construction) + the live update; asserts construction correctness,
	//               already-realized stability, and that the generated source carries the edit; fails-for-bug:
	//               a body edit corrupts an already-realized subtree, or the writer stops emitting the new
	//               factory / stops replacing the keyed entry.
	// Expected: GREEN
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact]
	public void KeyedDataTemplate_EditBody_ConstructionStableAndSourceReflectsEdit()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(KeyedDataTemplateXaml("Alpha"), KeyedDataTemplateXaml("Bravo"));

		// Generated-source oracle: the new factory lands in InitializeComponent, and UpdateComponent replaces
		// the keyed entry with a bare template (the reason future realization is reclassified below).
		Assert.NotNull(generation[1].InitializeComponentSource);
		Assert.Contains("Bravo", generation[1].InitializeComponentSource!, StringComparison.Ordinal);
		Assert.NotNull(generation[1].UpdateComponentSource);
		Assert.Contains(
			"this.Resources[\"Row\"] = new global::Microsoft.Maui.Controls.DataTemplate();",
			generation[1].UpdateComponentSource!,
			StringComparison.Ordinal);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var template = Assert.IsType<DataTemplate>(page.Resources["Row"]);

			// Construction realization is correct.
			var r1 = Assert.IsType<Label>(template.CreateContent());
			Assert.Equal("Alpha", r1.Text);

			// The update applies cleanly and the already-realized subtree is left intact.
			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Equal("Alpha", r1.Text);
		});
	}

	// Wave-2 · Templates · TS-01 (RED-PROBE — reclassified future realization)
	// Provenance: keyed template resources are replaced by a factory-less `new DataTemplate()` in
	//             UpdateComponent (dumped), so a held template's next CreateContent() serves the stale factory.
	// Faithfulness: reaches the post-update CreateContent(); fails-for-bug: future realization does not reflect
	//               the edited body (observed "Alpha" instead of "Bravo").
	// Expected: RED-PROBE(#36482) — green anchor: KeyedDataTemplate_EditBody_ConstructionStableAndSourceReflectsEdit
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact(Skip = "Keyed DataTemplate future realization does not reflect the edit under IHR (writer replaces the entry with a factory-less new DataTemplate) — see #36482; green anchor: KeyedDataTemplate_EditBody_ConstructionStableAndSourceReflectsEdit")]
	public void KeyedDataTemplate_EditBody_FutureRealizationReflectsNewFactory()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(KeyedDataTemplateXaml("Alpha"), KeyedDataTemplateXaml("Bravo"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var template = Assert.IsType<DataTemplate>(page.Resources["Row"]);

			var r1 = Assert.IsType<Label>(template.CreateContent());
			Assert.Equal("Alpha", r1.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Equal("Alpha", r1.Text); // already-realized subtree stays on V1 (correct)

			// The intended-but-not-yet-delivered behavior: the next realization should reflect the edit.
			var r2 = Assert.IsType<Label>(template.CreateContent());
			Assert.Equal("Bravo", r2.Text);
			Assert.NotSame(r1, r2);
		});
	}

	static string SelectorXaml(string oddText) => $$"""
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:TestTemplates"
		             x:Class="TestTemplates.MainPage">
		  <ContentPage.Resources>
		    <local:OddEvenSelector x:Key="Sel">
		      <local:OddEvenSelector.OddTemplate>
		        <DataTemplate>
		          <Label Text="{{oddText}}" />
		        </DataTemplate>
		      </local:OddEvenSelector.OddTemplate>
		      <local:OddEvenSelector.EvenTemplate>
		        <DataTemplate>
		          <Label Text="EvenA" />
		        </DataTemplate>
		      </local:OddEvenSelector.EvenTemplate>
		    </local:OddEvenSelector>
		  </ContentPage.Resources>
		  <VerticalStackLayout>
		    <Label Text="Host" />
		  </VerticalStackLayout>
		</ContentPage>
		""";

	// Wave-2 · Selectors · TS-02 (GREEN anchor)
	// Provenance: MAUI §hot-reload DataTemplateSelector realization; SetPropertiesVisitor.cs L245-296 (#36482)
	// Faithfulness: reaches SelectTemplate(...).CreateContent(); asserts the selector routes odd/even to
	//               distinct branch templates, existing realizations stay put across an update, and the edit
	//               reaches the generated source; fails-for-bug: a branch returns null/bare, both branches
	//               collapse to one template, or the writer drops the new odd-branch factory.
	// Expected: GREEN
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact]
	public void Selector_ConstructionDistinguishesBranchesAndIsStableAcrossUpdate()
	{
		using var harness = CreateHarness(additionalSources: SelectorSource);
		var generation = harness.Generate(SelectorXaml("OddB1"), SelectorXaml("OddB2"));

		Assert.NotNull(generation[1].InitializeComponentSource);
		Assert.Contains("OddB2", generation[1].InitializeComponentSource!, StringComparison.Ordinal);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var selector = Assert.IsAssignableFrom<DataTemplateSelector>(page.Resources["Sel"]);

			// Odd vs even resolve to DISTINCT branch templates, each producing its own leaf.
			var oddTemplate = selector.SelectTemplate(1, page);
			var evenTemplate = selector.SelectTemplate(2, page);
			Assert.NotNull(oddTemplate);
			Assert.NotNull(evenTemplate);
			Assert.NotSame(oddTemplate, evenTemplate);

			var odd = Assert.IsType<Label>(oddTemplate!.CreateContent());
			var even = Assert.IsType<Label>(evenTemplate!.CreateContent());
			Assert.Equal("OddB1", odd.Text);
			Assert.Equal("EvenA", even.Text);

			// The update applies cleanly; the already-realized odd subtree is unchanged.
			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Equal("OddB1", odd.Text);
		});
	}

	// Wave-2 · Selectors · TS-02 (RED-PROBE — reclassified future realization)
	// Provenance: the selector's keyed branch templates keep their construction-time factories after an edit
	//             (same keyed-resource replacement gap as TS-01).
	// Faithfulness: reaches the post-update SelectTemplate(...).CreateContent(); fails-for-bug: a future odd
	//               selection still yields the old leaf ("OddB1" instead of "OddB2").
	// Expected: RED-PROBE(#36482) — green anchor: Selector_ConstructionDistinguishesBranchesAndIsStableAcrossUpdate
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact(Skip = "DataTemplateSelector future realization does not reflect the edited odd-branch factory under IHR — see #36482; green anchor: Selector_ConstructionDistinguishesBranchesAndIsStableAcrossUpdate")]
	public void Selector_FutureRealizationReflectsNewFactory()
	{
		using var harness = CreateHarness(additionalSources: SelectorSource);
		var generation = harness.Generate(SelectorXaml("OddB1"), SelectorXaml("OddB2"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var selector = Assert.IsAssignableFrom<DataTemplateSelector>(page.Resources["Sel"]);

			var r1 = Assert.IsType<Label>(selector.SelectTemplate(1, page)!.CreateContent());
			Assert.Equal("OddB1", r1.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Equal("OddB1", r1.Text);

			var r2 = Assert.IsType<Label>(selector.SelectTemplate(1, page)!.CreateContent());
			Assert.Equal("OddB2", r2.Text); // future odd realization should be V2
		});
	}

	const string CompiledTemplateShape = """
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:TestTemplates"
		             x:Class="TestTemplates.MainPage">
		  <ContentPage.Resources>
		    <DataTemplate x:Key="Row" x:DataType="__DT__">
		      <Label Text="{Binding __PROP__}" />
		    </DataTemplate>
		  </ContentPage.Resources>
		  <VerticalStackLayout>
		    <Label Text="Host" />
		  </VerticalStackLayout>
		</ContentPage>
		""";

	static string CompiledTemplateXaml(string dataType, string prop) => CompiledTemplateShape
		.Replace("__DT__", dataType, StringComparison.Ordinal)
		.Replace("__PROP__", prop, StringComparison.Ordinal);

	// Wave-2 · Templates · TS-03 (GREEN anchor)
	// Provenance: MAUI §hot-reload compiled-binding retype; SetPropertiesVisitor.cs L245-296 + compiled-binding
	//             getter (CreateTypedBindingFrom_*, #36482).
	// Faithfulness: reaches the generated InitializeComponent source across a retype+reverse AND the V1
	//               compiled-binding realization; asserts the emitted accessor tracks the current x:DataType
	//               (retype and reverse) and construction binds correctly; fails-for-bug: a retyped x:DataType
	//               keeps emitting the stale accessor, or the compiled binding fails at construction.
	// Expected: GREEN
	// Issue: https://github.com/dotnet/maui/issues/36482
	[Theory]
	[InlineData("local:ItemA", "local:ItemB")]
	[InlineData("{x:Type local:ItemA}", "{x:Type local:ItemB}")]
	public void CompiledTemplate_RetypeAndReverse_GeneratedSourceTracksCurrentType(string dataTypeA, string dataTypeB)
	{
		var xamlV1 = CompiledTemplateXaml(dataTypeA, "Caption");
		var xamlV2 = CompiledTemplateXaml(dataTypeB, "Heading");
		var xamlV3 = CompiledTemplateXaml(dataTypeA, "Caption");

		using var harness = CreateHarness(additionalSources: ItemTypesSource);
		var generation = harness.Generate(xamlV1, xamlV2, xamlV3);

		// V2 emits the new type's accessor and not the stale one; V3 reverses it back.
		var v2 = generation[1].InitializeComponentSource;
		var v3 = generation[2].InitializeComponentSource;
		Assert.NotNull(v2);
		Assert.NotNull(v3);
		Assert.Contains("Heading", v2!, StringComparison.Ordinal);
		Assert.Contains("ItemB", v2!, StringComparison.Ordinal);
		Assert.DoesNotContain("Caption", v2!, StringComparison.Ordinal);
		Assert.Contains("Caption", v3!, StringComparison.Ordinal);
		Assert.Contains("ItemA", v3!, StringComparison.Ordinal);
		Assert.DoesNotContain("Heading", v3!, StringComparison.Ordinal);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var template = Assert.IsType<DataTemplate>(page.Resources["Row"]);

			// V1 compiled binding realizes and binds against ItemA.Caption.
			var r1 = Assert.IsType<Label>(template.CreateContent());
			r1.BindingContext = MakeLiveItem(page, "TestTemplates.ItemA", "Caption", "alpha");
			Assert.Equal("alpha", r1.Text);

			// The update applies cleanly; the already-realized binding is unchanged.
			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Equal("alpha", r1.Text);
		});
	}

	// Wave-2 · Templates · TS-03 (RED-PROBE — reclassified future realization)
	// Provenance: the retyped compiled-binding getter lives in a keyed template whose entry is replaced by a
	//             factory-less `new DataTemplate()` on update (dumped), so a held template keeps the V1 getter.
	// Faithfulness: reaches the post-update CreateContent() + a live ItemB BindingContext; fails-for-bug: the
	//               future realization still binds the old type, yielding null instead of "bravo".
	// Expected: RED-PROBE(#36482) — green anchor: CompiledTemplate_RetypeAndReverse_GeneratedSourceTracksCurrentType
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact(Skip = "Compiled-binding retype does not reach future realizations of a keyed DataTemplate under IHR — see #36482; green anchor: CompiledTemplate_RetypeAndReverse_GeneratedSourceTracksCurrentType")]
	public void CompiledTemplate_Retype_FutureRealizationBindsNewType()
	{
		using var harness = CreateHarness(additionalSources: ItemTypesSource);
		var generation = harness.Generate(
			CompiledTemplateXaml("local:ItemA", "Caption"),
			CompiledTemplateXaml("local:ItemB", "Heading"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var template = Assert.IsType<DataTemplate>(page.Resources["Row"]);

			var r1 = Assert.IsType<Label>(template.CreateContent());
			r1.BindingContext = MakeLiveItem(page, "TestTemplates.ItemA", "Caption", "alpha");
			Assert.Equal("alpha", r1.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));

			var r2 = Assert.IsType<Label>(template.CreateContent());
			r2.BindingContext = MakeLiveItem(page, "TestTemplates.ItemB", "Heading", "bravo");
			Assert.Equal("bravo", r2.Text); // future realization should bind ItemB.Heading
		});
	}

	static string ControlTemplateXaml(string baseText, string activeText) => $$"""
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestTemplates.MainPage">
		  <ContentPage.Resources>
		    <ControlTemplate x:Key="Ct">
		      <VerticalStackLayout>
		        <Label x:Name="Target" Text="{{baseText}}" />
		        <Label x:Name="Mirror" Text="{Binding Source={x:Reference Target}, Path=Text}" />
		        <Label x:Name="VsmTarget" Text="idle" />
		        <VisualStateManager.VisualStateGroups>
		          <VisualStateGroup x:Name="CommonStates">
		            <VisualState x:Name="Normal" />
		            <VisualState x:Name="Active">
		              <VisualState.Setters>
		                <Setter TargetName="VsmTarget" Property="Label.Text" Value="{{activeText}}" />
		              </VisualState.Setters>
		            </VisualState>
		          </VisualStateGroup>
		        </VisualStateManager.VisualStateGroups>
		      </VerticalStackLayout>
		    </ControlTemplate>
		  </ContentPage.Resources>
		  <VerticalStackLayout>
		    <Label Text="Host" />
		  </VerticalStackLayout>
		</ContentPage>
		""";

	// Wave-2 · Templates · TS-04 (GREEN anchor)
	// Provenance: MAUI §hot-reload ControlTemplate construction (namescope / x:Reference / VSM TargetName);
	//             SetNamescopesAndRegisterNamesVisitor via LoadTemplate (#36482).
	// Faithfulness: reaches ControlTemplate.CreateContent() and drives GoToState headlessly (pure BindableObject
	//               logic, per VisualStateManagerTests.TargetedVisualElementGoesToCorrectState); asserts each
	//               realization owns its namescope, x:Reference resolves to that realization's own Target, and a
	//               VSM TargetName setter mutates only its own realization; fails-for-bug: realizations share a
	//               namescope, x:Reference leaks across realizations, or a VSM setter escapes its root.
	// Expected: GREEN — verified empirically; x:Reference is asserted at construction only (mutating a bound
	//                   source needs a dispatcher headlessly), and the VSM setter targets an UNBOUND label so
	//                   GoToState never triggers a binding proxy.
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact]
	public void ControlTemplate_Construction_NamescopeXReferenceAndVsmAreIsolated()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(ControlTemplateXaml("V1", "V1-Active"), ControlTemplateXaml("V2", "V2-Active"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var template = Assert.IsType<ControlTemplate>(page.Resources["Ct"]);

			// Two realizations from the SAME (V1) template.
			var rootA = Assert.IsType<VerticalStackLayout>(template.CreateContent());
			var rootB = Assert.IsType<VerticalStackLayout>(template.CreateContent());
			Assert.NotSame(rootA, rootB);

			var targetA = rootA.FindByName<Label>("Target");
			var targetB = rootB.FindByName<Label>("Target");
			var mirrorA = rootA.FindByName<Label>("Mirror");
			var mirrorB = rootB.FindByName<Label>("Mirror");
			var vsmTargetA = rootA.FindByName<Label>("VsmTarget");
			var vsmTargetB = rootB.FindByName<Label>("VsmTarget");
			Assert.NotNull(targetA);
			Assert.NotNull(targetB);
			Assert.NotNull(mirrorA);
			Assert.NotNull(mirrorB);
			Assert.NotNull(vsmTargetA);
			Assert.NotNull(vsmTargetB);

			// Per-realization namescope: the same name resolves to distinct elements.
			Assert.NotSame(targetA, targetB);
			Assert.NotSame(vsmTargetA, vsmTargetB);
			Assert.Equal("V1", targetA!.Text);

			// x:Reference resolved (at construction) to each realization's OWN Target.
			Assert.Equal("V1", mirrorA!.Text);
			Assert.Equal("V1", mirrorB!.Text);

			// VSM TargetName isolation: activating A mutates only A's (unbound) VsmTarget.
			Assert.Equal("idle", vsmTargetA!.Text);
			Assert.True(VisualStateManager.GoToState(rootA, "Active"));
			Assert.Equal("V1-Active", vsmTargetA.Text);
			Assert.Equal("idle", vsmTargetB!.Text);
			Assert.True(VisualStateManager.GoToState(rootA, "Normal"));
			Assert.Equal("idle", vsmTargetA.Text); // Active setters unapplied
		});
	}

	// Wave-2 · Templates · TS-04 (RED-PROBE — reclassified future realization)
	// Provenance: the ControlTemplate is a keyed resource replaced by a factory-less template on update (dumped);
	//             a held template keeps its V1 factory, so a future realization stays V1.
	// Faithfulness: reaches the post-update CreateContent() and FindByName; fails-for-bug: the next realization's
	//               Target is still "V1" instead of "V2".
	// Expected: RED-PROBE(#36482) — green anchor: ControlTemplate_Construction_NamescopeXReferenceAndVsmAreIsolated
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact(Skip = "ControlTemplate future realization does not reflect the edit under IHR (keyed-resource replacement) — see #36482; green anchor: ControlTemplate_Construction_NamescopeXReferenceAndVsmAreIsolated")]
	public void ControlTemplate_FutureRealizationReflectsNewVersion()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(ControlTemplateXaml("V1", "V1-Active"), ControlTemplateXaml("V2", "V2-Active"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var template = Assert.IsType<ControlTemplate>(page.Resources["Ct"]);

			var rootA = Assert.IsType<VerticalStackLayout>(template.CreateContent());
			Assert.Equal("V1", rootA.FindByName<Label>("Target")!.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));

			var rootC = Assert.IsType<VerticalStackLayout>(template.CreateContent());
			Assert.Equal("V2", rootC.FindByName<Label>("Target")!.Text); // future realization should be V2
		});
	}

	static string BindableLayoutXaml(string text) => $$"""
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="TestTemplates.MainPage">
		  <VerticalStackLayout>
		    <BindableLayout.ItemTemplate>
		      <DataTemplate>
		        <Label Text="{{text}}" />
		      </DataTemplate>
		    </BindableLayout.ItemTemplate>
		  </VerticalStackLayout>
		</ContentPage>
		""";

	// Wave-2 · Templates · TS-05 (GREEN guard)
	// Provenance: MAUI §hot-reload complex/attached-property gap; UpdateComponentCodeWriter.cs L1319.
	// Faithfulness: reaches the writer's attached-complex branch (L1319) and pins the exact skip marker plus a
	//               successful compile of the emitted V2 UpdateComponent.
	// Expected: DOC-SKIP-GUARD
	// Issue: https://github.com/dotnet/maui/issues/36732
	[Fact]
	public void BindableLayoutTemplate_AttachedComplex_EmitsSkipMarker()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(BindableLayoutXaml("V1"), BindableLayoutXaml("V2"));
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		// Exact marker text (em-dash U+2014) from UpdateComponentCodeWriter.cs L1319, with the attached
		// property's dotted LocalName (propDiff.PropertyName.LocalName, L1132).
		Assert.Contains("Complex attached property 'BindableLayout.ItemTemplate' — skipped", updateComponentSource!, StringComparison.Ordinal);

		var compilation = harness.Compile(generation[1]);
		Assert.True(compilation.PeImage.Length > 0, "Generated V2 UpdateComponent should still compile.");
	}

	// Wave-2 · Templates · TS-06 (RED-PROBE)
	// Provenance: MAUI §hot-reload complex/collection reconcile; UpdateComponentCodeWriter attached-complex skip (L1319).
	// Faithfulness: reaches the attached-complex skip and would exercise BindableLayout's controller re-templating;
	//               fails-for-bug: retype/reverse of the item template duplicates or orphans controller children.
	// Expected: RED-PROBE(#36732) — paired green anchor: BindableLayoutTemplate_AttachedComplex_EmitsSkipMarker
	// Issue: https://github.com/dotnet/maui/issues/36732
	[MetadataUpdateFact(Skip = "Blocked by writer complex-property gap — see #36732; green anchor: BindableLayoutTemplate_AttachedComplex_EmitsSkipMarker")]
	public void BindableLayoutTemplate_RetypeAndReverse_DoesNotDuplicateControllerChildren()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(BindableLayoutXaml("V1"), BindableLayoutXaml("V2"), BindableLayoutXaml("V3"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var host = Assert.IsType<VerticalStackLayout>(page.Content);

			var items = new ObservableCollection<string> { "a", "b" };
			BindableLayout.SetItemsSource(host, items);
			Assert.Equal(2, host.Children.Count);

			IReadOnlyList<object> ChildSnapshot() => host.Children.ToList();
			var beforeUpdate = ChildSnapshot();

			// Retype the item template; the controller must re-template WITHOUT duplicating/orphaning children.
			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));
			Assert.Equal(2, host.Children.Count);
			Assert.DoesNotContain(host.Children, c => beforeUpdate.Contains(c)); // no stale child left parented

			// Reverse the retype.
			Assert.Same(page, live.ApplyUpdate<ContentPage>(2));
			Assert.Equal(2, host.Children.Count);

			// A new source item after V3 must produce exactly one additional child (no double subscription).
			items.Add("c");
			Assert.Equal(3, host.Children.Count);
		});
	}
}
