using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Xunit;
using MauiResourceDictionary = Microsoft.Maui.Controls.ResourceDictionary;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;

[Collection("XamlHotReloadTests")]
public class Maui39984Tests
{
	const string DictionarySource = """
		namespace Test39984;

		public partial class Styles : global::Microsoft.Maui.Controls.ResourceDictionary
		{
			public Styles() => InitializeComponent();
		}
		""";

	static string DictionaryXaml(string color, int fontSize = 24, bool shared = true) => $$"""
		<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		                    x:Class="Test39984.Styles">
		    <Style x:Key="BaseStyle" TargetType="Label">
		        <Setter Property="FontSize" Value="{{fontSize}}" />
		    </Style>
		    <Style x:Key="Chosen" TargetType="Label" x:Shared="{{shared}}" BasedOn="{StaticResource BaseStyle}">
		        <Setter Property="TextColor" Value="{{color}}" />
		        <Setter Property="BackgroundColor" Value="{{color}}" />
		    </Style>
		    <Style Class="Warning" TargetType="Label">
		        <Setter Property="TextColor" Value="{{color}}" />
		    </Style>
		</ResourceDictionary>
		""";

	static IReadOnlyDictionary<string, XamlHotReloadDocument> Snapshot(string xaml, bool lazy) =>
		new Dictionary<string, XamlHotReloadDocument>(StringComparer.Ordinal)
		{
			["MainPage.xaml"] = new(xaml, LazyRD: lazy),
		};

	static XamlHotReloadGeneration Generate(XamlHotReloadTestHarness harness, bool lazy, params string[] versions)
		=> harness.GenerateDocuments(Array.ConvertAll(versions, xaml => Snapshot(xaml, lazy)));

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void KeyedAndClassStyleSetterPatch_UpdatesExistingStyles(bool lazy)
	{
		using var harness = new XamlHotReloadTestHarness(nameof(KeyedAndClassStyleSetterPatch_UpdatesExistingStyles),
			"Test39984.Styles", DictionarySource);
		var generation = Generate(harness, lazy, DictionaryXaml("Red"), DictionaryXaml("Blue"));

		var update = generation[1].UpdateComponentSource;
		Assert.NotNull(update);
		Assert.Contains("XamlComponentRegistry.TryUpdateStyle(this, \"Chosen\",", update, StringComparison.Ordinal);
		Assert.Contains("XamlComponentRegistry.TryUpdateClassStyle(this,", update, StringComparison.Ordinal);
		Assert.DoesNotContain(".Value = \"Blue\"", update, StringComparison.Ordinal);
		harness.Compile(generation[1]);
	}

	[MetadataUpdateTheory]
	[InlineData(true)]
	[InlineData(false)]
	public void KeyedStyleSetter_UpdatesExistingStaticAndDynamicConsumers(bool lazy)
	{
		using var harness = new XamlHotReloadTestHarness(nameof(KeyedStyleSetter_UpdatesExistingStaticAndDynamicConsumers),
			"Test39984.Styles", DictionarySource);
		var generation = Generate(harness, lazy,
			DictionaryXaml("Red"), DictionaryXaml("Blue"), DictionaryXaml("Blue", fontSize: 30), DictionaryXaml("Green", fontSize: 30));

		harness.RunLive(generation, live =>
		{
			var dictionary = live.GetInstance<MauiResourceDictionary>();
			var style = Assert.IsType<Style>(dictionary["Chosen"]);
			var staticLabel = new Label { Style = style };
			var locallyStyledLabel = new Label { Style = style, TextColor = Colors.Yellow };
			var dynamicLabel = new Label();
			dynamicLabel.SetDynamicResource(VisualElement.StyleProperty, "Chosen");
			var layout = new VerticalStackLayout { Children = { staticLabel, locallyStyledLabel, dynamicLabel } };
			var page = new ContentPage { Content = layout };
			page.Resources.MergedDictionaries.Add(dictionary);
			Assert.Equal(Colors.Red, staticLabel.TextColor);
			Assert.Equal(Colors.Red, dynamicLabel.TextColor);

			Assert.Same(dictionary, live.ApplyUpdate<MauiResourceDictionary>(1));
			Assert.Same(style, dictionary["Chosen"]);
			Assert.All(style.Setters, setter => Assert.Equal(Colors.Blue, Assert.IsType<Color>(setter.Value)));
			foreach (var label in new[] { staticLabel, dynamicLabel })
			{
				Assert.Equal(Colors.Blue, label.TextColor);
				Assert.Equal(Colors.Blue, label.BackgroundColor);
				Assert.Equal(24, label.FontSize);
			}
			Assert.Equal(Colors.Yellow, locallyStyledLabel.TextColor);
			Assert.Equal(Colors.Blue, locallyStyledLabel.BackgroundColor);
			Assert.Same(staticLabel, layout.Children[0]);

			var newLabel = new Label { Style = (Style)dictionary["Chosen"] };
			layout.Children.Add(newLabel);
			Assert.Equal(Colors.Blue, newLabel.TextColor);

			// Editing the base style refreshes consumers of the derived style.
			live.ApplyUpdate<MauiResourceDictionary>(2);
			Assert.Equal(30, staticLabel.FontSize);
			Assert.Equal(30, dynamicLabel.FontSize);
			Assert.Equal(30, newLabel.FontSize);

			live.ApplyUpdate<MauiResourceDictionary>(3);
			Assert.Equal(Colors.Green, staticLabel.TextColor);
			Assert.Equal(Colors.Green, dynamicLabel.TextColor);
			Assert.Equal(Colors.Green, newLabel.TextColor);
			Assert.Equal(Colors.Yellow, locallyStyledLabel.TextColor);
		});
	}

