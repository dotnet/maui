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
public partial class TemplateAndSelectorHotReloadTests
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
}
