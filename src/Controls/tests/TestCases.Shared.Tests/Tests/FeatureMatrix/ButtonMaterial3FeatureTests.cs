#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class ButtonMaterial3FeatureTests : _GalleryUITest
{
	public override string GalleryPageName => "Button Feature Matrix";

	public ButtonMaterial3FeatureTests(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetBorderColorAndTextColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BorderColorRedButton");
		App.Tap("BorderColorRedButton");
		App.WaitForElement("BorderWidthEntry");
		App.ClearText("BorderWidthEntry");
		App.EnterText("BorderWidthEntry", "5");
		App.WaitForElement("TextColorGreenButton");
		App.Tap("TextColorGreenButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "Button Text");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetBorderWidthAndLineBreakMode()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BorderWidthEntry");
		App.ClearText("BorderWidthEntry");
		App.EnterText("BorderWidthEntry", "5");
		App.WaitForElement("LineBreakModeCharacterWrapButton");
		App.Tap("LineBreakModeCharacterWrapButton");
		App.WaitForElement("TextEntry");
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetCharacterSpacingAndText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacingEntry");
		App.ClearText("CharacterSpacingEntry");
		App.EnterText("CharacterSpacingEntry", "5");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "ButtonText");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		App.WaitForElement("ClickedEventLabel");
		App.Tap("ClickedEventLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetCornerRadiusAndBorderWidth()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CornerRadiusEntry");
		App.ClearText("CornerRadiusEntry");
		App.EnterText("CornerRadiusEntry", "20");
		App.WaitForElement("BorderWidthEntry");
		App.ClearText("BorderWidthEntry");
		App.EnterText("BorderWidthEntry", "5");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "Button Text");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		App.WaitForElement("ClickedEventLabel");
		App.Tap("ClickedEventLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetFontAttributesAndFontFamily()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBoldButton");
		App.Tap("FontAttributesBoldButton");
		App.WaitForElement("FontFamilyMontserratBoldButton");
		App.Tap("FontFamilyMontserratBoldButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "Button Text");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetFontAttributesAndText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBoldButton");
		App.Tap("FontAttributesBoldButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "Button Text");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetFontAttributesAndTextTransform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBoldButton");
		App.Tap("FontAttributesBoldButton");
		App.WaitForElement("TextTransformUppercaseButton");
		App.Tap("TextTransformUppercaseButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "Button Text");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetFontFamilyAndText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "Button Text");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetFontFamilyAndTextTransform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("TextTransformUppercaseButton");
		App.Tap("TextTransformUppercaseButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "Button Text");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetFontSizeAndLineBreakMode()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("LineBreakModeCharacterWrapButton");
		App.Tap("LineBreakModeCharacterWrapButton");
		App.WaitForElement("TextEntry");
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetFontSizeAndText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "Button Text");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		App.WaitForElement("ClickedEventLabel");
		App.Tap("ClickedEventLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetFontSizeAndTextTransform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("TextTransformUppercaseButton");
		App.Tap("TextTransformUppercaseButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "Button Text");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetLineBreakModeCharacterWrap()
	{
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("LineBreakModeCharacterWrapButton");
		App.Tap("LineBreakModeCharacterWrapButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetLineBreakModeHeadTruncation()
	{
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("LineBreakModeHeadTruncationButton");
		App.Tap("LineBreakModeHeadTruncationButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetLineBreakModeMiddleTruncation()
	{
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("LineBreakModeMiddleTruncationButton");
		App.Tap("LineBreakModeMiddleTruncationButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetLineBreakModeTailTruncation()
	{
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("LineBreakModeTailTruncationButton");
		App.Tap("LineBreakModeTailTruncationButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetLineBreakModeWordWrap()
	{
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("LineBreakModeWordWrapButton");
		App.Tap("LineBreakModeWordWrapButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetPaddingAndText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PaddingEntry");
		App.ClearText("PaddingEntry");
		App.EnterText("PaddingEntry", "5");
		App.WaitForElement("BorderWidthEntry");
		App.ClearText("BorderWidthEntry");
		App.EnterText("BorderWidthEntry", "5");
		App.WaitForElement("BorderColorGreenButton");
		App.Tap("BorderColorGreenButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "Button Text");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetShadowAndText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowTrueButton");
		App.Tap("ShadowTrueButton");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "Shadow Text");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void VerifyMaterial3Button_SetTextAndTextColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "Button Text");
		App.WaitForElement("TextColorGreenButton");
		App.Tap("TextColorGreenButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
