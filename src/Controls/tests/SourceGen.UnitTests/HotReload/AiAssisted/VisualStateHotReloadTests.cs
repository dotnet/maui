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
public partial class VisualStateHotReloadTests
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
