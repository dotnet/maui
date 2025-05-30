using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class RadioButtonFeatureTests : UITest
{
	public const string RadioButtonFeatureMatrix = "RadioButton Feature Matrix";

	public RadioButtonFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(RadioButtonFeatureMatrix);
	}
	[Test]
	[Category(UITestCategories.RadioButton)]
#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID // This test fails on Windows and Android because the RadioButton control does not update the BorderColor at runtime. Issue Link - https://github.com/dotnet/maui/issues/15806
	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetTextColorAndBorderColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRedButton");
		App.Tap("TextColorRedButton");
		App.WaitForElement("BorderColorPurpleButton");
		App.Tap("BorderColorPurpleButton");
		App.WaitForElement("BorderWidthEntry");
		App.ClearText("BorderWidthEntry");
		App.EnterText("BorderWidthEntry", "2");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControl");
		VerifyScreenshot();
	}
#endif

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetFontAttributesAndTextColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("TextColorBlueButton");
		App.Tap("TextColorBlueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetFontFamilyAndFontSize_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyMontserratBold");
		App.Tap("FontFamilyMontserratBold");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "24");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControl");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID // This test fails on Windows and Android because the RadioButton control does not update the BorderColor at runtime. Issue Link - https://github.com/dotnet/maui/issues/15806
	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetBorderWidthAndCornerRadius_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BorderWidthEntry");
		App.ClearText("BorderWidthEntry");
		App.EnterText("BorderWidthEntry", "4");
		App.PressEnter();
		App.WaitForElement("BorderColorGreenButton");
		App.Tap("BorderColorGreenButton");
		App.WaitForElement("CornerRadiusEntry");
		App.ClearText("CornerRadiusEntry");
		App.EnterText("CornerRadiusEntry", "12");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControl");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // This test fails on Android and Windows because the text transform is not applied correctly. Issue Link - https://github.com/dotnet/maui/issues/29729
	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetFontFamilyAndTextTransform_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyMontserratBold");
		App.Tap("FontFamilyMontserratBold");
		App.WaitForElement("TextTransformUpper");
		App.Tap("TextTransformUpper");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControl");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_ANDROID // On Android, the View object is not supported, so it falls back to a string representation of the object. https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/radiobutton?view=net-maui-9.0#create-radiobuttons
	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetContentWithView()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ViewContentButton");
		App.Tap("ViewContentButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControl");
		VerifyScreenshot();
	}
#endif

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetContentAndTextColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ContentEntry");
		App.ClearText("ContentEntry");
		App.EnterText("ContentEntry", "Colored Text");
		App.PressEnter();
		App.WaitForElement("TextColorBlueButton");
		App.Tap("TextColorBlueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControl");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // This test fails on Windows because the character spacing is not applied correctly.
	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetContentAndCharacterSpacing_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ContentEntry");
		App.ClearText("ContentEntry");
		App.EnterText("ContentEntry", "Spaced Text");
		App.PressEnter();
		App.WaitForElement("CharacterSpacingEntry");
		App.ClearText("CharacterSpacingEntry");
		App.EnterText("CharacterSpacingEntry", "10");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControl");
		VerifyScreenshot();
	}
#endif

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetContentAndFontSize_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ContentEntry");
		App.ClearText("ContentEntry");
		App.EnterText("ContentEntry", "Large Text");
		App.PressEnter();
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "24");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetContentAndFontAttributes_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ContentEntry");
		App.ClearText("ContentEntry");
		App.EnterText("ContentEntry", "Bold Option");
		App.PressEnter();
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControl");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // This test fails on Android and Windows because the text transform is not applied correctly. Issue Link - https://github.com/dotnet/maui/issues/29729
	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetContentAndTextTransform()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ContentEntry");
		App.ClearText("ContentEntry");
		App.EnterText("ContentEntry", "transform this");
		App.PressEnter();
		App.WaitForElement("TextTransformUpper");
		App.Tap("TextTransformUpper");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot();
	}
#endif

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetGroupNameAndIsChecked()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("GroupName2Radio");
		App.Tap("GroupName2Radio");
		App.WaitForElement("IsCheckedTrueRadio");
		App.Tap("IsCheckedTrueRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Group2RadioButton");
		App.Tap("Group2RadioButton");
		var text = App.WaitForElement("IsCheckedLabel").GetText();
		Assert.That(text, Is.EqualTo("False"));
	}

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetFontFamilyAndFontAttributes_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyMontserratBold");
		App.Tap("FontFamilyMontserratBold");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControl");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButton_SetFontSizeAndFontAttributes_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "24");
		App.PressEnter();
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControl");
		VerifyScreenshot();
	}
}
