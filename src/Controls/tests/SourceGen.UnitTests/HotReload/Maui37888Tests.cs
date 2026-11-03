#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

[Collection("XamlHotReloadTests")]
public class Maui37888Tests
{
	const string IncrementalHotReloadSwitch = "Microsoft.Maui.RuntimeFeature.IsIncrementalHotReloadEnabled";
	const string ResourceKey = "ExploratoryCaption";

	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	public void AddedResourceRetainsOuterBindingContext(bool expressionBinding, bool referenceChild)
		=> VerifyAddedResourceBindingContext(expressionBinding, referenceChild, applyUpdate: false);

	[MetadataUpdateTheory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	public void AddedResourceResolvesOuterBindingAtRuntime(bool expressionBinding, bool referenceChild)
		=> VerifyAddedResourceBindingContext(expressionBinding, referenceChild, applyUpdate: true);

	static void VerifyAddedResourceBindingContext(bool expressionBinding, bool referenceChild, bool applyUpdate)
	{
		const string rootClass = "TestMaui37888.MainPage";
		const string codeBehind = """
			namespace TestMaui37888;
			public class Person
			{
				public string FirstName { get; set; } = "Ada";
			}
			public partial class MainPage
			{
				public MainPage() => InitializeComponent();
			}
			""";
		var addedResource = expressionBinding
			? """<Label x:Key="Caption" Text="{FirstName}" />"""
			: $$"""<Label x:Key="Caption" Text="{Binding Source={x:Reference {{(referenceChild ? "Owner" : "Page")}}}, Path={{(referenceChild ? "Text" : "Title")}}}" />""";
		string Xaml(bool added) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestMaui37888"
			             x:Class="{{rootClass}}" x:Name="Page" x:DataType="local:Person">
			  <ContentPage.Resources>
			    <ResourceDictionary>
			      <x:String x:Key="Existing">Unchanged</x:String>
			      <DataTemplate x:Key="ExistingTemplate">
			        <Label x:Name="Page" />
			      </DataTemplate>
			      {{(added ? addedResource : "")}}
			    </ResourceDictionary>
			  </ContentPage.Resources>
			  <Label x:Name="Owner" Text="Existing owner" />
			</ContentPage>
			""";
		using var harness = new XamlHotReloadTestHarness(nameof(AddedResourceRetainsOuterBindingContext), rootClass, codeBehind);
		var generations = harness.Generate(Xaml(false), Xaml(true));

		Assert.DoesNotContain(generations[1].GeneratorResult.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
		if (!applyUpdate)
		{
			Assert.NotEmpty(harness.Compile(generations[1]).PeImage);
			return;
		}

		harness.RunLive(generations, live =>
		{
			var page = live.GetInstance<ContentPage>();
			page.Title = "Existing page";
			page.BindingContext = Activator.CreateInstance(page.GetType().Assembly.GetType("TestMaui37888.Person")!);
			var owner = page.Content;
			live.ApplyUpdate<ContentPage>(1);

			Assert.Same(owner, page.Content);
			var caption = Assert.IsType<Label>(page.Resources["Caption"]);
			caption.BindingContext = page.BindingContext;
			Assert.Equal(expressionBinding ? "Ada" : referenceChild ? "Existing owner" : "Existing page", caption.Text);
		});
	}

	[MetadataUpdateTheory]
	[InlineData(false)]
	[InlineData(true)]
	public void AddedResourcePreservesLazySharedSemantics(bool shared)
	{
		const string rootClass = "TestMaui37888.MainPage";
		const string codeBehind = """
			namespace TestMaui37888;
			public class Token
			{
				public static int Created;
				public Token() => Created++;
			}
			public partial class MainPage
			{
				public int Created => Token.Created;
				public MainPage() => InitializeComponent();
			}
			""";
		string Xaml(bool added) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:TestMaui37888" x:Class="{{rootClass}}">
			  <ContentPage.Resources>
			    <ResourceDictionary>
			      <local:Token x:Key="Existing" />
			      {{(added ? $"""<local:Token x:Key="Added" x:Shared="{shared}" />""" : "")}}
			    </ResourceDictionary>
			  </ContentPage.Resources>
			</ContentPage>
			""";
		using var harness = new XamlHotReloadTestHarness(nameof(AddedResourcePreservesLazySharedSemantics), rootClass, codeBehind);
		var generations = harness.Generate(Xaml(false), Xaml(true));
		harness.RunLive(generations, live =>
		{
			var page = live.GetInstance<ContentPage>();
			live.ApplyUpdate<ContentPage>(1);

			Assert.Equal(0, page.GetType().GetProperty("Created")!.GetValue(page));
			var first = page.Resources["Added"];
			var second = page.Resources["Added"];
			Assert.Equal(shared, ReferenceEquals(first, second));
			Assert.Equal(shared ? 1 : 2, page.GetType().GetProperty("Created")!.GetValue(page));
		});
	}

	[MetadataUpdateTheory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, true)]
	public void AddingApplicationResources_UpdatesExistingAndNewConsumers(bool useMergedDictionary, bool explicitDictionary)
	{
		const string rootClass = "TestMaui37888.App";
		const string codeBehind = """
			namespace TestMaui37888;

			public partial class App
			{
				public App() => InitializeComponent();
			}
			""";
		const string additions = """
			<x:String x:Key="AddedResource">Added</x:String>
			<Style TargetType="{x:Type Label}">
			  <Setter Property="BackgroundColor" Value="Red" />
			</Style>
			""";

		IReadOnlyDictionary<string, XamlHotReloadDocument> Snapshot(bool addResources)
		{
			var resources = $$"""
				<x:String x:Key="ExistingResource">Unchanged</x:String>
				{{(addResources && !useMergedDictionary ? additions : "")}}
				""";
			if (explicitDictionary)
				resources = $$"""
					<ResourceDictionary>
					  <ResourceDictionary.MergedDictionaries>
					    <ResourceDictionary Source="Styles.xaml" />
					  </ResourceDictionary.MergedDictionaries>
					  {{resources}}
					</ResourceDictionary>
					""";

			return new Dictionary<string, XamlHotReloadDocument>(StringComparer.Ordinal)
			{
				["MainPage.xaml"] = new XamlHotReloadDocument(
					$$"""
					<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
					             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
					             x:Class="{{rootClass}}">
					  <Application.Resources>
					    {{resources}}
					  </Application.Resources>
					</Application>
					""",
					ManifestResourceName: "TestMaui37888.App.xaml"),
				["Styles.xaml"] = new XamlHotReloadDocument(
					$$"""
					<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
					                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
					  <x:String x:Key="MergedResource">Unchanged</x:String>
					  {{(addResources && useMergedDictionary ? additions : "")}}
					</ResourceDictionary>
					""",
					ManifestResourceName: "TestMaui37888.Styles.xaml"),
			};
		}

		AppContext.TryGetSwitch(IncrementalHotReloadSwitch, out var previousSwitch);
		var previousApplication = Application.Current;
		AppContext.SetSwitch(IncrementalHotReloadSwitch, true);
		SetMainThreadImplementation();

		try
		{
			using var harness = new XamlHotReloadTestHarness(
				nameof(AddingApplicationResources_UpdatesExistingAndNewConsumers),
				rootClass,
				codeBehind);
			var generations = harness.GenerateDocuments(Snapshot(false), Snapshot(true));

			harness.RunLive(generations, live =>
			{
				var application = live.GetInstance<Application>();
				Application.Current = application;
				var existingLabel = new Label();
				var layout = new VerticalStackLayout { Children = { existingLabel } };
#pragma warning disable CS0618
				application.MainPage = new ContentPage { Content = layout };
#pragma warning restore CS0618
				application.Resources["RuntimeResource"] = "Preserved";
				Assert.Null(existingLabel.BackgroundColor);

				Assert.Same(application, live.ApplyUpdateThroughRuntimeHandler<Application>(1));

				Assert.Same(existingLabel, layout.Children[0]);
				Assert.Equal(Colors.Red, existingLabel.BackgroundColor);
				var newLabel = new Label();
				layout.Children.Add(newLabel);
				Assert.Equal(Colors.Red, newLabel.BackgroundColor);
				Assert.Equal("Added", application.Resources["AddedResource"]);
				Assert.Equal("Unchanged", application.Resources["ExistingResource"]);
				Assert.Equal("Preserved", application.Resources["RuntimeResource"]);
				if (explicitDictionary)
					Assert.Equal("Unchanged", application.Resources["MergedResource"]);
			});
		}
		finally
		{
			Application.Current = previousApplication;
			AppContext.SetSwitch(IncrementalHotReloadSwitch, previousSwitch);
			ClearMainThreadImplementation();
		}
	}

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

