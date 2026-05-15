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
	public void VerifyButton_CommandAndCommandParameter()
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

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //CharacterSpacingEntry property not working on iOS and Catalyst, Issue: https://github.com/dotnet/maui/issues/21488
	[Test, Order(2)]
	public void VerifyButton_CharacterSpacingAndText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacingEntry");
		App.ClearText("CharacterSpacingEntry");
		App.EnterText("CharacterSpacingEntry", "5");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button CharacterSpacing");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		App.WaitForElement("ClickedEventLabel");
		App.Tap("ClickedEventLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(3)]
	public void VerifyButton_BorderWidthAndLineBreakMode()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BorderWidthEntry");
		App.ClearText("BorderWidthEntry");
		App.EnterText("BorderWidthEntry", "5");
		App.WaitForElement("LineBreakModeCharacterWrapButton");
		App.Tap("LineBreakModeCharacterWrapButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(4)]
	public void VerifyButton_CornerRadiusAndBorderWidth()
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
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button CornerRadius BorderWidth");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		App.WaitForElement("ClickedEventLabel");
		App.Tap("ClickedEventLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(5)]
	public void VerifyButton_AllEventHandlersExecute()
	{
		App.WaitForElement("ButtonControl");
		App.Tap("ButtonControl");
		Assert.That(App.FindElement("ClickedEventLabel").GetText(), Is.EqualTo("Clicked Event Executed"));
		Assert.That(App.FindElement("PressedEventLabel").GetText(), Is.EqualTo("Pressed Event Executed"));
		Assert.That(App.FindElement("ReleasedEventLabel").GetText(), Is.EqualTo("Released Event Executed"));
	}

	[Test, Order(6)]
	public void VerifyButton_FontAttributesAndFontFamily()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("FontFamilyMontserratBoldButton");
		App.Tap("FontFamilyMontserratBoldButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button FontAttributes FontFamily");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(7)]
	public void VerifyButton_FontAttributesBoldAndText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button FontAttributes Bold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(8)]
	public void VerifyButton_FontAttributesBoldAndTextTransform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("TextTransformUppercaseButton");
		App.Tap("TextTransformUppercaseButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button FontAttributes Bold TextTransform");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(9)]
	public void VerifyButton_FontFamilyAndText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button FontFamily");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(10)]
	public void VerifyButton_FontFamilyAndTextTransform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("TextTransformUppercaseButton");
		App.Tap("TextTransformUppercaseButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button FontFamily TextTransform");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(11)]
	public void VerifyButton_FontSizeAndLineBreakMode()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("LineBreakModeCharacterWrapButton");
		App.Tap("LineBreakModeCharacterWrapButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(12)]
	public void VerifyButton_FontSizeAndText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button FontSize");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		App.WaitForElement("ClickedEventLabel");
		App.Tap("ClickedEventLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
	public void VerifyButton_FontSizeAndTextTransform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("TextTransformUppercaseButton");
		App.Tap("TextTransformUppercaseButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button Text");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(14)]
	public void VerifyButton_IsEnabledFalse()
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

	[Test, Order(15)]
	public void VerifyButton_IsVisibleFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseButton");
		App.Tap("IsVisibleFalseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("ButtonControl");
	}

	[Test, Order(16)]
	public void VerifyButton_LineBreakModeCharacterWrap()
	{
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("LineBreakModeCharacterWrapButton");
		App.Tap("LineBreakModeCharacterWrapButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(17)]
	public void VerifyButton_LineBreakModeHeadTruncation()
	{
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("LineBreakModeHeadTruncationButton");
		App.Tap("LineBreakModeHeadTruncationButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(18)]
	public void VerifyButton_LineBreakModeMiddleTruncation()
	{
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("LineBreakModeMiddleTruncationButton");
		App.Tap("LineBreakModeMiddleTruncationButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(19)]
	public void VerifyButton_LineBreakModeTailTruncation()
	{
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("LineBreakModeTailTruncationButton");
		App.Tap("LineBreakModeTailTruncationButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(20)]
	public void VerifyButton_LineBreakModeWordWrap()
	{
		string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("LineBreakModeWordWrapButton");
		App.Tap("LineBreakModeWordWrapButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", longText);
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(21)]
	public void VerifyButton_PaddingAndText()
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
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button Padding");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(22)]
	public void VerifyButton_ShadowAndText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowTrueButton");
		App.Tap("ShadowTrueButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button Shadow");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(23)]
	public void VerifyButton_TextAndTextTransform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button TextTransform");
		App.WaitForElement("TextTransformUppercaseButton");
		App.Tap("TextTransformUppercaseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		Assert.That(App.FindElement("ButtonControl").GetText(), Is.EqualTo("BUTTON TEXTTRANSFORM"));
	}

	[Test, Order(24)]
	public void VerifyButton_FontAttributesItalicAndText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button FontAttributes Italic");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(25)]
	public void VerifyButton_FontAttributesBoldItalicAndText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button FontAttributes BoldItalic");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(26)]
	public void VerifyButton_HeightRequest()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HeightRequestEntry");
		App.ClearText("HeightRequestEntry");
		App.EnterText("HeightRequestEntry", "100");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button HeightRequest");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(27)]
	public void VerifyButton_WidthRequest()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WidthRequestEntry");
		App.ClearText("WidthRequestEntry");
		App.EnterText("WidthRequestEntry", "200");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button WidthRequest");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(28)]
	public void VerifyButton_HeightRequestAndWidthRequest()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HeightRequestEntry");
		App.ClearText("HeightRequestEntry");
		App.EnterText("HeightRequestEntry", "100");
		App.WaitForElement("WidthRequestEntry");
		App.ClearText("WidthRequestEntry");
		App.EnterText("WidthRequestEntry", "200");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button HeightRequest WidthRequest");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(29)]
	public void VerifyButton_ContentLayoutLeftWithImage()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageSourceDotnetBotButton");
		App.Tap("ImageSourceDotnetBotButton");
		App.WaitForElement("ContentLayoutLeftButton");
		App.Tap("ContentLayoutLeftButton");
		App.WaitForElement("ContentLayoutSpacingEntry");
		App.ClearText("ContentLayoutSpacingEntry");
		App.EnterText("ContentLayoutSpacingEntry", "20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(30)]
	public void VerifyButton_ContentLayoutTopWithImage()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageSourceDotnetBotButton");
		App.Tap("ImageSourceDotnetBotButton");
		App.WaitForElement("ContentLayoutTopButton");
		App.Tap("ContentLayoutTopButton");
		App.WaitForElement("ContentLayoutSpacingEntry");
		App.ClearText("ContentLayoutSpacingEntry");
		App.EnterText("ContentLayoutSpacingEntry", "20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(31)]
	public void VerifyButton_ContentLayoutRightWithImage()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageSourceDotnetBotButton");
		App.Tap("ImageSourceDotnetBotButton");
		App.WaitForElement("ContentLayoutRightButton");
		App.Tap("ContentLayoutRightButton");
		App.WaitForElement("ContentLayoutSpacingEntry");
		App.ClearText("ContentLayoutSpacingEntry");
		App.EnterText("ContentLayoutSpacingEntry", "20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(32)]
	public void VerifyButton_ContentLayoutBottomWithImage()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageSourceDotnetBotButton");
		App.Tap("ImageSourceDotnetBotButton");
		App.WaitForElement("ContentLayoutBottomButton");
		App.Tap("ContentLayoutBottomButton");
		App.WaitForElement("ContentLayoutSpacingEntry");
		App.ClearText("ContentLayoutSpacingEntry");
		App.EnterText("ContentLayoutSpacingEntry", "20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(33)]
	public void VerifyButton_BackgroundColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorRed");
		App.Tap("BackgroundColorRed");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button Background");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(34)]
	public void VerifyButton_FlowDirectionRightToLeft()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeftButton");
		App.Tap("FlowDirectionRightToLeftButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button RTL");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(35)]
	public void VerifyButton_ImageSourceOnly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageSourceDotnetBotButton");
		App.Tap("ImageSourceDotnetBotButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(36)]
	public void VerifyButton_ClearImageSource()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageSourceDotnetBotButton");
		App.Tap("ImageSourceDotnetBotButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageSourceNoneButton");
		App.Tap("ImageSourceNoneButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(37)]
	public void VerifyButton_ImageWithCornerRadius()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageSourceDotnetBotButton");
		App.Tap("ImageSourceDotnetBotButton");
		App.WaitForElement("CornerRadiusEntry");
		App.ClearText("CornerRadiusEntry");
		App.EnterText("CornerRadiusEntry", "20");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button Image CornerRadius");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(38)]
	public void VerifyButton_ImageWithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageSourceDotnetBotButton");
		App.Tap("ImageSourceDotnetBotButton");
		App.WaitForElement("ShadowTrueButton");
		App.Tap("ShadowTrueButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button Image Shadow");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(39)]
	public void VerifyButton_ImageWithBackgroundColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageSourceDotnetBotButton");
		App.Tap("ImageSourceDotnetBotButton");
		App.WaitForElement("BackgroundColorGreen");
		App.Tap("BackgroundColorGreen");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button Image Background");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(40)]
	public void VerifyButton_ContentLayoutSpacingZero()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageSourceDotnetBotButton");
		App.Tap("ImageSourceDotnetBotButton");
		App.WaitForElement("ContentLayoutLeftButton");
		App.Tap("ContentLayoutLeftButton");
		App.WaitForElement("ContentLayoutSpacingEntry");
		App.ClearText("ContentLayoutSpacingEntry");
		App.EnterText("ContentLayoutSpacingEntry", "0");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(41)]
	public void VerifyButton_ContentLayoutWithRightToLeftFlowDirection()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageSourceDotnetBotButton");
		App.Tap("ImageSourceDotnetBotButton");
		App.WaitForElement("ContentLayoutLeftButton");
		App.Tap("ContentLayoutLeftButton");
		App.WaitForElement("FlowDirectionRightToLeftButton");
		App.Tap("FlowDirectionRightToLeftButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button RTL Image");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(42)]
	public void VerifyButton_LargeCornerRadius()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CornerRadiusEntry");
		App.ClearText("CornerRadiusEntry");
		App.EnterText("CornerRadiusEntry", "999");
		App.WaitForElement("BackgroundColorGreen");
		App.Tap("BackgroundColorGreen");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button Pill");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(43)]
	public void VerifyButton_IsEnabledFalseVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledFalseButton");
		App.Tap("IsEnabledFalseButton");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button Disabled");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(44)]
	public void VerifyButton_IsEnabledToggleFiresClick()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledFalseButton");
		App.Tap("IsEnabledFalseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledTrueButton");
		App.Tap("IsEnabledTrueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		App.Tap("ButtonControl");
		Assert.That(App.FindElement("ClickedEventLabel").GetText(), Is.EqualTo("Clicked Event Executed"));
	}

	[Test, Order(45)]
	public void VerifyButton_TextAndTextColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button TextColor");
		App.WaitForElement("TextColorGreenButton");
		App.Tap("TextColorGreenButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(46)]
	public void VerifyButton_BorderColorAndTextColor()
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
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button BorderColor TextColor");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
