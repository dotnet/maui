using System;
using Microsoft.Maui.Platform;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Button)]
public class ButtonFeatureTests : _GalleryUITest
{
	public const string ButtonFeatureMatrix = "Button Feature Matrix";

	public override string GalleryPageName => ButtonFeatureMatrix;

	public ButtonFeatureTests(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test, Order(1)]
	public void Button_SetBorderColorAndTextColor_VerifyVisualState()
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

	[Test, Order(2)]
	public void Button_SetBorderWidthEntryAndLineBreakMode_VerifyVisualState()
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

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //CharacterSpacingEntry property not working on iOS and Catalyst, Issue: https://github.com/dotnet/maui/issues/21488
	[Test, Order(3)]
	public void Button_SetCharacterSpacingAndText_VerifyVisualState()
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
#endif

	[Test, Order(4)]
	public void Button_SetCommandAndCommandParameter()
	{
		App.WaitForElement("ButtonControl");
		App.Tap("ButtonControl");
		Assert.That(App.FindElement("ButtonControl").GetText(), Is.EqualTo("Command Executed"));
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Command with Parameter");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		App.Tap("ButtonControl");
		Assert.That(App.FindElement("ButtonControl").GetText(), Is.EqualTo("Command Executed with Parameter"));
	}

	[Test, Order(5)]
	public void Button_SetCornerRadiusAndBorderWidth_VerifyVisualState()
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

	[Test, Order(6)]
	public void Button_VerifiesAllEventHandlersExecute()
	{
		App.WaitForElement("ButtonControl");
		App.Tap("ButtonControl");
		Assert.That(App.FindElement("ClickedEventLabel").GetText(), Is.EqualTo("Clicked Event Executed"));
		Assert.That(App.FindElement("PressedEventLabel").GetText(), Is.EqualTo("Pressed Event Executed"));
		Assert.That(App.FindElement("ReleasedEventLabel").GetText(), Is.EqualTo("Released Event Executed"));
	}

	[Test, Order(7)]
	public void Button_setFontAttributesAndFontFamily_VerifyVisualState()
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

	[Test, Order(8)]
	public void Button_setFontAttributesAndText_VerifyVisualState()
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

	[Test, Order(9)]
	public void Button_setFontAttributesAndTextTransform_VerifyVisualState()
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

	[Test, Order(10)]
	public void Button_setFontFamilyAndText_VerifyVisualState()
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

	[Test, Order(11)]
	public void Button_setFontFamilyAndTextTransform_VerifyVisualState()
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

	[Test, Order(12)]
	public void Button_setFontSizeAndLineBreakMode_VerifyVisualState()
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

	[Test, Order(13)]
	public void Button_setFontSizeAndText_VerifyVisualState()
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

	[Test, Order(14)]
	public void Button_setFontSizeAndTextTransform_VerifyVisualState()
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

	[Test, Order(15)]
	public void Button_SetIsEnabledFalse_VerifyButtonState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledFalseButton");
		App.Tap("IsEnabledFalseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		App.WaitForElement("ButtonControl");
		App.Tap("ButtonControl");
		Assert.That(App.FindElement("ClickedEventLabel").GetText(), Is.EqualTo(string.Empty));
	}

	[Test, Order(16)]
	public void Button_SetIsVisibleFalse_VerifyButtonState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseButton");
		App.Tap("IsVisibleFalseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("ButtonControl");
	}

	[Test, Order(17)]
	public void Button_SetLineBreakModeCharacterWrap_VerifyVisualState()
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

	[Test, Order(18)]
	public void Button_SetLineBreakModeHeadTruncation_VerifyVisualState()
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

	[Test, Order(19)]
	public void Button_SetLineBreakModeMiddleTruncation_VerifyVisualState()
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

	[Test, Order(20)]
	public void Button_SetLineBreakModeTailTruncation_VerifyVisualState()
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

	[Test, Order(21)]
	public void Button_SetLineBreakModeWordWrap_VerifyVisualState()
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

	[Test, Order(22)]
	public void Button_setPaddingAndText_VerifyVisualState()
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

	[Test, Order(23)]
	public void Button_setShadowAndText_VerifyVisualState()
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

	[Test, Order(24)]
	public void Button_setTextAndTextColor_VerifyVisualState()
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

	[Test, Order(25)]
	public void Button_setTextAndTextTransform_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.EnterText("TextEntry", "ButtonText");
		App.WaitForElement("TextTransformUppercaseButton");
		App.Tap("TextTransformUppercaseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		Assert.That(App.FindElement("ButtonControl").GetText(), Is.EqualTo("BUTTONTEXT"));
	}
}
