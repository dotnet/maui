// Material3 Label tests reuse the existing Label Feature Matrix HostApp page.
// The native Android view differs (MauiMaterialTextView vs MauiTextView), so these tests
// produce separate screenshot baselines under the Material3 category.
#if ANDROID
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class Material3LabelFeatureTests : _GalleryUITest
{
	public override string GalleryPageName => "Label Feature Matrix";

	public Material3LabelFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedText()
	{
		App.WaitForElement("This is a Basic Label");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWhenFormattedTextWithHorizontalAndVerticalAlignmentStart()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HorizontalTextStart");
		App.Tap("HorizontalTextStart");
		App.WaitForElement("VerticalTextStart");
		App.Tap("VerticalTextStart");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(3)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenHorizontalAndVerticalAlignmentCenter()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HorizontalTextCenter");
		App.Tap("HorizontalTextCenter");
		App.WaitForElement("VerticalTextCenter");
		App.Tap("VerticalTextCenter");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(4)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenHorizontalAndVerticalAlignmentEnd()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HorizontalTextEnd");
		App.Tap("HorizontalTextEnd");
		App.WaitForElement("VerticalTextEnd");
		App.Tap("VerticalTextEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(5)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextAndTextTransformLower()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("TextTransformLower");
		App.Tap("TextTransformLower");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(6)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenTextTransformUpper()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("TextTransformUpper");
		App.Tap("TextTransformUpper");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(7)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenTextColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(8)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWhenFormattedTextWithPadding()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("HorizontalTextStart");
		App.Tap("HorizontalTextStart");
		App.WaitForElement("VerticalTextStart");
		App.Tap("VerticalTextStart");
		App.WaitForElement("PaddingEntry");
		App.ClearText("PaddingEntry");
		App.EnterText("PaddingEntry", "20,20,20,20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(9)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenMaxLines()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("MaxLinesEntry");
		App.ClearText("MaxLinesEntry");
		App.EnterText("MaxLinesEntry", "1");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/27828
	[Test, Order(10)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextAndLineHeight()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("LineHeightEntry");
		App.ClearText("LineHeightEntry");
		App.EnterText("LineHeightEntry", "3");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(11)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenTextDecorationsUnderLine()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("TextDecorationsLine");
		App.Tap("TextDecorationsLine");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(12)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenTextDecorationsStrikethrough()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("TextDecorationsStrike");
		App.Tap("TextDecorationsStrike");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextAndCharacterSpacing()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("CharacterSpacingEntry");
		App.ClearText("CharacterSpacingEntry");
		App.EnterText("CharacterSpacingEntry", "3");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(14)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenFontAttributesBold()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(15)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenFontAttributesItalic()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(16)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenFontFamilyDokdo()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("FontFamilyDokdo");
		App.Tap("FontFamilyDokdo");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(17)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenFontFamilyMonserratBold()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("FontFamilyMontserratBold");
		App.Tap("FontFamilyMontserratBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(18)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenFontSize()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "24");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/22594, https://github.com/dotnet/maui/issues/21294
	[Test, Order(19)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextAndLineBreakModeCharacterWrap()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("LineBreakModeCharacterWrap");
		App.Tap("LineBreakModeCharacterWrap");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(41)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndLineBreakModeCharacterWrap()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea");
		App.WaitForElement("LineBreakModeCharacterWrap");
		App.Tap("LineBreakModeCharacterWrap");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(20)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenLineBreakModeTailTruncation()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("LineBreakModeTailTruncation");
		App.Tap("LineBreakModeTailTruncation");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(40)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndLineBreakModeTailTruncation()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea");
		App.WaitForElement("LineBreakModeTailTruncation");
		App.Tap("LineBreakModeTailTruncation");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(21)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenLineBreakModeWordWrap()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("LineBreakModeWordWrap");
		App.Tap("LineBreakModeWordWrap");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(22)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextAndLineBreakModeNoWrap()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("LineBreakModeNoWrap");
		App.Tap("LineBreakModeNoWrap");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(23)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenLineBreakModeHeadTruncation()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("LineBreakModeHeadTruncation");
		App.Tap("LineBreakModeHeadTruncation");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(24)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFormattedTextWhenLineBreakModeMiddleTruncation()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormattedText");
		App.Tap("FormattedText");
		App.WaitForElement("LineBreakModeMiddleTruncation");
		App.Tap("LineBreakModeMiddleTruncation");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(25)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndFontSize()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label with FontSize");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "24");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		App.Tap("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(26)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndFontColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label with TextColor");
		App.WaitForElement("TextColorGreen");
		App.Tap("TextColorGreen");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(27)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndTextTransform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label with TextTransform");
		App.WaitForElement("TextTransformUpper");
		App.Tap("TextTransformUpper");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(28)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndCharacterSpacing()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label with CharacterSpacing");
		App.WaitForElement("CharacterSpacingEntry");
		App.ClearText("CharacterSpacingEntry");
		App.EnterText("CharacterSpacingEntry", "3");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		App.Tap("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(29)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndTextDecorations()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label with TextDecorations");
		App.WaitForElement("TextDecorationsLine");
		App.Tap("TextDecorationsLine");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(30)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndFontFamily()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label with FontFamily");
		App.WaitForElement("FontFamilyDokdo");
		App.Tap("FontFamilyDokdo");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(31)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndFontAttributes()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label with FontAttributes");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(32)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextWhenVerticalAndHorizontalAlignmentStart()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("HorizontalTextStart");
		App.Tap("HorizontalTextStart");
		App.WaitForElement("VerticalTextStart");
		App.Tap("VerticalTextStart");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(33)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextWhenVerticalAndHorizontalAlignmentCenter()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("HorizontalTextCenter");
		App.Tap("HorizontalTextCenter");
		App.WaitForElement("VerticalTextCenter");
		App.Tap("VerticalTextCenter");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(34)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextWhenVerticalAndHorizontalAlignmentEnd()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("HorizontalTextEnd");
		App.Tap("HorizontalTextEnd");
		App.WaitForElement("VerticalTextEnd");
		App.Tap("VerticalTextEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(35)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextWhenPaddingApplied()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("HorizontalTextStart");
		App.Tap("HorizontalTextStart");
		App.WaitForElement("VerticalTextStart");
		App.Tap("VerticalTextStart");
		App.WaitForElement("PaddingEntry");
		App.ClearText("PaddingEntry");
		App.EnterText("PaddingEntry", "20,20,20,20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(36)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndMaxlines()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea");
		App.WaitForElement("MaxLinesEntry");
		App.ClearText("MaxLinesEntry");
		App.EnterText("MaxLinesEntry", "1");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		App.Tap("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(58)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextWhenLineHeight()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea");
		App.WaitForElement("LineHeightEntry");
		App.ClearText("LineHeightEntry");
		App.EnterText("LineHeightEntry", "2");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("MainLabel");
		App.Tap("MainLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(37)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndLineBreakModeNoWrap()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea");
		App.WaitForElement("LineBreakModeNoWrap");
		App.Tap("LineBreakModeNoWrap");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(38)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndLineBreakModeHeadTruncation()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea");
		App.WaitForElement("LineBreakModeHeadTruncation");
		App.Tap("LineBreakModeHeadTruncation");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(39)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndLineBreakModeMiddleTruncation()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Lorem ipsum dolor sit amet consectetur adipiscing elit vivamus lacinia felis eu sagittis congue nibh urna malesuada orci at fringilla quam turpis eget nunc");
		App.WaitForElement("LineBreakModeMiddleTruncation");
		App.Tap("LineBreakModeMiddleTruncation");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(44)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFontAttributesAndFontFamily()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("FontFamilyMontserratBold");
		App.Tap("FontFamilyMontserratBold");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(45)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFontAttributesAndFontSize()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "24");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(46)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFontAttributesAndTextColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(47)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFontAttributesAndTextTransform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("TextTransformLower");
		App.Tap("TextTransformLower");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(48)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFontAttributesAndTextDecorations()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("TextDecorationsStrike");
		App.Tap("TextDecorationsStrike");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(43)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextAndTextType()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Label</b>");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(49)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFontAttributesAndTextType()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Label</b>");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(55)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextTypeAndTextColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Label</b>");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(56)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextTypeAndFontSize()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Label</b>");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "24");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(57)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextTypeAndLineBreakModeNoWrap()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliquan ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("LineBreakModeNoWrap");
		App.Tap("LineBreakModeNoWrap");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(42)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextTypeAndLineBreakModeWordWrap()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("LineBreakModeWordWrap");
		App.Tap("LineBreakModeWordWrap");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(59)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFontFamilyAndTextType()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Label</b>");
		App.WaitForElement("FontFamilyDokdo");
		App.Tap("FontFamilyDokdo");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/29672, https://github.com/dotnet/maui/issues/29668, https://github.com/dotnet/maui/issues/22594
	[Test, Order(66)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextTypeAndLineHeight()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("LineHeightEntry");
		App.ClearText("LineHeightEntry");
		App.EnterText("LineHeightEntry", "2");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(67)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextTypeAndCharacterSpacing()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>This is a Label</b>");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("CharacterSpacingEntry");
		App.ClearText("CharacterSpacingEntry");
		App.EnterText("CharacterSpacingEntry", "3");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(65)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextTypeAndLineBreakModeCharacterWrap()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("LineBreakModeCharacterWrap");
		App.Tap("LineBreakModeCharacterWrap");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(50)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFontFamilyAndFontColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("FontFamilyMontserratBold");
		App.Tap("FontFamilyMontserratBold");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(51)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFontFamilyAndFontSize()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("FontFamilyDokdo");
		App.Tap("FontFamilyDokdo");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "22");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(52)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFontFamilyAndTextDecorations()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("FontFamilyDokdo");
		App.Tap("FontFamilyDokdo");
		App.WaitForElement("TextDecorationsStrike");
		App.Tap("TextDecorationsStrike");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(53)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithFontFamilyAndTextTransform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("FontFamilyDokdo");
		App.Tap("FontFamilyDokdo");
		App.WaitForElement("TextTransformLower");
		App.Tap("TextTransformLower");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(54)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextDecorationsAndTextTransform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "This is a Label");
		App.WaitForElement("TextDecorationsLine");
		App.Tap("TextDecorationsLine");
		App.WaitForElement("TextTransformUpper");
		App.Tap("TextTransformUpper");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/24298, https://github.com/dotnet/maui/issues/29673, https://github.com/dotnet/maui/issues/29674
	[Test, Order(60)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextTypeAndLineBreakModeTailTruncation()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("LineBreakModeTailTruncation");
		App.Tap("LineBreakModeTailTruncation");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(61)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextTypeAndTextDecorations()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Label</b>");
		App.WaitForElement("TextDecorationsStrike");
		App.Tap("TextDecorationsStrike");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(62)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextTypeAndLineBreakModeHeadTruncation()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("LineBreakModeHeadTruncation");
		App.Tap("LineBreakModeHeadTruncation");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(63)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextTypeAndLineBreakModeMiddleTruncation()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("LineBreakModeMiddleTruncation");
		App.Tap("LineBreakModeMiddleTruncation");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(64)]
	[Category(UITestCategories.Material3)]
	public void Material3Label_VerifyLabelWithTextTypeAndTextTransform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "<b>Label</b>");
		App.WaitForElement("TextTransformUpper");
		App.Tap("TextTransformUpper");
		App.WaitForElement("TextTypeHtml");
		App.Tap("TextTypeHtml");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif
}
#endif