	[MetadataUpdateTheory]
	[InlineData(true)]
	[InlineData(false)]
	public void ClassStyleSetter_UpdatesExistingConsumers(bool lazy)
	{
		using var harness = new XamlHotReloadTestHarness(nameof(ClassStyleSetter_UpdatesExistingConsumers),
			"Test39984.Styles", DictionarySource);
		var generation = Generate(harness, lazy, DictionaryXaml("Red"), DictionaryXaml("Blue"), DictionaryXaml("Green"));

		harness.RunLive(generation, live =>
		{
			var dictionary = live.GetInstance<MauiResourceDictionary>();
			var label = new Label { StyleClass = ["Warning"] };
			var page = new ContentPage { Content = new VerticalStackLayout { Children = { label } } };
			page.Resources.MergedDictionaries.Add(dictionary);
			Assert.Equal(Colors.Red, label.TextColor);

			live.ApplyUpdate<MauiResourceDictionary>(1);
			Assert.Equal(Colors.Blue, label.TextColor);

			var newLabel = new Label { StyleClass = ["Warning"] };
			((VerticalStackLayout)page.Content).Children.Add(newLabel);
			Assert.Equal(Colors.Blue, newLabel.TextColor);

			live.ApplyUpdate<MauiResourceDictionary>(2);
			Assert.Equal(Colors.Green, label.TextColor);
			Assert.Equal(Colors.Green, newLabel.TextColor);
		});
	}

	[MetadataUpdateTheory]
	[InlineData(true)]
	[InlineData(false)]
	public void UnresolvedOrUnsharedKeyedStyleSetter_UpdatesNewConsumers(bool shared)
	{
		using var harness = new XamlHotReloadTestHarness(nameof(UnresolvedOrUnsharedKeyedStyleSetter_UpdatesNewConsumers),
			"Test39984.Styles", DictionarySource);
		var generation = Generate(harness, lazy: true,
			DictionaryXaml("Red", shared: shared), DictionaryXaml("Blue", shared: shared), DictionaryXaml("Green", shared: shared));

		harness.RunLive(generation, live =>
		{
			var dictionary = live.GetInstance<MauiResourceDictionary>();

			live.ApplyUpdate<MauiResourceDictionary>(1);
			var style = Assert.IsType<Style>(dictionary["Chosen"]);
			var label = new Label { Style = style };
			Assert.Equal(Colors.Blue, label.TextColor);
			Assert.Equal(24, label.FontSize);
			if (shared)
				Assert.Same(style, dictionary["Chosen"]);
			else
				Assert.NotSame(style, dictionary["Chosen"]);

			live.ApplyUpdate<MauiResourceDictionary>(2);
			Assert.Equal(Colors.Green, new Label { Style = (Style)dictionary["Chosen"] }.TextColor);
		});
	}

