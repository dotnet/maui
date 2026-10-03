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
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
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

	[Test, Order(47)]
	public void VerifyButton_IsVisibleTrueAfterFalse()
	{
		// Hide the button.
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseButton");
		App.Tap("IsVisibleFalseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("ButtonControl");

		// Restore visibility and assert the button reappears.
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleTrueButton");
		App.Tap("IsVisibleTrueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		App.Tap("ButtonControl");
		Assert.That(App.FindElement("ClickedEventLabel").GetText(), Is.EqualTo("Clicked Event Executed"));
	}

	[Test, Order(48)]
	public void VerifyButton_NegativeContentLayoutSpacing()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageSourceDotnetBotButton");
		App.Tap("ImageSourceDotnetBotButton");
		App.WaitForElement("ContentLayoutLeftButton");
		App.Tap("ContentLayoutLeftButton");
		App.WaitForElement("ContentLayoutSpacingEntry");
		App.ClearText("ContentLayoutSpacingEntry");
		App.EnterText("ContentLayoutSpacingEntry", "-1");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(49)]
	public void VerifyButton_ZeroHeightAndWidthRequest()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HeightRequestEntry");
		App.ClearText("HeightRequestEntry");
		App.EnterText("HeightRequestEntry", "0");
		App.WaitForElement("WidthRequestEntry");
		App.ClearText("WidthRequestEntry");
		App.EnterText("WidthRequestEntry", "0");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button ZeroSize");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("Options");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(52)]
	public void VerifyButton_BackgroundColorResetToNone()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorRed");
		App.Tap("BackgroundColorRed");
		App.WaitForElement("TextEntry");
		App.ClearText("TextEntry");
		App.EnterText("TextEntry", "Button Background Reset");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");

		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorNone");
		App.Tap("BackgroundColorNone");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(55)]
	public void VerifyButton_HorizontalOptionsEnd()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HorizontalOptionsEndButton");
		App.Tap("HorizontalOptionsEndButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(56)]
	public void VerifyButton_VerticalOptionsEnd()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VerticalOptionsEndButton");
		App.Tap("VerticalOptionsEndButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("ButtonControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
