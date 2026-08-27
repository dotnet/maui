#nullable enable

using System;
using System.Reflection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

[Collection("XamlHotReloadTests")]
public class Maui37888Tests
{
	const string IncrementalHotReloadSwitch = "Microsoft.Maui.RuntimeFeature.IsIncrementalHotReloadEnabled";
	const string ResourceKey = "ExploratoryCaption";

	[MetadataUpdateTheory]
	[InlineData(false)]
	[InlineData(true)]
	public void ApplicationResourceUpdate_ReachesExistingAndNewConsumers(bool useMergedDictionary)
	{
		AppContext.TryGetSwitch(IncrementalHotReloadSwitch, out var previousSwitch);
		var previousApplication = Application.Current;
		AppContext.SetSwitch(IncrementalHotReloadSwitch, true);
		SetMainThreadImplementation();

		try
		{
			if (useMergedDictionary)
				VerifyMergedDictionaryUpdate();
			else
				VerifyInlineApplicationResourceUpdate();
		}
		finally
		{
			Application.Current = previousApplication;
			AppContext.SetSwitch(IncrementalHotReloadSwitch, previousSwitch);
			ClearMainThreadImplementation();
		}
	}

	static void VerifyInlineApplicationResourceUpdate()
	{
		const string rootClass = "TestMaui37888.App";
		const string codeBehind = """
			namespace TestMaui37888;

			public partial class App : global::Microsoft.Maui.Controls.Application
			{
				private partial void InitializeComponent();

				public App() => InitializeComponent();
			}
			""";

		string Xaml(string value) => $$"""
			<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="{{rootClass}}">
			  <Application.Resources>
			    <x:String x:Key="{{ResourceKey}}">{{value}}</x:String>
			  </Application.Resources>
			</Application>
			""";

		using var harness = new XamlHotReloadTestHarness(
			nameof(ApplicationResourceUpdate_ReachesExistingAndNewConsumers) + "_Inline",
			rootClass,
			codeBehind);
		var generation = harness.Generate(Xaml("App resource V0"), Xaml("App resource V1"));

		harness.RunLive(generation, live =>
		{
			var application = live.GetInstance<Application>();
			Application.Current = application;
			VerifyConsumers(application, () => live.ApplyUpdateThroughRuntimeHandler<Application>(1));
		});
	}

	static void VerifyMergedDictionaryUpdate()
	{
		const string rootClass = "TestMaui37888.AppResources";
		const string codeBehind = """
			namespace TestMaui37888;

			public partial class AppResources : global::Microsoft.Maui.Controls.ResourceDictionary
			{
				private partial void InitializeComponent();

				public AppResources() => InitializeComponent();
			}
			""";

		string Xaml(string value) => $$"""
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			                    x:Class="{{rootClass}}">
			  <x:String x:Key="{{ResourceKey}}">{{value}}</x:String>
			</ResourceDictionary>
			""";

		using var harness = new XamlHotReloadTestHarness(
			nameof(ApplicationResourceUpdate_ReachesExistingAndNewConsumers) + "_Merged",
			rootClass,
			codeBehind);
		var generation = harness.Generate(Xaml("App resource V0"), Xaml("App resource V1"));

		harness.RunLive(generation, live =>
		{
			var application = new Application();
			var dictionary = live.GetInstance<global::Microsoft.Maui.Controls.ResourceDictionary>();
			application.Resources.MergedDictionaries.Add(dictionary);
			Application.Current = application;
			VerifyConsumers(application, () => live.ApplyUpdateThroughRuntimeHandler<global::Microsoft.Maui.Controls.ResourceDictionary>(1));
		});
	}

	static void VerifyConsumers(Application application, Action applyUpdate)
	{
		var existingConsumer = CreateConsumer();
#pragma warning disable CS0618
		application.MainPage = new ContentPage { Content = existingConsumer };
#pragma warning restore CS0618
		Assert.Equal("App resource V0", existingConsumer.Text);

		applyUpdate();

		Assert.Equal("App resource V1", existingConsumer.Text);
		Assert.Equal("App resource V1", CreateConsumer().Text);
		Assert.Equal("App resource V1", application.Resources[ResourceKey]);
	}

	static Label CreateConsumer()
	{
		var label = new Label();
		label.SetDynamicResource(Label.TextProperty, ResourceKey);
		return label;
	}

	static void SetMainThreadImplementation()
	{
		var method = typeof(MainThread).GetMethod(
			"SetCustomImplementation",
			BindingFlags.Static | BindingFlags.NonPublic,
			binder: null,
			[typeof(Func<bool>), typeof(Action<Action>)],
			modifiers: null);
		Assert.NotNull(method);
		method!.Invoke(null, [() => true, (Action<Action>)(action => action())]);
	}

	static void ClearMainThreadImplementation()
	{
		var method = typeof(MainThread).GetMethod(
			"ClearCustomImplementation",
			BindingFlags.Static | BindingFlags.NonPublic);
		Assert.NotNull(method);
		method!.Invoke(null, null);
	}
}