	[MetadataUpdateTheory]
	[InlineData(true)]
	[InlineData(false)]
	public void EditingEarlierClassAndImplicitBasePreservesPrecedence(bool lazy)
	{
		static string Xaml(int size) => $$"""
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			                    x:Class="Test39984.Styles">
			    <Style x:Key="Base" TargetType="Label">
			        <Setter Property="TextColor" Value="Red" />
			        <Setter Property="FontSize" Value="{{size}}" />
			    </Style>
			    <Style TargetType="Label" BasedOn="{StaticResource Base}">
			        <Setter Property="TextColor" Value="Blue" />
			    </Style>
			    <Style Class="First" TargetType="Label">
			        <Setter Property="TextColor" Value="Red" />
			        <Setter Property="FontSize" Value="{{size}}" />
			    </Style>
			    <Style Class="Second" TargetType="Label">
			        <Setter Property="TextColor" Value="Blue" />
			    </Style>
			</ResourceDictionary>
			""";

		using var harness = new XamlHotReloadTestHarness(nameof(EditingEarlierClassAndImplicitBasePreservesPrecedence),
			"Test39984.Styles", DictionarySource);
		var generation = Generate(harness, lazy, Xaml(12), Xaml(20));
		harness.RunLive(generation, live =>
		{
			var dictionary = live.GetInstance<MauiResourceDictionary>();
			var implicitLabel = new Label();
			var classLabel = new Label { StyleClass = ["First", "Second"] };
			var layout = new VerticalStackLayout { Children = { implicitLabel, classLabel } };
			var page = new ContentPage { Content = layout };
			page.Resources.MergedDictionaries.Add(dictionary);
			Assert.Equal(Colors.Blue, implicitLabel.TextColor);
			Assert.Equal(Colors.Blue, classLabel.TextColor);

			live.ApplyUpdate<MauiResourceDictionary>(1);
			Assert.Equal(Colors.Blue, implicitLabel.TextColor);
			Assert.Equal(Colors.Blue, classLabel.TextColor);
			Assert.Equal(20, implicitLabel.FontSize);
			Assert.Equal(20, classLabel.FontSize);
			var newLabel = new Label { StyleClass = ["First", "Second"] };
			layout.Children.Add(newLabel);
			Assert.Equal(Colors.Blue, newLabel.TextColor);
		});
	}

	[MetadataUpdateTheory]
	[InlineData(true)]
	[InlineData(false)]
	public void InlineBaseAndDuplicateClassStylesUpdateTheCorrectDeclaration(bool lazy)
	{
		static string Xaml(string first, string second) => $$"""
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			                    x:Class="Test39984.Styles">
			    <Style x:Key="Chosen" TargetType="Label">
			        <Style.BasedOn><Style TargetType="Label"><Setter Property="FontSize" Value="17" /></Style></Style.BasedOn>
			        <Setter Property="TextColor" Value="{{first}}" />
			    </Style>
			    <Style Class="Warning" TargetType="Label">
			        <Style.BasedOn><Style TargetType="Label"><Setter Property="FontSize" Value="18" /></Style></Style.BasedOn>
			        <Setter Property="TextColor" Value="{{first}}" />
			    </Style>
			    <Style Class="Warning" TargetType="Label">
			        <Setter Property="TextColor" Value="{{second}}" />
			    </Style>
			</ResourceDictionary>
			""";

		using var harness = new XamlHotReloadTestHarness(nameof(InlineBaseAndDuplicateClassStylesUpdateTheCorrectDeclaration),
			"Test39984.Styles", DictionarySource);
		var generation = Generate(harness, lazy, Xaml("Red", "Yellow"), Xaml("Blue", "Yellow"), Xaml("Blue", "Green"));
		harness.RunLive(generation, live =>
		{
			var dictionary = live.GetInstance<MauiResourceDictionary>();
			var style = Assert.IsType<Style>(dictionary["Chosen"]);
			var oldBase = style.BasedOn;
			var keyed = new Label { Style = style };
			var classLabel = new Label { StyleClass = ["Warning"] };
			var layout = new VerticalStackLayout { Children = { keyed, classLabel } };
			var page = new ContentPage { Content = layout };
			page.Resources.MergedDictionaries.Add(dictionary);

			live.ApplyUpdate<MauiResourceDictionary>(1);
			Assert.Same(style, dictionary["Chosen"]);
			Assert.Same(oldBase, style.BasedOn);
			Assert.Equal(Colors.Blue, keyed.TextColor);
			Assert.Equal(17, keyed.FontSize);
			Assert.Equal(Colors.Blue, classLabel.TextColor);
			Assert.Equal(18, classLabel.FontSize);

			live.ApplyUpdate<MauiResourceDictionary>(2);
			Assert.Equal(Colors.Blue, classLabel.TextColor);
			var classStyles = Assert.IsAssignableFrom<IList<Style>>(dictionary["Microsoft.Maui.Controls.StyleClass.Warning"]);
			Assert.Equal(Colors.Blue, classStyles[0].Setters[0].Value);
			Assert.Equal(Colors.Green, classStyles[1].Setters[0].Value);
			var newLabel = new Label { StyleClass = ["Warning"] };
			layout.Children.Add(newLabel);
			Assert.Equal(Colors.Blue, newLabel.TextColor);
		});
	}

