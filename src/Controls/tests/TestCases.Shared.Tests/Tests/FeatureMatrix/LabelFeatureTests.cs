using Microsoft.Maui.Controls.Shapes;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class LabelFeatureTests : UITest
{
	public const string LabelFeatureMatrix = "Label Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string FormattedText = "FormattedText";
	public const string HorizontalTextStart = "HorizontalTextStart";
	public const string HorizontalTextCenter = "HorizontalTextCenter";
	public const string HorizontalTextEnd = "HorizontalTextEnd";
	public const string VerticalTextStart = "VerticalTextStart";
	public const string VerticalTextCenter = "VerticalTextCenter";
	public const string VerticalTextEnd = "VerticalTextEnd";
	public const string TextEntry = "TextEntry";
	public const string FontSizeEntry = "FontSizeEntry";
	public const string CharacterSpacingEntry = "CharacterSpacingEntry";
	public const string PaddingEntry = "PaddingEntry";
	public const string LineHeightEntry = "LineHeightEntry";
	public const string MaxLinesEntry = "MaxLinesEntry";
	public const string FontAutoScalingTrue = "FontAutoScalingTrue";
	public const string FontAutoScalingFalse = "FontAutoScalingFalse";
	public const string TextColorRed = "TextColorRed";
	public const string TextColorGreen = "TextColorGreen";
	public const string TextTypePlain = "TextTypePlain";
	public const string TextTypeHtml = "TextTypeHtml";
	public const string FontFamilyDokdo = "FontFamilyDokdo";
	public const string FontFamilyMontserratBold = "FontFamilyMontserratBold";
	public const string FontAttributesBold = "FontAttributesBold";
	public const string FontAttributesItalic = "FontAttributesItalic";
	public const string TextTransformUpper = "TextTransformUpper";
	public const string TextTransformLower = "TextTransformLower";
	public const string TextDecorationsLine = "TextDecorationsLine";
	public const string TextDecorationsStrike = "TextDecorationsStrike";
	public const string LineBreakModeWordWrap = "LineBreakModeWordWrap";
	public const string LineBreakModeCharacterWrap = "LineBreakModeCharacterWrap";
	public const string LineBreakModeHeadTruncation = "LineBreakModeHeadTruncation";
	public const string LineBreakModeTailTruncation = "LineBreakModeTailTruncation";
	public const string LineBreakModeMiddleTruncation = "LineBreakModeMiddleTruncation";
	public const string LineBreakModeNoWrap = "LineBreakModeNoWrap";
	public const string MainLabel = "MainLabel";

	public LabelFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(LabelFeatureMatrix);
	}

	[Test, Order(1)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedText()
	{
		App.WaitForElement("This is a Basic Label");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWhenFormattedTextWithHorizontalAndVerticalAlignmentStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalTextStart);
		App.Tap(HorizontalTextStart);
		App.WaitForElement(VerticalTextStart);
		App.Tap(VerticalTextStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(3)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenHorizontalAndVerticalAlignmentCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalTextCenter);
		App.Tap(HorizontalTextCenter);
		App.WaitForElement(VerticalTextCenter);
		App.Tap(VerticalTextCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(4)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenHorizontalAndVerticalAlignmentEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HorizontalTextEnd);
		App.Tap(HorizontalTextEnd);
		App.WaitForElement(VerticalTextEnd);
		App.Tap(VerticalTextEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(5)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextAndTextTransformLower()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(TextTransformLower);
		App.Tap(TextTransformLower);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(6)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenTextTransformUpper()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(TextTransformUpper);
		App.Tap(TextTransformUpper);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(7)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenTextColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(TextColorRed);
		App.Tap(TextColorRed);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(8)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWhenFormattedTextWithPadding()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(HorizontalTextStart);
		App.Tap(HorizontalTextStart);
		App.WaitForElement(VerticalTextStart);
		App.Tap(VerticalTextStart);
		App.WaitForElement(PaddingEntry);
		App.ClearText(PaddingEntry);
		App.EnterText(PaddingEntry, "20,20,20,20");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(9)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenMaxLines()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(MaxLinesEntry);
		App.ClearText(MaxLinesEntry);
		App.EnterText(MaxLinesEntry, "1");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/27828
	[Test, Order(10)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextAndLineHeight()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(LineHeightEntry);
		App.ClearText(LineHeightEntry);
		App.EnterText(LineHeightEntry, "3");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(11)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenTextDecorationsUnderLine()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(TextDecorationsLine);
		App.Tap(TextDecorationsLine);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(12)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenTextDecorationsStrikethrough()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(TextDecorationsStrike);
		App.Tap(TextDecorationsStrike);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/27828
	[Test, Order(13)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextAndCharacterSpacing()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(CharacterSpacingEntry);
		App.ClearText(CharacterSpacingEntry);
		App.EnterText(CharacterSpacingEntry, "3");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(14)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenFontAttributesBold()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(FontAttributesBold);
		App.Tap(FontAttributesBold);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(15)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenFontAttributesItalic()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(FontAttributesItalic);
		App.Tap(FontAttributesItalic);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(16)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenFontFamilyDokdo()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(FontFamilyDokdo);
		App.Tap(FontFamilyDokdo);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(17)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenFontFamilyMonserratBold()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(FontFamilyMontserratBold);
		App.Tap(FontFamilyMontserratBold);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}
#endif
#endif

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/27828
	[Test, Order(18)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenFontSize()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(FontSizeEntry);
		App.ClearText(FontSizeEntry);
		App.EnterText(FontSizeEntry, "24");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/22594, https://github.com/dotnet/maui/issues/21294
    [Test, Order(19)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextAndLineBreakModeCharacterWrap()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(LineBreakModeCharacterWrap);
		App.Tap(LineBreakModeCharacterWrap);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(42)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndLineBreakModeCharacterWrap()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea");
		App.WaitForElement(LineBreakModeCharacterWrap);
		App.Tap(LineBreakModeCharacterWrap);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/23159
	[Test, Order(20)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenLineBreakModeTailTruncation()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(LineBreakModeTailTruncation);
		App.Tap(LineBreakModeTailTruncation);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(41)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndLineBreakModeTailTruncation()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea");
		App.WaitForElement(LineBreakModeTailTruncation);
		App.Tap(LineBreakModeTailTruncation);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

	[Test, Order(21)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenLineBreakModeWordWrap()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(LineBreakModeWordWrap);
		App.Tap(LineBreakModeWordWrap);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(22)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextAndLineBreakModeNoWrap()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(LineBreakModeNoWrap);
		App.Tap(LineBreakModeNoWrap);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/21294
	[Test, Order(23)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenLineBreakModeHeadTruncation()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(LineBreakModeHeadTruncation);
		App.Tap(LineBreakModeHeadTruncation);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}

	[Test, Order(24)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFormattedTextWhenLineBreakModeMiddleTruncation()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FormattedText);
		App.Tap(FormattedText);
		App.WaitForElement(LineBreakModeMiddleTruncation);
		App.Tap(LineBreakModeMiddleTruncation);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MainLabel);
		VerifyScreenshot();
	}
#endif

	[Test, Order(25)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithText()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("This is a Label");
	}

	[Test, Order(26)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndFontSize()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label with FontSize");
		App.WaitForElement(FontSizeEntry);
		App.ClearText(FontSizeEntry);
		App.EnterText(FontSizeEntry, "24");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(27)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndFontColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label with TextColor");
		App.WaitForElement(TextColorGreen);
		App.Tap(TextColorGreen);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(28)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndTextTransform()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label with TextTransform");
		App.WaitForElement(TextTransformUpper);
		App.Tap(TextTransformUpper);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(29)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndCharacterSpacing()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label with CharacterSpacing");
		App.WaitForElement(CharacterSpacingEntry);
		App.ClearText(CharacterSpacingEntry);
		App.EnterText(CharacterSpacingEntry, "3");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(30)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndTextDecorations()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label with TextDecorations");
		App.WaitForElement(TextDecorationsLine);
		App.Tap(TextDecorationsLine);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(31)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndFontFamily()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label with FontFamily");
		App.WaitForElement(FontFamilyDokdo);
		App.Tap(FontFamilyDokdo);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(32)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndFontAttributes()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label with FontAttributes");
		App.WaitForElement(FontAttributesItalic);
		App.Tap(FontAttributesItalic);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(33)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextWhenVerticalAndHorizontalAlignmentStart()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(HorizontalTextStart);
		App.Tap(HorizontalTextStart);
		App.WaitForElement(VerticalTextStart);
		App.Tap(VerticalTextStart);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(34)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextWhenVerticalAndHorizontalAlignmentCenter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(HorizontalTextCenter);
		App.Tap(HorizontalTextCenter);
		App.WaitForElement(VerticalTextCenter);
		App.Tap(VerticalTextCenter);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(35)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextWhenVerticalAndHorizontalAlignmentEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(HorizontalTextEnd);
		App.Tap(HorizontalTextEnd);
		App.WaitForElement(VerticalTextEnd);
		App.Tap(VerticalTextEnd);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(36)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextWhenPaddingApplied()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(HorizontalTextStart);
		App.Tap(HorizontalTextStart);
		App.WaitForElement(VerticalTextStart);
		App.Tap(VerticalTextStart);
		App.WaitForElement(PaddingEntry);
		App.ClearText(PaddingEntry);
		App.EnterText(PaddingEntry, "20,20,20,20");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(37)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndMaxlines()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea");
		App.WaitForElement(MaxLinesEntry);
		App.ClearText(MaxLinesEntry);
		App.EnterText(MaxLinesEntry, "1");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(59)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextWhenLineHeight()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea");
		App.WaitForElement(LineHeightEntry);
		App.ClearText(LineHeightEntry);
		App.EnterText(LineHeightEntry, "2");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(38)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndLineBreakModeNoWrap()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea");
		App.WaitForElement(LineBreakModeNoWrap);
		App.Tap(LineBreakModeNoWrap);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/21294
	[Test, Order(39)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndLineBreakModeHeadTruncation()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea");
		App.WaitForElement(LineBreakModeHeadTruncation);
		App.Tap(LineBreakModeHeadTruncation);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(40)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndLineBreakModeMiddleTruncation()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "Lorem ipsum dolor sit amet consectetur adipiscing elit vivamus lacinia felis eu sagittis congue nibh urna malesuada orci at fringilla quam turpis eget nunc");
		App.WaitForElement(LineBreakModeMiddleTruncation);
		App.Tap(LineBreakModeMiddleTruncation);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

	[Test, Order(45)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFontAttributesAndFontFamily()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(FontFamilyMontserratBold);
		App.Tap(FontFamilyMontserratBold);
		App.WaitForElement(FontAttributesBold);
		App.Tap(FontAttributesBold);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(46)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFontAttributesAndFontSize()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(FontAttributesBold);
		App.Tap(FontAttributesBold);
		App.WaitForElement(FontSizeEntry);
		App.ClearText(FontSizeEntry);
		App.EnterText(FontSizeEntry, "24");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(47)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFontAttributesAndTextColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(FontAttributesBold);
		App.Tap(FontAttributesBold);
		App.WaitForElement(TextColorRed);
		App.Tap(TextColorRed);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(48)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFontAttributesAndTextTransform()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(FontAttributesItalic);
		App.Tap(FontAttributesItalic);
		App.WaitForElement(TextTransformLower);
		App.Tap(TextTransformLower);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(49)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFontAttributesAndTextDecorations()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(FontAttributesItalic);
		App.Tap(FontAttributesItalic);
		App.WaitForElement(TextDecorationsStrike);
		App.Tap(TextDecorationsStrike);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/20372,  https://github.com/dotnet/maui/issues/29672, https://github.com/dotnet/maui/issues/29668 
	[Test, Order(44)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextAndTextType()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Label</b>");
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(50)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFontAttributesAndTextType()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Label</b>");
		App.WaitForElement(FontAttributesItalic);
		App.Tap(FontAttributesItalic);
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(56)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextTypeAndTextColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Label</b>");
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(TextColorRed);
		App.Tap(TextColorRed);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(57)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextTypeAndFontSize()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Label</b>");
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(FontSizeEntry);
		App.ClearText(FontSizeEntry);
		App.EnterText(FontSizeEntry, "24");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(58)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextTypeAndLineBreakModeNoWrap()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliquan ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(LineBreakModeNoWrap);
		App.Tap(LineBreakModeNoWrap);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(43)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextTypeAndLineBreakModeWordWrap()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(LineBreakModeWordWrap);
		App.Tap(LineBreakModeWordWrap);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(60)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFontFamilyAndTextType()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Label</b>");
		App.WaitForElement(FontFamilyDokdo);
		App.Tap(FontFamilyDokdo);
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID// Issue Link:  https://github.com/dotnet/maui/issues/29672, https://github.com/dotnet/maui/issues/29668, https://github.com/dotnet/maui/issues/22594
	[Test, Order(67)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextTypeAndLineHeight()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(LineHeightEntry);
		App.ClearText(LineHeightEntry);
		App.EnterText(LineHeightEntry, "2");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(68)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextTypeAndCharacterSpacing()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>This is a Label</b>");
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(CharacterSpacingEntry);
		App.ClearText(CharacterSpacingEntry);
		App.EnterText(CharacterSpacingEntry, "3");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/21294
	[Test, Order(66)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextTypeAndLineBreakModeCharacterWrap()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(LineBreakModeCharacterWrap);
		App.Tap(LineBreakModeCharacterWrap);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif
#endif
#endif

	[Test, Order(51)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFontFamilyAndFontColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(FontFamilyMontserratBold);
		App.Tap(FontFamilyMontserratBold);
		App.WaitForElement(TextColorRed);
		App.Tap(TextColorRed);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(52)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFontFamilyAndFontSize()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(FontFamilyDokdo);
		App.Tap(FontFamilyDokdo);
		App.WaitForElement(FontSizeEntry);
		App.ClearText(FontSizeEntry);
		App.EnterText(FontSizeEntry, "22");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(53)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFontFamilyAndTextDecorations()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(FontFamilyDokdo);
		App.Tap(FontFamilyDokdo);
		App.WaitForElement(TextDecorationsStrike);
		App.Tap(TextDecorationsStrike);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(54)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithFontFamilyAndTextTransform()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(FontFamilyDokdo);
		App.Tap(FontFamilyDokdo);
		App.WaitForElement(TextTransformLower);
		App.Tap(TextTransformLower);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(55)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextDecorationsAndTextTransform()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "This is a Label");
		App.WaitForElement(TextDecorationsLine);
		App.Tap(TextDecorationsLine);
		App.WaitForElement(TextTransformUpper);
		App.Tap(TextTransformUpper);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/24298 , https://github.com/dotnet/maui/issues/29673, https://github.com/dotnet/maui/issues/29674
    [Test, Order(61)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextTypeAndLineBreakModeTailTruncation()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(LineBreakModeTailTruncation);
		App.Tap(LineBreakModeTailTruncation);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(62)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextTypeAndTextDecorations()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Label</b>");
		App.WaitForElement(TextDecorationsStrike);
		App.Tap(TextDecorationsStrike);
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/21294
	[Test, Order(63)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextTypeAndLineBreakModeHeadTruncation()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(LineBreakModeHeadTruncation);
		App.Tap(LineBreakModeHeadTruncation);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(64)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextTypeAndLineBreakModeMiddleTruncation()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea</b>");
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(LineBreakModeMiddleTruncation);
		App.Tap(LineBreakModeMiddleTruncation);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(65)]
	[Trait("Category", UITestCategories.Label)]
	public void VerifyLabelWithTextTypeAndTextTransform()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(TextEntry);
		App.ClearText(TextEntry);
		App.EnterText(TextEntry, "<b>Label</b>");
		App.WaitForElement(TextTransformUpper);
		App.Tap(TextTransformUpper);
		App.WaitForElement(TextTypeHtml);
		App.Tap(TextTypeHtml);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif
#endif
}