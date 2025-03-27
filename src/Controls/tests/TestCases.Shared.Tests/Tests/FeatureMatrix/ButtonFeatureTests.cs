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

    //(Excel-2L) Border and TextColor
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
    //(Excel-3H) BorderWidth and FontSize
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_SetBorderWidthAndFontSize_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("BorderWidthEntry");
        App.ClearText("BorderWidthEntry");
        App.EnterText("BorderWidthEntry", "5");
        App.WaitForElement("FontSizeEntry");
        App.ClearText("FontSizeEntry");
        App.EnterText("FontSizeEntry", "20");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "Button Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(Excel-3I)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_SetBorderWidthEntryAndLineBreakMode_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("BorderWidthEntry");
        App.ClearText("BorderWidthEntry");
        App.EnterText("BorderWidthEntry", "20");
        App.WaitForElement("LineBreakModeCharacterWrapButton");
        App.Tap("LineBreakModeCharacterWrapButton");
        App.WaitForElement("TextEntry");
        string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
        App.EnterText("TextEntry", longText);
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(4F)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_CharacterSpacingAndFontAttributes_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("CharacterSpacingEntry");
        App.ClearText("CharacterSpacingEntry");
        App.EnterText("CharacterSpacingEntry", "5");
        App.WaitForElement("FontAttributesBoldButton");
        App.Tap("FontAttributesBoldButton");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "ButtonText");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(4G)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_CharacterSpacingAndFontFamily_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("CharacterSpacingEntry");
        App.ClearText("CharacterSpacingEntry");
        App.EnterText("CharacterSpacingEntry", "5");
        App.WaitForElement("FontFamilyCourierNewButton");
        App.Tap("FontFamilyCourierNewButton");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "ButtonText");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(4H)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_CharacterSpacingAndFontSize_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("CharacterSpacingEntry");
        App.ClearText("CharacterSpacingEntry");
        App.EnterText("CharacterSpacingEntry", "5");
        App.WaitForElement("FontSizeEntry");
        App.EnterText("FontSizeEntry", "20");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "ButtonText");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(4I)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_SetCharacterSpacingAndLineBreakMode_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("CharacterSpacingEntry");
        App.ClearText("CharacterSpacingEntry");
        App.EnterText("CharacterSpacingEntry", "5");
        App.WaitForElement("LineBreakModeCharacterWrapButton");
        App.Tap("LineBreakModeCharacterWrapButton");
        App.WaitForElement("TextEntry");
        string longText = "ThisisaverylongtextthatshouldwrapcorrectlybasedontheLineBreakMode";
        App.EnterText("TextEntry", longText);
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(4K)
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
    //(4M)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_SetCharacterSpacingAndTextTransform_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("CharacterSpacingEntry");
        App.ClearText("CharacterSpacingEntry");
        App.EnterText("CharacterSpacingEntry", "5");
        App.WaitForElement("TextTransformUppercaseButton");
        App.Tap("TextTransformUppercaseButton");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "ButtonText");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(5C)
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
    //(5J)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_SetCornerRadiusAndPadding_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("CornerRadiusEntry");
        App.ClearText("CornerRadiusEntry");
        App.EnterText("CornerRadiusEntry", "5");
        App.WaitForElement("PaddingEntry");
        App.ClearText("PaddingEntry");
        App.EnterText("PaddingEntry", "5");
        App.WaitForElement("BorderWidthEntry");
        App.ClearText("BorderWidthEntry");
        App.EnterText("BorderWidthEntry", "5");
        App.WaitForElement("BorderColorRedButton");
        App.Tap("BorderColorRedButton");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "Button Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(6G)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setFontAtributesAndFontFamily_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAttributesBoldButton");
        App.Tap("FontAttributesBoldButton");
        App.WaitForElement("FontFamilyCourierNewButton");
        App.Tap("FontFamilyCourierNewButton");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "Button Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(6H)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setFontAtributesAndFontSize_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAttributesBoldButton");
        App.Tap("FontAttributesBoldButton");
        App.WaitForElement("FontSizeEntry");
        App.ClearText("FontSizeEntry");
        App.EnterText("FontSizeEntry", "20");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "Button Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(6I)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setFontAtributesAndLineBreakMode_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAttributesBoldButton");
        App.Tap("FontAttributesBoldButton");
        App.WaitForElement("LineBreakModeCharacterWrapButton");
        App.Tap("LineBreakModeCharacterWrapButton");
        App.WaitForElement("TextEntry");
        string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
        App.EnterText("TextEntry", longText);
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(6J)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setFontAtributesAndPadding_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAttributesBoldButton");
        App.Tap("FontAttributesBoldButton");
        App.WaitForElement("PaddingEntry");
        App.ClearText("PaddingEntry");
        App.EnterText("PaddingEntry", "5");
        App.WaitForElement("BorderWidthEntry");
        App.ClearText("BorderWidthEntry");
        App.EnterText("BorderWidthEntry", "5");
        App.WaitForElement("BorderColorRedButton");
        App.Tap("BorderColorRedButton");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "Button Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(6K)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setFontAtributesAndText_VerifyVisualState()
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
    //(6M)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setFontAtributesAndTextTransform_VerifyVisualState()
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
    //(7H)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setFontFamilyAndFontSize_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontFamilyCourierNewButton");
        App.Tap("FontFamilyCourierNewButton");
        App.WaitForElement("FontSizeEntry");
        App.ClearText("FontSizeEntry");
        App.EnterText("FontSizeEntry", "20");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "Button Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(7I)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setFontFamilyAndLineBreakMode_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontFamilyCourierNewButton");
        App.Tap("FontFamilyCourierNewButton");
        App.WaitForElement("LineBreakModeCharacterWrapButton");
        App.Tap("LineBreakModeCharacterWrapButton");
        App.WaitForElement("TextEntry");
        string longText = "This is a very long text that should wrap correctly based on the LineBreakMode settings applied to the Button";
        App.EnterText("TextEntry", longText);
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(7J)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setFontFamilyAndPadding_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontFamilyCourierNewButton");
        App.Tap("FontFamilyCourierNewButton");
        App.WaitForElement("PaddingEntry");
        App.ClearText("PaddingEntry");
        App.EnterText("PaddingEntry", "5");
        App.WaitForElement("BorderWidthEntry");
        App.ClearText("BorderWidthEntry");
        App.EnterText("BorderWidthEntry", "5");
        App.WaitForElement("BorderColorRedButton");
        App.Tap("BorderColorRedButton");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "Button Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(7K)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setFontFamilyAndText_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontFamilyCourierNewButton");
        App.Tap("FontFamilyCourierNewButton");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "Button Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(7M)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setFontFamilyAndTextTransform_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontFamilyCourierNewButton");
        App.Tap("FontFamilyCourierNewButton");
        App.WaitForElement("TextTransformUppercaseButton");
        App.Tap("TextTransformUppercaseButton");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "Button Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(8I)
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
    //(8J)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setFontSizeAndPadding_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontSizeEntry");
        App.ClearText("FontSizeEntry");
        App.EnterText("FontSizeEntry", "20");
        App.WaitForElement("PaddingEntry");
        App.ClearText("PaddingEntry");
        App.EnterText("PaddingEntry", "5");
        App.WaitForElement("BorderWidthEntry");
        App.ClearText("BorderWidthEntry");
        App.EnterText("BorderWidthEntry", "5");
        App.WaitForElement("BorderColorRedButton");
        App.Tap("BorderColorRedButton");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "Button Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(8K)
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
    //(8M)
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
    //(9K)
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
    //(9M)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setLineBreakModeAndTextTransform_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("LineBreakModeCharacterWrapButton");
        App.Tap("LineBreakModeCharacterWrapButton");
        App.WaitForElement("TextTransformUppercaseButton");
        App.Tap("TextTransformUppercaseButton");
        App.WaitForElement("TextEntry");
        App.EnterText("TextEntry", "Button Text");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    //(10K)
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
    //(11L)
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
    //(11M)
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
    //(12M)
    [Test]
    [Category(UITestCategories.Button)]
    public void Button_setTextColorAndTextTransform_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("TextColorGreenButton");
        App.Tap("TextColorGreenButton");
        App.WaitForElement("TextTransformUppercaseButton");
        App.Tap("TextTransformUppercaseButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }

}
