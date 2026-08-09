// Material3 Entry uses TextInputLayout + TextInputEditText instead of standard EditText.
// These tests run only on Android where Material3 EntryHandler2 is used.
#if ANDROID
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class Material3EntryFeatureTests : _GalleryUITest
{
	public override string GalleryPageName => "Entry Material3 Feature Matrix";

	private const int CropBottomValue = 1500;

	public Material3EntryFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_InitialState_VerifyVisualState()
	{
		App.WaitForElement("TestEntry");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_TextWithHorizontalAlignment_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HCenter");
		App.Tap("HCenter");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(3)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_TextWithVerticalAlignment_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(4)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_HorizontalAndVerticalAlignment_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("HEnd");
		App.Tap("HEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(5)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_TextColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(6)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_FontSize_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(7)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_FontFamily_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamily");
		App.EnterText("FontFamily", "MontserratBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(8)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_FontAttributes_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(9)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_CharacterSpacing_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(10)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_HorizontalAlignmentWithCharacterSpacing_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("HCenter");
		App.Tap("HCenter");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(11)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_VerticalAlignmentWithCharacterSpacing_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(12)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_CharacterSpacingWithFontFamily_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("FontFamily");
		App.EnterText("FontFamily", "MontserratBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_CharacterSpacingWithMaxLength_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "Test Entered Set MaxLength");
		App.WaitForElement("MaxLength");
		App.ClearText("MaxLength");
		App.EnterText("MaxLength", "6");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(14)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_IsPassword_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(15)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_IsPasswordWithCharacterSpacing_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(16)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_IsPasswordWithFontSize_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(17)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_FlowDirection_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(18)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_PlaceholderText_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		App.ClearText("TestEntry");
		App.DismissKeyboard();
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(19)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_PlaceholderColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderColorRed");
		App.Tap("PlaceholderColorRed");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID //FlowDirection does not apply to the placeholder in Material3 Entry.
	[Test, Order(20)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_PlaceholderWithFlowDirection_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

#if TEST_FAILS_ON_ANDROID //HorizontalTextAlignment does not apply to the placeholder in Material3 Entry. TextInputLayout does not expose a public API for hint text gravity.
	[Test, Order(21)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_PlaceholderWithHorizontalAlignment_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HCenter");
		App.Tap("HCenter");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(22)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_PlaceholderWithVerticalAlignment_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VStart");
		App.Tap("VStart");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(23)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_PlaceholderWithFontFamily_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamily");
		App.EnterText("FontFamily", "MontserratBold");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(24)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_PlaceholderWithFontSize_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(25)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_PlaceholderWithFontAttributes_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID //CharacterSpacing does not apply to the placeholder in Material3 Entry. TextInputLayout does not expose a public API for hint letter spacing.
	[Test, Order(26)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_PlaceholderWithCharacterSpacing_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(27)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_PlaceholderWithPasswordTrue_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your password");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(28)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_Shadow_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(29)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_PlaceholderWithShadow_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(30)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_IsEnabled_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("EnabledFalse");
		App.Tap("EnabledFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(31)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_IsPasswordWithMaxLength_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "Test Entered Set MaxLength");
		App.WaitForElement("MaxLength");
		App.ClearText("MaxLength");
		App.EnterText("MaxLength", "6");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(32)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_IsPasswordWithVerticalAlignment_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(33)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_IsPasswordWithHorizontalAlignment_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HEnd");
		App.Tap("HEnd");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, cropBottom: CropBottomValue, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(34)]
	[Category(UITestCategories.Material3)]
	public void Material3Entry_PlaceholderColorAndTextColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("PlaceholderColorBlue");
		App.Tap("PlaceholderColorBlue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TestEntry");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

}
#endif
