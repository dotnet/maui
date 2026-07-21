// Material3 Entry uses TextInputLayout + TextInputEditText instead of standard EditText.
// These tests run only on Android where Material3 EntryHandler2 is used.
#if ANDROID
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;
[Category(UITestCategories.Material3)]
public class Material3EntryFeatureTests : _GalleryUITest
{
	public override string GalleryPageName => "Entry Material3 Feature Matrix";

	private const int CropBottomValue = 1500;

	public Material3EntryFeatureTests(TestDevice device)
		: base(device)
	{
	}

	// Note: FontAutoScaling states cannot currently be reliably covered in CI environments, as system font scaling settings are not consistently supported or controllable in automated runs.

	[Test, Order(1)]
	public void VerifyMaterial3TextWhenAlignedHorizontally()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HCenter");
		App.Tap("HCenter");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(2)]
	public void VerifyMaterial3TextWhenAlignedVertically()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(3)]
	public void VerifyMaterial3TextWhenFontFamilySetValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamily");
		App.EnterText("FontFamily", "MontserratBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(4)]
	public void VerifyMaterial3TextWhenCharacterSpacingSetValues()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacing");
		App.ClearText("CharacterSpacing");
		App.EnterText("CharacterSpacing", "5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(5)]
	public void VerifyMaterial3HorizontalTextAlignmentBasedOnCharacterSpacing()
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
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(6)]
	public void VerifyMaterial3VerticalTextAlignmentBasedOnCharacterSpacing()
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
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(7)]
	public void VerifyMaterial3IsPasswordBasedOnCharacterSpacing()
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
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(8)]
	public void VerifyMaterial3CharacterSpacingWhenFontFamily()
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
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(9)]
	public void VerifyMaterial3TextWhenIsPasswordTrueOrFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(10)]
	public void VerifyMaterial3IsPasswordBasedOnVerticalTextAlignment()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(11)]
	public void VerifyMaterial3IsPasswordBasedOnHorizontalTextAlignment()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HEnd");
		App.Tap("HEnd");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(12)]
	public void VerifyMaterial3IsPasswordWhenMaxLengthSetValue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.EnterText("TextEntryChanged", "Test Entered Set MaxLength");
		App.WaitForElement("MaxLength");
		App.ClearText("MaxLength");
		App.EnterText("MaxLength", "6");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(13)]
	public void VerifyMaterial3CharacterSpacingWhenMaxLengthSet()
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
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(14)]
	public void VerifyMaterial3HorizontalTextAlignmentWhenVerticalTextAlignmentSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VEnd");
		App.Tap("VEnd");
		App.WaitForElement("HEnd");
		App.Tap("HEnd");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(15)]
	public void VerifyMaterial3TextWhenTextColorSetCorrectly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(16)]
	public void VerifyMaterial3TextColorResetToDefault()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorDefault");
		App.Tap("TextColorDefault");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(17)]
	public void VerifyMaterial3TextWhenFontSizeSetCorrectly()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

[Test, Order(18)]
	public void VerifyMaterial3IsPasswordWhenFontSizeSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PasswordTrue");
		App.Tap("PasswordTrue");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

#if TEST_FAILS_ON_ANDROID //Related issue: issue link: https://github.com/dotnet/maui/issues/29833"
	[Test, Order(19)]
	public void VerifyMaterial3TextWhenIsSpellCheckEnabledTrue()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SpellCheckTrue");
		App.Tap("SpellCheckTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		App.EnterText("TestEntry", "Testig");
		App.EnterText("TestEntry", " ");
		VerifyScreenshotWithKeyboardHandling();
	}
#endif

#if TEST_FAILS_ON_ANDROID //In Android related issue:https://github.com/dotnet/maui/issues/26968 and In mac and Windows keybord type is not supported.
	[Test, Order(20)]
	public void VerifyMaterial3TextWhenKeyboardTypeSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Numeric");
		App.Tap("Numeric");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshot();
	}

	[Test, Order(21)]
	public void VerifyMaterial3TextWhenReturnTypeSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Search");
		App.Tap("Search");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		VerifyScreenshot();
	}
#endif


	[Test, Order(22)]
	public void VerifyMaterial3EntryControlWhenFlowDirectionSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(23)]
	public void VerifyMaterial3PlaceholderWhenFlowDirectionSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}


	[Test, Order(24)]
	public void VerifyMaterial3EntryControlWhenPlaceholderTextSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		HideSoftKeyboardIfVisible();
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(25)]
	public void VerifyMaterial3EntryControlWhenPlaceholderColorSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderColorRed");
		App.Tap("PlaceholderColorRed");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(26)]
	public void VerifyMaterial3PlaceholderColorResetToDefault()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderColorRed");
		App.Tap("PlaceholderColorRed");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PlaceholderColorDefault");
		App.Tap("PlaceholderColorDefault");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(27)]
	public void VerifyMaterial3TextWhenFontAttributesSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

[Test, Order(28)]
	public void VerifyMaterial3Entry_WithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(29)]
	public void VerifyMaterial3PlaceholderWithShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(30)]
	public void VerifyMaterial3PlaceholderWithPasswordTrue()
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
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(31)]
	public void VerifyMaterial3PlaceholderWithHorizontalAlignment()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HCenter");
		App.Tap("HCenter");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(32)]
	public void VerifyMaterial3PlaceholderWithVerticalAlignment()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VStart");
		App.Tap("VStart");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

