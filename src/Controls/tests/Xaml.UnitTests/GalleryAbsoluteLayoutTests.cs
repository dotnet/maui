using System;
using System.Globalization;
using Maui.Controls.Sample;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;
using static Microsoft.Maui.Controls.Xaml.UnitTests.GalleryTestHelpers;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Xaml Inflation")]
public class GalleryAbsoluteLayoutTests : BaseTestFixture
{
	[Theory]
	[InlineData("None", 100, 100, 100, 100, 100, 100, 100, 100)]
	[InlineData("None", 0, 0, 250, 500, 0, 0, 250, 500)]
	[InlineData("XProportional", 0.5, 0, 100, 100, 150, 0, 100, 100)]
	[InlineData("YProportional", 0, 0.5, 100, 100, 0, 250, 100, 100)]
	[InlineData("XProportional,YProportional", 0.5, 0.5, 100, 100, 150, 250, 100, 100)]
	[InlineData("PositionProportional", 0.5, 0.5, 100, 100, 150, 250, 100, 100)]
	[InlineData("WidthProportional", 0, 0, 0.5, 100, 0, 0, 200, 100)]
	[InlineData("HeightProportional", 0, 0, 100, 0.5, 0, 0, 100, 300)]
	[InlineData("WidthProportional,HeightProportional", 0, 0, 0.5, 0.5, 0, 0, 200, 300)]
	[InlineData("SizeProportional", 0, 0, 0.5, 0.5, 0, 0, 200, 300)]
	[InlineData("SizeProportional", 0, 0, 1, 1, 0, 0, 400, 600)]
	[InlineData("All", 0.5, 0.5, 0.5, 0.5, 100, 150, 200, 300)]
	[InlineData("PositionProportional,SizeProportional", 0.5, 0.5, 0.5, 0.5, 100, 150, 200, 300)]
	public void GalleryOptionsProduceExpectedArrangedBounds(string flags, double x, double y, double width, double height,
		double expectedX, double expectedY, double expectedWidth, double expectedHeight)
	{
		var model = new AbsoluteLayoutViewModel();
		var page = new AbsoluteLayoutControlMainPage(model);
		var options = new AbsoluteLayoutOptionsPage(model);
		SetBounds(options, x, y, width, height);
		foreach (var flag in flags.Split(','))
			Find<CheckBox>(options, $"LayoutFlag{flag}CheckBox").IsChecked = true;

		var layout = Assert.IsType<AbsoluteLayout>(page.Content);
		var box = Find<BoxView>(page, "BlueBox");
		Assert.Equal(new Rect(x, y, width, height), AbsoluteLayout.GetLayoutBounds(box));
		Assert.Equal(Enum.Parse<AbsoluteLayoutFlags>(flags), AbsoluteLayout.GetLayoutFlags(box));
		Arrange(layout, new Rect(0, 0, 400, 600));
		Assert.Equal(new Rect(expectedX, expectedY, expectedWidth, expectedHeight), box.Frame);
	}

	[Fact]
	public void ProportionalLayoutRecalculatesAfterResizeBoundsAndFlagsChange()
	{
		var model = new AbsoluteLayoutViewModel();
		var page = new AbsoluteLayoutControlMainPage(model);
		var options = new AbsoluteLayoutOptionsPage(model);
		var layout = Assert.IsType<AbsoluteLayout>(page.Content);
		var box = Find<BoxView>(page, "BlueBox");
		layout.Padding = new Thickness(8, 12, 16, 20);
		SetBounds(options, 0.5, 0.5, 100, 100);
		Find<CheckBox>(options, "LayoutFlagPositionProportionalCheckBox").IsChecked = true;

		Arrange(layout, new Rect(10, 20, 400, 600));
		Assert.Equal(new Rect(156, 266, 100, 100), box.Frame);
		Arrange(layout, new Rect(10, 20, 500, 700));
		Assert.Equal(new Rect(206, 316, 100, 100), box.Frame);

		Find<CheckBox>(options, "LayoutFlagNoneCheckBox").IsChecked = true;
		SetBounds(options, 30, 40, 80, 60);
		Arrange(layout, new Rect(10, 20, 500, 700));
		Assert.Equal(new Rect(48, 72, 80, 60), box.Frame);
	}

	static void SetBounds(AbsoluteLayoutOptionsPage options, double x, double y, double width, double height)
	{
		Find<Entry>(options, "XEntry").Text = x.ToString(CultureInfo.CurrentCulture);
		Find<Entry>(options, "YEntry").Text = y.ToString(CultureInfo.CurrentCulture);
		Find<Entry>(options, "WidthEntry").Text = width.ToString(CultureInfo.CurrentCulture);
		Find<Entry>(options, "HeightEntry").Text = height.ToString(CultureInfo.CurrentCulture);
	}

	static void Arrange(AbsoluteLayout layout, Rect bounds)
	{
		var manager = new AbsoluteLayoutManager(layout);
		manager.Measure(bounds.Width, bounds.Height);
		manager.ArrangeChildren(bounds);
	}
}