	[MetadataUpdateFact]
	public void ClasslessSourceDictionaryUpdate_ReachesApplicationConsumers()
	{
		const string rootClass = "TestMaui37888.App";
		const string codeBehind = """
			namespace TestMaui37888;

			public partial class App
			{
				public App() => InitializeComponent();
			}
			""";
		const string appXaml = """
			<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestMaui37888.App">
			  <Application.Resources>
			    <ResourceDictionary>
			      <ResourceDictionary.MergedDictionaries>
			        <ResourceDictionary Source="AppResources.xaml" />
			      </ResourceDictionary.MergedDictionaries>
			    </ResourceDictionary>
			  </Application.Resources>
			</Application>
			""";

		static string ResourcesXaml(string value) => $$"""
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			  <x:String x:Key="{{ResourceKey}}">{{value}}</x:String>
			</ResourceDictionary>
			""";

		static IReadOnlyDictionary<string, XamlHotReloadDocument> Snapshot(string value) =>
			new Dictionary<string, XamlHotReloadDocument>(StringComparer.Ordinal)
			{
				["MainPage.xaml"] = new XamlHotReloadDocument(
					appXaml,
					ManifestResourceName: "TestMaui37888.App.xaml"),
				["AppResources.xaml"] = new XamlHotReloadDocument(
					ResourcesXaml(value),
					ManifestResourceName: "TestMaui37888.AppResources.xaml"),
			};

		AppContext.TryGetSwitch(IncrementalHotReloadSwitch, out var previousSwitch);
		var previousApplication = Application.Current;
		AppContext.SetSwitch(IncrementalHotReloadSwitch, true);
		SetMainThreadImplementation();

		try
		{
			using var harness = new XamlHotReloadTestHarness(
				nameof(ClasslessSourceDictionaryUpdate_ReachesApplicationConsumers),
				rootClass,
				codeBehind);
			var generation = harness.GenerateDocuments(
				Snapshot("App resource V0"),
				Snapshot("App resource V1"));

			Assert.Contains("SetAndCreateSource<global::__XamlGeneratedCode__.__Type", generation[0].InitializeComponentSource, StringComparison.Ordinal);
			var dictionaryUpdate = Assert.Single(
				generation[1].GeneratedRoots,
				static root => root.TypeName.StartsWith("__XamlGeneratedCode__.__Type", StringComparison.Ordinal));
			Assert.Contains("App resource V1", dictionaryUpdate.UpdateComponentSource, StringComparison.Ordinal);
			Assert.Contains($"this[\"{ResourceKey}\"]", dictionaryUpdate.UpdateComponentSource, StringComparison.Ordinal);

			harness.RunLive(generation, live =>
			{
				var application = live.GetInstance<Application>();
				Application.Current = application;
				VerifyConsumers(application, () => live.ApplyUpdateThroughRuntimeHandler<Application>(1));
			});
		}
		finally
		{
			Application.Current = previousApplication;
			AppContext.SetSwitch(IncrementalHotReloadSwitch, previousSwitch);
			ClearMainThreadImplementation();
		}
	}

