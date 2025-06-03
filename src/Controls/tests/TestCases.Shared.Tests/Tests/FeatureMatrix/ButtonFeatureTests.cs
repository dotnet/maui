using System;
using Microsoft.Maui.Platform;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class ButtonFeatureTests : UITest
{
    public const string ButtonFeatureMatrix = "Button Feature Matrix";
    public ButtonFeatureTests(TestDevice testDevice) : base(testDevice)
    {
    }

    protected override void FixtureSetup()
    {
        base.FixtureSetup();
        App.NavigateToGallery(ButtonFeatureMatrix);
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //CharacterSpacingEntry property not working on iOS and Catalyst, Issue: https://github.com/dotnet/maui/issues/21488
    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }
#endif

    [Test]
    [Category(UITestCategories.Button)]
    public void Button_SetCommandAndCommandParameter()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "Command Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElement("ButtonControl");
        App.Tap("ButtonControl");
        var buttonText = App.FindElement("ButtonControl").GetText();
        Assert.That(buttonText, Is.EqualTo("Command Executed"));
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setLineBreakModeAndText_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("LineBreakModeCharacterWrapButton");
        App.Tap("LineBreakModeCharacterWrapButton");
        App.WaitForElement("TextEntry");
        string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
        App.EnterText("TextEntry", longText);
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.Button)]
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
        Assert.That(App.FindElement("ButtonControl").GetText(), Is.EqualTo("BUTTONTEXT"));
    }
}
