// Material3 Picker tests reuse the existing Picker Feature Matrix HostApp page.
// The native Android view differs, so these tests
// produce separate screenshot baselines under the Material3 category.
#if ANDROID
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class Material3PickerFeatureTests : _GalleryUITest
{
	public override string GalleryPageName => "Picker Feature Matrix";

	public Material3PickerFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_Validate_VerifyLabels()
	{
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_TapPicker_TakeScreenshot()
	{
		App.WaitForElement("Picker");
		App.Tap("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(3)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SelectItem_VerifySelectedItem()
	{
		App.WaitForElement("Cancel");
		App.Tap("Cancel");
		App.WaitForElement("Picker");
		App.Tap("Picker");
		App.WaitForElement("Option 3 - Third option");
		App.Tap("Option 3 - Third option");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(4)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetSelectedIndex_VerifySelectedIndexAndItem()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SelectedIndexEntry");
		App.ClearText("SelectedIndexEntry");
		App.EnterText("SelectedIndexEntry", "1");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		Assert.That(App.FindElement("SelectedIndexLabel").GetText(), Is.EqualTo("1"));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(5)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetSelectedItem_VerifySelectedItemLabel()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SelectedItemEntry");
		App.ClearText("SelectedItemEntry");
		App.EnterText("SelectedItemEntry", "Option 4 - Fourth option");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		Assert.That(App.FindElement("SelectedItemLabel").GetText(), Is.EqualTo("Option 4 - Fourth option"));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(6)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetCharacterSpacing_VerifyCharacterSpacingLabel()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacingEntry");
		App.ClearText("CharacterSpacingEntry");
		App.EnterText("CharacterSpacingEntry", "5");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(7)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetCharacterSpacingAndTitle_VerifyBoth()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TitleEntry");
		App.ClearText("TitleEntry");
		App.EnterText("TitleEntry", "Custom Title");
		App.WaitForElement("CharacterSpacingEntry");
		App.ClearText("CharacterSpacingEntry");
		App.EnterText("CharacterSpacingEntry", "5");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(8)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetFontSizeAndFontAttributesBold_VerifyFontSizeAndAttributes()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "24");
		App.PressEnter();
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("SelectedIndexEntry");
		App.ClearText("SelectedIndexEntry");
		App.EnterText("SelectedIndexEntry", "1");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(9)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetFontSizeAndFontFamilyDokdo_VerifyFontSizeAndFontFamily()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "18");
		App.PressEnter();
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("SelectedIndexEntry");
		App.ClearText("SelectedIndexEntry");
		App.EnterText("SelectedIndexEntry", "1");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(10)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetFlowDirectionRTL_VerifyFlowDirection()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement("SelectedIndexEntry");
		App.ClearText("SelectedIndexEntry");
		App.EnterText("SelectedIndexEntry", "1");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(11)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetFlowDirectionRTLAndTitle_VerifyFlowDirectionAndTitle()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement("TitleEntry");
		App.ClearText("TitleEntry");
		App.EnterText("TitleEntry", "RTL Title");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(12)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetFontAttributesItalicAndFontFamilyDokdo_VerifyFontAttributesAndFontFamily()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("SelectedIndexEntry");
		App.ClearText("SelectedIndexEntry");
		App.EnterText("SelectedIndexEntry", "2");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetIsEnabledFalse_VerifyPickerDisabled()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledFalseRadio");
		App.Tap("IsEnabledFalseRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		App.Tap("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(14)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetIsVisibleFalse_VerifyPickerHidden()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseRadio");
		App.Tap("IsVisibleFalseRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("Picker");
	}

	[Test, Order(15)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetHorizontalTextAlignmentAndSelectedItem_VerifySelectedItem()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HorizontalTextAlignmentEnd");
		App.Tap("HorizontalTextAlignmentEnd");
		App.WaitForElement("SelectedItemEntry");
		App.ClearText("SelectedItemEntry");
		App.EnterText("SelectedItemEntry", "Option 2 - Second option");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(16)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetHorizontalTextAlignmentEndAndTitle_VerifyTitleAlignment()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HorizontalTextAlignmentEnd");
		App.Tap("HorizontalTextAlignmentEnd");
		App.WaitForElement("TitleEntry");
		App.ClearText("TitleEntry");
		App.EnterText("TitleEntry", "Aligned Title");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(17)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetVerticalTextAlignmentAndSelectedItem_VerifySelectedItem()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VerticalTextAlignmentEnd");
		App.Tap("VerticalTextAlignmentEnd");
		App.WaitForElement("SelectedItemEntry");
		App.ClearText("SelectedItemEntry");
		App.EnterText("SelectedItemEntry", "Option 3 - Third option");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(18)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetVerticalTextAlignmentEndAndTitle_VerifyTitleAlignment()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("VerticalTextAlignmentEnd");
		App.Tap("VerticalTextAlignmentEnd");
		App.WaitForElement("TitleEntry");
		App.ClearText("TitleEntry");
		App.EnterText("TitleEntry", "Aligned Title");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(19)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetShadow_VerifyShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowTrueButton");
		App.Tap("ShadowTrueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(20)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetTextColorRed_VerifyTextColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRedButton");
		App.Tap("TextColorRedButton");
		App.WaitForElement("SelectedIndexEntry");
		App.ClearText("SelectedIndexEntry");
		App.EnterText("SelectedIndexEntry", "2");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(21)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetTitle_VerifyTitleLabel()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TitleEntry");
		App.ClearText("TitleEntry");
		App.EnterText("TitleEntry", "Choose Option");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(22)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetTitleColorOrange_VerifyTitleColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TitleColorOrangeButton");
		App.Tap("TitleColorOrangeButton");
		App.WaitForElement("TitleEntry");
		App.ClearText("TitleEntry");
		App.EnterText("TitleEntry", "Choose Option");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(23)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetTitleWithFontSize_VerifyTitleAndFontSize()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TitleEntry");
		App.ClearText("TitleEntry");
		App.EnterText("TitleEntry", "Styled Title");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "22");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(24)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetTitleWithFontAttributeBold_VerifyTitleAndFontAttribute()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TitleEntry");
		App.ClearText("TitleEntry");
		App.EnterText("TitleEntry", "Styled Title");
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(25)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetTitleWithFontFamilyDokdo_VerifyTitleAndFontFamily()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TitleEntry");
		App.ClearText("TitleEntry");
		App.EnterText("TitleEntry", "Styled Title");
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID // Issue Link - https://github.com/dotnet/maui/issues/4818
	[Test, Order(26)]
	[Category(UITestCategories.Material3)]
	public void Material3Picker_SetItemDisplayBindingName_VerifyItemDisplay()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemDisplayNameButton");
		App.Tap("ItemDisplayNameButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Picker");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif
}
#endif
