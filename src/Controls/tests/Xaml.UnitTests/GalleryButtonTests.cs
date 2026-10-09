using System.Globalization;
using Maui.Controls.Sample;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;
using Xunit;
using static Microsoft.Maui.Controls.Xaml.UnitTests.GalleryTestHelpers;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Xaml Inflation")]
public class GalleryButtonTests : BaseTestFixture
{
	[Theory]
	[InlineData(FontAttributes.Bold, null, 0, TextTransform.Default)]
	[InlineData(FontAttributes.Italic, null, 0, TextTransform.Default)]
	[InlineData(FontAttributes.Bold | FontAttributes.Italic, null, 0, TextTransform.Default)]
	[InlineData(FontAttributes.Bold, null, 0, TextTransform.Uppercase)]
	[InlineData(FontAttributes.None, "Dokdo", 0, TextTransform.Uppercase)]
	[InlineData(FontAttributes.None, null, 20, TextTransform.Default)]
	[InlineData(FontAttributes.None, null, 20, TextTransform.Uppercase)]
	[InlineData(FontAttributes.Bold, "MontserratBold", 22, TextTransform.Default)]
	public void FontAndTextOptionsUpdateTogetherAndReset(FontAttributes attributes, string family, double size, TextTransform transform)
	{
		var (page, options, button) = CreateGallery();
		Find<CheckBox>(options, "FontAttributesBold").IsChecked = attributes.HasFlag(FontAttributes.Bold);
		Find<CheckBox>(options, "FontAttributesItalic").IsChecked = attributes.HasFlag(FontAttributes.Italic);
		if (family != null)
			Find<RadioButton>(options, $"FontFamily{family}Button").IsChecked = true;
		Find<Entry>(options, "FontSizeEntry").Text = size.ToString(CultureInfo.InvariantCulture);
		Find<RadioButton>(options, $"TextTransform{transform}Button").IsChecked = true;
		Find<Entry>(options, "TextEntry").Text = "Button Font Combo";

		Assert.Equal(attributes, button.FontAttributes);
		Assert.Equal(family, button.FontFamily);
		Assert.Equal(size, button.FontSize);
		Assert.Equal(transform, button.TextTransform);
		Assert.Equal(transform == TextTransform.Uppercase ? "BUTTON FONT COMBO" : "Button Font Combo",
			button.UpdateFormsText(button.Text, button.TextTransform));

		page._viewModel.Reset();
		Assert.Equal(FontAttributes.None, button.FontAttributes);
		Assert.Null(button.FontFamily);
		Assert.Equal(0, button.FontSize);
		Assert.Equal(TextTransform.Default, button.TextTransform);
		Assert.Equal("Button", button.Text);
	}

	[Theory]
	[InlineData("Uppercase", TextTransform.Uppercase, "BUTTON TEXTTRANSFORM")]
	[InlineData("Lowercase", TextTransform.Lowercase, "button texttransform")]
	[InlineData("Default", TextTransform.Default, "Button TextTransform")]
	public void TextTransformOptionsPreserveSourceAndRestoreDefault(string option, TextTransform transform, string expected)
	{
		var (page, options, button) = CreateGallery();
		Find<Entry>(options, "TextEntry").Text = "Button TextTransform";
		Find<RadioButton>(options, $"TextTransform{option}Button").IsChecked = true;
		Assert.Equal("Button TextTransform", button.Text);
		Assert.Equal(transform, button.TextTransform);
		Assert.Equal(expected, button.UpdateFormsText(button.Text, button.TextTransform));
		page._viewModel.Reset();
		Assert.Equal("Button", button.UpdateFormsText(button.Text, button.TextTransform));
	}

	[Theory]
	[InlineData(100, -1)]
	[InlineData(-1, 200)]
	[InlineData(100, 200)]
	[InlineData(0, 0)]
	public void SizeOptionsUpdateFrameworkDimensionsAndReset(double height, double width)
	{
		var (page, options, button) = CreateGallery();
		Find<Entry>(options, "HeightRequestEntry").Text = height.ToString(CultureInfo.InvariantCulture);
		Find<Entry>(options, "WidthRequestEntry").Text = width.ToString(CultureInfo.InvariantCulture);
		Assert.Equal(height, button.HeightRequest);
		Assert.Equal(width, button.WidthRequest);
		Assert.Equal(height < 0 ? Dimension.Unset : height, ((IView)button).Height);
		Assert.Equal(width < 0 ? Dimension.Unset : width, ((IView)button).Width);
		page._viewModel.Reset();
		Assert.Equal(-1, button.HeightRequest);
		Assert.Equal(-1, button.WidthRequest);
		Assert.Equal(Dimension.Unset, ((IView)button).Height);
		Assert.Equal(Dimension.Unset, ((IView)button).Width);
	}

	[Theory]
	[InlineData("Red", 5, 20)]
	[InlineData("Green", 4, 16)]
	public void BorderAndTextColorOptionsUpdateTogetherAndReset(string borderColor, double width, int radius)
	{
		var (page, options, button) = CreateGallery();
		Click(options, $"BorderColor{borderColor}Button");
		Click(options, "TextColorGreenButton");
		Find<Entry>(options, "BorderWidthEntry").Text = width.ToString(CultureInfo.InvariantCulture);
		Find<Entry>(options, "CornerRadiusEntry").Text = radius.ToString(CultureInfo.InvariantCulture);
		Find<Entry>(options, "TextEntry").Text = "Button Border Combo";
		Assert.Equal(borderColor == "Red" ? Colors.Red : Colors.Green, button.BorderColor);
		Assert.Equal(Colors.Green, button.TextColor);
		Assert.Equal(width, button.BorderWidth);
		Assert.Equal(radius, button.CornerRadius);
		Assert.Equal("Button Border Combo", button.Text);
		page._viewModel.Reset();
		Assert.Null(button.BorderColor);
		Assert.Null(button.TextColor);
		Assert.Equal(0, button.BorderWidth);
		Assert.Equal(0, button.CornerRadius);
	}

	[Fact]
	public void CommandParameterAndEventsFollowBoundText()
	{
		var (page, options, button) = CreateGallery();
		button.SendPressed();
		button.SendReleased();
		button.SendClicked();
		Assert.Equal("Command Executed", button.Text);
		Assert.Equal("Pressed Event Executed", Find<Label>(page, "PressedEventLabel").Text);
		Assert.Equal("Released Event Executed", Find<Label>(page, "ReleasedEventLabel").Text);
		Assert.Equal("Clicked Event Executed", Find<Label>(page, "ClickedEventLabel").Text);

		page._viewModel.Reset();
		Find<Entry>(options, "TextEntry").Text = "Command with Parameter";
		Assert.Equal("Command with Parameter", button.CommandParameter);
		button.SendClicked();
		Assert.Equal("Command Executed with Parameter", button.Text);
	}

	static (ButtonControlMainPage Page, ButtonOptionsPage Options, Button Button) CreateGallery()
	{
		var page = new ButtonControlMainPage();
		page._viewModel.Reset();
		return (page, new ButtonOptionsPage(page._viewModel), Find<Button>(page, "ButtonControl"));
	}
}