	[MetadataUpdateTheory]
	[InlineData(true)]
	[InlineData(false)]
	public void UnusedLazyStyleDoesNotResolveDependenciesWhenPatched(bool shared)
	{
		static string Xaml(int size, bool shared) => $$"""
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			                    x:Class="Test39984.Styles">
			    <Style x:Key="Chosen" TargetType="Label" x:Shared="{{shared}}">
			        <Setter Property="TextColor" Value="{StaticResource Later}" />
			        <Setter Property="FontSize" Value="{{size}}" />
			    </Style>
			</ResourceDictionary>
			""";
		using var harness = new XamlHotReloadTestHarness(nameof(UnusedLazyStyleDoesNotResolveDependenciesWhenPatched),
			"Test39984.Styles", DictionarySource);
		var generation = Generate(harness, true, Xaml(12, shared), Xaml(20, shared));
		harness.RunLive(generation, live =>
		{
			var dictionary = live.GetInstance<MauiResourceDictionary>();
			live.ApplyUpdate<MauiResourceDictionary>(1);
			dictionary.Add("Later", Colors.Blue);
			var label = new Label { Style = (Style)dictionary["Chosen"] };
			Assert.Equal(Colors.Blue, label.TextColor);
			Assert.Equal(20, label.FontSize);
		});
	}

	[MetadataUpdateTheory]
	[InlineData(true)]
	[InlineData(false)]
	public void SharedStyleBasedOnUnsharedResourceRetainsItsBase(bool lazy)
	{
		static string Xaml(string color) => $$"""
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			                    x:Class="Test39984.Styles">
			    <Style x:Key="Base" TargetType="Label" x:Shared="False">
			        <Setter Property="FontSize" Value="17" />
			    </Style>
			    <Style x:Key="Chosen" TargetType="Label" BasedOn="{StaticResource Base}">
			        <Setter Property="TextColor" Value="{{color}}" />
			    </Style>
			</ResourceDictionary>
			""";
		using var harness = new XamlHotReloadTestHarness(nameof(SharedStyleBasedOnUnsharedResourceRetainsItsBase),
			"Test39984.Styles", DictionarySource);
		var generation = Generate(harness, lazy, Xaml("Red"), Xaml("Blue"));
		harness.RunLive(generation, live =>
		{
			var dictionary = live.GetInstance<MauiResourceDictionary>();
			var style = Assert.IsType<Style>(dictionary["Chosen"]);
			var originalBase = style.BasedOn;
			var label = new Label { Style = style };
			Assert.True(dictionary.Remove("Base"));
			live.ApplyUpdate<MauiResourceDictionary>(1);
			Assert.Same(style, dictionary["Chosen"]);
			Assert.Same(originalBase, style.BasedOn);
			Assert.Equal(Colors.Blue, label.TextColor);
			Assert.Equal(17, label.FontSize);
		});
	}

	[MetadataUpdateFact]
	public void MissingClassStyleReportsUpdateFailure()
	{
		using var harness = new XamlHotReloadTestHarness(nameof(MissingClassStyleReportsUpdateFailure),
			"Test39984.Styles", DictionarySource);
		var generation = Generate(harness, false, DictionaryXaml("Red"), DictionaryXaml("Blue"));
		harness.RunLive(generation, live =>
		{
			var dictionary = live.GetInstance<MauiResourceDictionary>();
			Assert.True(dictionary.Remove("Microsoft.Maui.Controls.StyleClass.Warning"));
			var error = Assert.Throws<System.Reflection.TargetInvocationException>(() => live.ApplyUpdate<MauiResourceDictionary>(1));
			Assert.IsType<InvalidOperationException>(error.InnerException);
		});
	}

	[MetadataUpdateFact]
	public void MissingEagerKeyedStyleIsReplacedWithItsBase()
	{
		using var harness = new XamlHotReloadTestHarness(nameof(MissingEagerKeyedStyleIsReplacedWithItsBase),
			"Test39984.Styles", DictionarySource);
		var generation = Generate(harness, false, DictionaryXaml("Red"), DictionaryXaml("Blue"));
		harness.RunLive(generation, live =>
		{
			var dictionary = live.GetInstance<MauiResourceDictionary>();
			Assert.True(dictionary.Remove("Chosen"));
			live.ApplyUpdate<MauiResourceDictionary>(1);
			var label = new Label { Style = (Style)dictionary["Chosen"] };
			Assert.Equal(Colors.Blue, label.TextColor);
			Assert.Equal(24, label.FontSize);
		});
	}
}