	[Fact]
	public void NullConvertedResource_GuardsComponentRegistration()
	{
		const string rootClass = "TestMaui37888.NullResources";
		const string codeBehind = """
			using System;
			using System.ComponentModel;
			using System.Globalization;

			namespace TestMaui37888;

			[TypeConverter(typeof(NullResourceConverter))]
			public sealed class NullResource
			{
			}

			public sealed class NullResourceConverter : TypeConverter
			{
				public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
					sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

				public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
					null;
			}

			public partial class NullResources : global::Microsoft.Maui.Controls.ResourceDictionary
			{
				private partial void InitializeComponent();

				public NullResources() => InitializeComponent();
			}
			""";

		string Xaml(string value) => $$"""
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			                    xmlns:local="clr-namespace:TestMaui37888"
			                    x:Class="{{rootClass}}">
			  <local:NullResource x:Key="NullResource">{{value}}</local:NullResource>
			  <x:String x:Key="FollowingResource">{{value}}</x:String>
			</ResourceDictionary>
			""";

		using var harness = new XamlHotReloadTestHarness(
			nameof(NullConvertedResource_GuardsComponentRegistration),
			rootClass,
			codeBehind);
		var generation = harness.Generate(Xaml("V0"), Xaml("V1"));
		var updateComponentSource = generation[1].UpdateComponentSource;

		Assert.NotNull(updateComponentSource);
		Assert.Contains("if (__uc_0 is not null)", updateComponentSource, StringComparison.Ordinal);
		Assert.Contains("this[\"FollowingResource\"] = __uc_1;", updateComponentSource, StringComparison.Ordinal);
		harness.Compile(generation[1]);
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