[Test, Order(33)]
	public void VerifyMaterial3PlaceholderWithCharacterSpacing()
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
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(34)]
	public void VerifyMaterial3PlaceholderWithFontFamily()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamily");
		App.EnterText("FontFamily", "MontserratBold");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(35)]
	public void VerifyMaterial3PlaceholderWithFontSize()
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
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(36)]
	public void VerifyMaterial3PlaceholderWithFontAttributes()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(37)]
	public void VerifyMaterial3EntryWhenWidthRequestSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("WidthRequest");
		App.ClearText("WidthRequest");
		App.EnterText("WidthRequest", "150");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(38)]
	public void VerifyMaterial3EntryWhenHeightRequestSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HeightRequest");
		App.ClearText("HeightRequest");
		App.EnterText("HeightRequest", "80");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(39)]
	public void VerifyMaterial3EntryWhenHeightRequestAndWidthRequestSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HeightRequest");
		App.ClearText("HeightRequest");
		App.EnterText("HeightRequest", "100");
		App.WaitForElement("WidthRequest");
		App.ClearText("WidthRequest");
		App.EnterText("WidthRequest", "150");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(40)]
	public void VerifyMaterial3EntryWhenOpacitySet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(41)]
	public void VerifyMaterial3EntryWhenOpacityResetToDefault()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0.5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "1.0");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(42)]
	public void VerifyMaterial3EntryWhenOpacitySetToZero()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OpacityEntry");
		App.ClearText("OpacityEntry");
		App.EnterText("OpacityEntry", "0");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(43)]
	public void VerifyMaterial3EntryWhenBackgroundColorSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorLightBlue");
		App.Tap("BackgroundColorLightBlue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}
	
	[Test, Order(44)]
	public void VerifyMaterial3TextWhenBoldAndItalicFontAttributesSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(45)]
	public void VerifyMaterial3PlaceholderTextWhenBoldAndItalicFontAttributesSet()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		HideSoftKeyboardIfVisible();
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(46)]
	public void VerifyMaterial3EntryBackgroundColorWithTextColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorYellow");
		App.Tap("BackgroundColorYellow");
		App.WaitForElement("TextColorRed");
		App.Tap("TextColorRed");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(47)]
	public void VerifyMaterial3EntryBackgroundColorWithPlaceholderText()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorLightBlue");
		App.Tap("BackgroundColorLightBlue");
		App.WaitForElement("PlaceholderText");
		App.ClearText("PlaceholderText");
		App.EnterText("PlaceholderText", "Enter your name");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");
		HideSoftKeyboardIfVisible();
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(48)]
	public void VerifyMaterial3EntryBackgroundColorWithPlaceholderColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorYellow");
		App.Tap("BackgroundColorYellow");
		App.WaitForElement("PlaceholderColorRed");
		App.Tap("PlaceholderColorRed");
		App.WaitForElement("TextEntryChanged");
		App.ClearText("TextEntryChanged");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}

	[Test, Order(49)]
	public void VerifyMaterial3EntryBackgroundColorResetToDefault()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorYellow");
		App.Tap("BackgroundColorYellow");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("BackgroundColorDefault");
		App.Tap("BackgroundColorDefault");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("TestEntry");
		VerifyScreenshot(cropBottom: CropBottomValue);
	}


	/// <summary>
	/// Helper method to handle keyboard visibility and take a screenshot with appropriate cropping
	/// </summary>
	/// <param name="screenshotName">Optional name for the screenshot</param>
	private void VerifyScreenshotWithKeyboardHandling(string? screenshotName = null)
	{		if (App.IsKeyboardShown())
			App.DismissKeyboard();

		// Using cropping instead of DismissKeyboard() on iOS to maintain focus during testing
		if (string.IsNullOrEmpty(screenshotName))
			VerifyScreenshot(cropBottom: CropBottomValue);
		else
			VerifyScreenshot(screenshotName, cropBottom: CropBottomValue);
	}

	/// <summary>
	/// Helper method to handle keyboard visibility and set exception if screenshot verification fails
	/// </summary>
	/// <param name="exception">Reference to exception variable</param>
	/// <param name="screenshotName">Name for the screenshot</param>
	private void VerifyScreenshotWithKeyboardHandlingOrSetException(ref Exception? exception, string screenshotName)
	{
		if (App.IsKeyboardShown())
			App.DismissKeyboard();
		VerifyScreenshotOrSetException(ref exception, screenshotName, cropBottom: CropBottomValue);

	}

	/// <summary>
	/// Hides the soft keyboard on Android/iOS if it is currently visible, and on Android waits for the
	/// hide animation to complete. Prevents flakiness when a prior action (e.g. tapping a button that
	/// calls Focus() on an Entry re-shows the keyboard. No-op on Windows and MacCatalyst.
	/// </summary>
	private void HideSoftKeyboardIfVisible()
	{if (App.IsKeyboardShown())
		{
			App.DismissKeyboard();
			App.WaitForKeyboardToHide();
		}
	}
}
#endif
