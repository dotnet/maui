// Material3 RadioButton tests reuse the existing RadioButton Feature Matrix HostApp page.
// The native Android view differs under Material3, so these tests
// produce separate screenshot baselines under the Material3 category.
#if ANDROID
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class Material3RadioButtonFeatureTests : _GalleryUITest
{
	public override string GalleryPageName => "RadioButton Feature Matrix";

	public Material3RadioButtonFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_Checking_Default_Configuration_VerifyVisualState()
	{
		App.WaitForElement("RadioButtonControlOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_Checking_Initial_Configuration_VerifyVisualState()
	{
		App.WaitForElement("RadioButtonControlOne");
		App.Tap("RadioButtonControlOne");
		App.WaitForElement("SelectedValueLabelOne");
		Assert.That(App.WaitForElement("SelectedValueLabelOne").GetText(), Is.EqualTo("Dark Mode"));
		App.WaitForElement("RadioButtonControlFour");
		App.Tap("RadioButtonControlFour");
		App.WaitForElement("SelectedValueLabelTwo");
		Assert.That(App.WaitForElement("SelectedValueLabelTwo").GetText(), Is.EqualTo("All Notifications"));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID // This test fails on Android because the RadioButton control does not update the BorderColor at runtime. Issue Link - https://github.com/dotnet/maui/issues/15806
	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_SetTextColorAndBorderColor_VerifyVisualState()
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
		App.WaitForElementTillPageNavigationSettled("RadioButtonControlOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_SetFontAttributesAndTextColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("TextColorBlueButton");
		App.Tap("TextColorBlueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControlOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_SetFontFamilyAndFontSize_VerifyVisualState()
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
		App.WaitForElementTillPageNavigationSettled("RadioButtonControlOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID // This test fails on Android because the RadioButton control does not update the BorderColor at runtime. Issue Link - https://github.com/dotnet/maui/issues/15806
	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_SetBorderWidthAndCornerRadius_VerifyVisualState()
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
		App.WaitForElementTillPageNavigationSettled("RadioButtonControlOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

#if TEST_FAILS_ON_ANDROID // This test fails on Android because the text transform is not applied correctly. Issue Link - https://github.com/dotnet/maui/issues/29729
	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_SetFontFamilyAndTextTransform_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyMontserratBold");
		App.Tap("FontFamilyMontserratBold");
		App.WaitForElement("TextTransformUpper");
		App.Tap("TextTransformUpper");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControlOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

#if TEST_FAILS_ON_ANDROID // On Android, the View object is not supported, so it falls back to a string representation of the object. https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/radiobutton?view=net-maui-9.0#create-radiobuttons
	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_SetContentWithView()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ViewContentButton");
		App.Tap("ViewContentButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControlOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_SetContentAndTextColor_VerifyVisualState()
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
		App.WaitForElementTillPageNavigationSettled("RadioButtonControlOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_SetContentAndCharacterSpacing_VerifyVisualState()
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
		App.WaitForElementTillPageNavigationSettled("RadioButtonControlOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_SetContentAndFontSize_VerifyVisualState()
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
		App.WaitForElementTillPageNavigationSettled("RadioButtonControlOne");
		App.WaitForElement("SelectedValueLabelOne");
		App.Tap("SelectedValueLabelOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_SetContentAndFontAttributes_VerifyVisualState()
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
		App.WaitForElementTillPageNavigationSettled("RadioButtonControlOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID // This test fails on Android because the text transform is not applied correctly. Issue Link - https://github.com/dotnet/maui/issues/29729
	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_SetContentAndTextTransform()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_SetFontFamilyAndFontAttributes_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyMontserratBold");
		App.Tap("FontFamilyMontserratBold");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControlOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_SetFontSizeAndFontAttributes_VerifyVisualState()
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
		App.WaitForElementTillPageNavigationSettled("RadioButtonControlOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_IsVisibleAndContent_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseRadio");
		App.Tap("IsVisibleFalseRadio");
		App.WaitForElement("ContentEntry");
		App.ClearText("ContentEntry");
		App.EnterText("ContentEntry", "Visible Option");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("RadioButtonControlOne");
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void Material3RadioButton_FlowDirectionAndContent_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("ContentEntry");
		App.ClearText("ContentEntry");
		App.EnterText("ContentEntry", "Right to Left Option");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("RadioButtonControlOne");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
