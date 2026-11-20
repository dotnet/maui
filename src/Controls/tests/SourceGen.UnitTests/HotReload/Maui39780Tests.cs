using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using Xunit;
using MauiResourceDictionary = Microsoft.Maui.Controls.ResourceDictionary;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;

[Collection("XamlHotReloadTests")]
public class Maui39780Tests
{
	const string DictionarySource = """
		namespace Test39780;

		public partial class Styles : global::Microsoft.Maui.Controls.ResourceDictionary
		{
			public Styles() => InitializeComponent();
		}
		""";

	static string DictionaryXaml(bool basedOn, bool shared = true) => $$"""
		<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		                    x:Class="Test39780.Styles">
		    <Style x:Key="BaseStyle" TargetType="Label">
		        <Setter Property="FontSize" Value="24" />
		    </Style>
		    <Style TargetType="{{(basedOn ? "{x:Type Label}" : "Label")}}" x:Shared="{{shared}}"{{(basedOn ? " BasedOn=\"{StaticResource BaseStyle}\"" : "")}}>
		        <Setter Property="TextColor" Value="Red" />
		        <Setter Property="BackgroundColor" Value="Red" />
		    </Style>
		</ResourceDictionary>
		""";

	[Fact]
	public void ImplicitStyleSetterPatch_ReplacesStyleResource()
	{
		using var harness = new XamlHotReloadTestHarness(nameof(ImplicitStyleSetterPatch_ReplacesStyleResource),
			"Test39780.Styles", DictionarySource);
		var xaml = DictionaryXaml(basedOn: true);
		var generation = harness.Generate(xaml, xaml.Replace("Red", "Blue", StringComparison.Ordinal));

		Assert.Contains("this.Remove(typeof(global::Microsoft.Maui.Controls.Label).FullName!);",
			generation[1].UpdateComponentSource, StringComparison.Ordinal);
		Assert.Contains("this.AddFactory(typeof(global::Microsoft.Maui.Controls.Label)", generation[1].UpdateComponentSource,
			StringComparison.Ordinal);
		harness.Compile(generation[1]);
	}

