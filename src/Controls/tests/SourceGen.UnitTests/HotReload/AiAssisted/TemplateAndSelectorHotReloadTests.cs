#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

// Wave-2 · Templates & Selectors · TS-01..TS-05 (PR #36730)
//
// Mechanism — established empirically against this harness by inspecting the generated sources:
//   * keyed DataTemplate/ControlTemplate/DataTemplateSelector resources are emitted as lazy factories whose
//     LoadTemplate delegates live inside the generated AddFactory lambda (SetPropertiesVisitor.cs L245-296,
//     #36482).
//   * the IHR apply path runs UpdateComponent (it does not re-run InitializeComponent). For keyed template
//     resources, the current writer replaces the keyed entry with a bare template in UpdateComponent.
//
// Net result: this PR keeps only the construction/source guards that are faithful on the current harness.
// They prove construction-time realization, already-realized stability, and generated-source shape.
// Future-realization follow-ups remain tracked under #36482, while complex/attached-property reconciliation
// scenarios remain tracked under #36732 and are intentionally left out of this PR.
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
