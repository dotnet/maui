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
		Assert.Contains("XamlComponentRegistry.TryUpdateStyle(this, null,", update, StringComparison.Ordinal);
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
			Assert.Equal(Colors.Blue, new Label { Style = style }.TextColor);
			if (shared)
				Assert.Same(style, dictionary["Chosen"]);
			else
				Assert.NotSame(style, dictionary["Chosen"]);

			live.ApplyUpdate<MauiResourceDictionary>(2);
			Assert.Equal(Colors.Green, new Label { Style = (Style)dictionary["Chosen"] }.TextColor);
		});
	}
}