	[MetadataUpdateFact]
	public void ClasslessSourceDictionary_ImplicitStyleSetter_UpdatesExistingLabel()
	{
		const string source = """
			namespace Test39780;

			public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
			{
				public MainPage() => InitializeComponent();
			}
			""";
		const string pageXaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="Test39780.MainPage">
			    <ContentPage.Resources>
			        <ResourceDictionary>
			            <ResourceDictionary.MergedDictionaries>
			                <ResourceDictionary Source="Styles.xaml" />
			            </ResourceDictionary.MergedDictionaries>
			        </ResourceDictionary>
			    </ContentPage.Resources>
			    <Label Text="Styled" />
			</ContentPage>
			""";
		static IReadOnlyDictionary<string, XamlHotReloadDocument> Snapshot(string color) =>
			new Dictionary<string, XamlHotReloadDocument>(StringComparer.Ordinal)
			{
				["MainPage.xaml"] = new(pageXaml, ManifestResourceName: "Test39780.MainPage.xaml"),
				["Styles.xaml"] = new($$"""
					<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
					                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
					    <Style TargetType="Label">
					        <Setter Property="TextColor" Value="{{color}}" />
					    </Style>
					</ResourceDictionary>
					""", ManifestResourceName: "Test39780.Styles.xaml"),
			};

		const string featureSwitch = "Microsoft.Maui.RuntimeFeature.IsIncrementalHotReloadEnabled";
		AppContext.TryGetSwitch(featureSwitch, out var previousSwitch);
		var setMainThread = typeof(MainThread).GetMethod("SetCustomImplementation",
			BindingFlags.Static | BindingFlags.NonPublic, null,
			[typeof(Func<bool>), typeof(Action<Action>)], null);
		var clearMainThread = typeof(MainThread).GetMethod("ClearCustomImplementation",
			BindingFlags.Static | BindingFlags.NonPublic);
		Assert.NotNull(setMainThread);
		Assert.NotNull(clearMainThread);
		AppContext.SetSwitch(featureSwitch, true);
		setMainThread.Invoke(null, [() => true, (Action<Action>)(action => action())]);
		try
		{
			using var harness = new XamlHotReloadTestHarness(
				nameof(ClasslessSourceDictionary_ImplicitStyleSetter_UpdatesExistingLabel),
				"Test39780.MainPage", source);
			var generation = harness.GenerateDocuments(Snapshot("Red"), Snapshot("Blue"), Snapshot("Red"));

			harness.RunLive(generation, live =>
			{
				var page = live.GetInstance<ContentPage>();
				var label = Assert.IsType<Label>(page.Content);
				var dictionary = Assert.Single(page.Resources.MergedDictionaries);
				Assert.Equal(Colors.Red, label.TextColor);

				live.ApplyUpdateThroughRuntimeHandler<ContentPage>(1);
				Assert.Equal(Colors.Blue, label.TextColor);
				Assert.Same(label, page.Content);
				Assert.Same(dictionary, Assert.Single(page.Resources.MergedDictionaries));

				live.ApplyUpdateThroughRuntimeHandler<ContentPage>(2);
				Assert.Equal(Colors.Red, label.TextColor);
			});
		}
		finally
		{
			AppContext.SetSwitch(featureSwitch, previousSwitch);
			clearMainThread.Invoke(null, null);
		}
	}

	[MetadataUpdateTheory]
	[InlineData(false, true)]
	[InlineData(true, true)]
	[InlineData(false, false)]
	[InlineData(true, false)]
	public void ImplicitStyleSetter_InMergedDictionary_UpdatesExistingLabel(bool basedOn, bool shared)
	{
		using var harness = new XamlHotReloadTestHarness(nameof(ImplicitStyleSetter_InMergedDictionary_UpdatesExistingLabel),
			"Test39780.Styles", DictionarySource);
		var xaml = DictionaryXaml(basedOn, shared);
		var generation = harness.Generate(xaml, xaml.Replace("Red", "Blue", StringComparison.Ordinal),
			xaml.Replace("Red", "Green", StringComparison.Ordinal));

		harness.RunLive(generation, live =>
		{
			var dictionary = live.GetInstance<MauiResourceDictionary>();
			var label = new Label();
			var locallyStyledLabel = new Label { TextColor = Colors.Yellow };
			var layout = new VerticalStackLayout { Children = { label, locallyStyledLabel } };
			var page = new ContentPage { Content = layout };
			page.Resources.MergedDictionaries.Add(dictionary);
			Assert.Equal(Colors.Red, label.TextColor);
			var baseStyle = dictionary["BaseStyle"];
			var originalStyle = dictionary[typeof(Label).FullName!];
			if (shared)
				Assert.Same(originalStyle, dictionary[typeof(Label).FullName!]);
			else
				Assert.NotSame(originalStyle, dictionary[typeof(Label).FullName!]);

			Assert.Same(dictionary, live.ApplyUpdate<MauiResourceDictionary>(1));
			Assert.Equal(Colors.Blue, label.TextColor);
			Assert.Equal(Colors.Blue, label.BackgroundColor);
			var updatedStyle = Assert.IsType<Style>(dictionary[typeof(Label).FullName!]);
			Assert.All(updatedStyle.Setters, setter => Assert.Equal(Colors.Blue, Assert.IsType<Color>(setter.Value)));
			if (shared)
				Assert.Same(updatedStyle, dictionary[typeof(Label).FullName!]);
			else
				Assert.NotSame(updatedStyle, dictionary[typeof(Label).FullName!]);
			Assert.Same(baseStyle, dictionary["BaseStyle"]);
			if (basedOn)
				Assert.Equal(24, label.FontSize);
			Assert.Equal(Colors.Yellow, locallyStyledLabel.TextColor);
			Assert.Same(label, layout.Children[0]);

			var newLabel = new Label();
			layout.Children.Add(newLabel);
			Assert.Equal(Colors.Blue, newLabel.TextColor);

			live.ApplyUpdate<MauiResourceDictionary>(2);
			Assert.Equal(Colors.Green, label.TextColor);
			Assert.Equal(Colors.Green, newLabel.TextColor);
			Assert.Equal(Colors.Green, label.BackgroundColor);
			Assert.Equal(Colors.Yellow, locallyStyledLabel.TextColor);
		});
	}
}
