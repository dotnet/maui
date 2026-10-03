using System;
using Maui.Controls.Sample;
using Microsoft.Maui.Graphics;
using Xunit;
using static Microsoft.Maui.Controls.Xaml.UnitTests.GalleryTestHelpers;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Xaml Inflation")]
public class GallerySafeAreaTests : BaseTestFixture
{
	[Theory]
	[InlineData("ContentPage")]
	[InlineData("ContentView")]
	[InlineData("Border")]
	[InlineData("Grid")]
	public void BackgroundAndPaddingOptionsPreserveSafeAreaBindings(string kind)
	{
		var model = new SafeAreaViewModel();
		var (page, target) = CreatePage(kind, model);
		var options = new SafeAreaOptionsPage(model);
		Find<RadioButton>(options, "UniformNone").IsChecked = true;
		var background = Find<CheckBox>(options, "BackgroundCheckBox");
		var padding = Find<CheckBox>(options, "PaddingCheckBox");

		for (var i = 0; i < 2; i++)
		{
			background.IsChecked = true;
			padding.IsChecked = true;
			Assert.Equal(SafeAreaEdges.None, ((ISafeAreaElement)target).SafeAreaEdges);
			Assert.Equal("None", Find<Label>(page, "SafeAreaEdgesValueLabel").Text);
			Assert.Equal(new Thickness(20), ((IPadding)target).Padding);
			var brush = Assert.IsType<LinearGradientBrush>(target.Background);
			Assert.Same(model.Background, brush);
			Assert.Collection(brush.GradientStops,
				stop => { Assert.Equal(Colors.LightBlue, stop.Color); Assert.Equal(0, stop.Offset); },
				stop => { Assert.Equal(Colors.LightPink, stop.Color); Assert.Equal(1, stop.Offset); });

			background.IsChecked = false;
			padding.IsChecked = false;
			Assert.Null(target.Background);
			Assert.Equal(Thickness.Zero, ((IPadding)target).Padding);
			Assert.Equal(SafeAreaEdges.None, ((ISafeAreaElement)target).SafeAreaEdges);
		}
	}

	[Theory]
	[InlineData("ContentPage")]
	[InlineData("ContentView")]
	[InlineData("Border")]
	[InlineData("Grid")]
	public void UniformAndPerEdgeChangesUpdateRealGalleryControls(string kind)
	{
		var model = new SafeAreaViewModel();
		var (page, target) = CreatePage(kind, model);
		var options = new SafeAreaOptionsPage(model);

		foreach (var region in Enum.GetValues<SafeAreaRegions>())
		{
			Find<RadioButton>(options, $"Uniform{region}").IsChecked = true;
			Assert.Equal(new SafeAreaEdges(region), ((ISafeAreaElement)target).SafeAreaEdges);
			Assert.Equal(region.ToString(), Find<Label>(page, "SafeAreaEdgesValueLabel").Text);
		}

		Click(page, "SafeAreaNoneButton");
		Find<RadioButton>(options, "LeftContainer").IsChecked = true;
		Find<RadioButton>(options, "TopSoftInput").IsChecked = true;
		Find<RadioButton>(options, "RightAll").IsChecked = true;
		Find<RadioButton>(options, "BottomNone").IsChecked = true;
		Assert.Equal(new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.SoftInput, SafeAreaRegions.All, SafeAreaRegions.None),
			((ISafeAreaElement)target).SafeAreaEdges);
		Assert.Equal("L:Container, T:SoftInput, R:All, B:None", Find<Label>(page, "SafeAreaEdgesValueLabel").Text);

		Click(page, "SafeAreaNoneButton");
		Assert.Equal(SafeAreaEdges.None, ((ISafeAreaElement)target).SafeAreaEdges);
		Assert.Equal("None", Find<Label>(page, "SafeAreaEdgesValueLabel").Text);
	}

	static (ContentPage Page, VisualElement Target) CreatePage(string kind, SafeAreaViewModel model)
	{
		ContentPage page = kind switch
		{
			"ContentPage" => new SafeAreaContentPage(model),
			"ContentView" => new SafeAreaContentViewPage(model),
			"Border" => new SafeAreaBorderPage(model),
			"Grid" => new SafeAreaGridPage(model),
			_ => throw new ArgumentOutOfRangeException(nameof(kind))
		};
		return (page, kind == "ContentPage" ? page : page.Content);
	}
}
