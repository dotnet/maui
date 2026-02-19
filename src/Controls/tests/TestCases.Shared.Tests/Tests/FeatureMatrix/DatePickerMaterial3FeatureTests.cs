#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class DatePickerMaterial3FeatureTests : _GalleryUITest
{
	public override string GalleryPageName => "Date Picker Feature Matrix";

	public DatePickerMaterial3FeatureTests(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_InitialState_VerifyVisualState()
	{
		App.WaitForElement("DatePickerControl");
		App.Tap("DatePickerControl");
		App.WaitForElement("OK");
		App.Tap("OK");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	[Category(UITestCategories.DatePicker)]
	public void Material3DatePicker_ModifyOldDateAndNewDate_VerifyVisualState()
	{
		App.WaitForElement("DatePickerControl");
		App.Tap("DatePickerControl");
		App.Tap("26");
		App.WaitForElement("OK");
		App.Tap("OK");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(3)]
	[Category(UITestCategories.DatePicker)]
	public void Material3DatePicker_OldDateAndNewDate_VerifyVisualState()
	{
		App.WaitForElement("DatePickerControl");
		App.Tap("DatePickerControl");
		App.WaitForElement("26");
		App.Tap("26");
		App.WaitForElement("OK");
		App.Tap("OK");
		App.WaitForElement("DatePickerControl");
		App.Tap("DatePickerControl");
		App.WaitForElement("28");
		App.Tap("28");
		App.WaitForElement("Cancel");
		App.Tap("Cancel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(4)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetCharacterSpacingAndDate_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacingEntry");
		App.ClearText("CharacterSpacingEntry");
		App.EnterText("CharacterSpacingEntry", "5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(5)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_DateSelectedEvent_FiresOnDateChange()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DateEntry");
		App.ClearText("DateEntry");
		App.EnterText("DateEntry", "12/15/2026");
		App.WaitForElement("SetDateButton");
		App.Tap("SetDateButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(6)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetDateAndTextColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorGreenButton");
		App.Tap("TextColorGreenButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(7)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetDateAndFlowDirection_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(8)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetFontAttributesAndFontFamily_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesItalicButton");
		App.Tap("FontAttributesItalicButton");
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(9)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetFontAttributesAndFontSize_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesItalicButton");
		App.Tap("FontAttributesItalicButton");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(10)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetFontFamilyAndFontSize_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(11)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetFontAttributesAndFormat_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesItalicButton");
		App.Tap("FontAttributesItalicButton");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "D");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(12)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetFontFamilyAndFormat_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "D");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetFontSizeAndFormat_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "30");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "D");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		App.WaitForElement("CultureFormatLabel");
		App.Tap("CultureFormatLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(14)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetDateAndIsEnabled_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledFalseRadioButton");
		App.Tap("IsEnabledFalseRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		App.WaitForElement("DatePickerControl");
		App.Tap("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(15)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetDateAndIsVisible_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseRadioButton");
		App.Tap("IsVisibleFalseRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(16)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetDateAndShadowOpacity_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(17)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetMinimumDateAndDate_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MinimumDateEntry");
		App.ClearText("MinimumDateEntry");
		App.EnterText("MinimumDateEntry", "12/24/2024");
		App.WaitForElement("SetMinimumDateButton");
		App.Tap("SetMinimumDateButton");
		App.WaitForElement("DateEntry");
		App.ClearText("DateEntry");
		App.EnterText("DateEntry", "12/24/2023");
		App.WaitForElement("SetDateButton");
		App.Tap("SetDateButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		App.WaitForElement("CultureFormatLabel");
		App.Tap("CultureFormatLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(18)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetMaximumDateAndDate_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MaximumDateEntry");
		App.ClearText("MaximumDateEntry");
		App.EnterText("MaximumDateEntry", "12/24/2028");
		App.WaitForElement("SetMaximumDateButton");
		App.Tap("SetMaximumDateButton");
		App.WaitForElement("DateEntry");
		App.ClearText("DateEntry");
		App.EnterText("DateEntry", "12/24/2031");
		App.WaitForElement("SetDateButton");
		App.Tap("SetDateButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		App.WaitForElement("CultureFormatLabel");
		App.Tap("CultureFormatLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(19)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_Format_D_LongDatePattern()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "D");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(20)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_Format_f_FullDateShortTime()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "f");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(21)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_Format_F_FullDateLongTime()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "F");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(22)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetFontAttributesAndFormat_f_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesItalicButton");
		App.Tap("FontAttributesItalicButton");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "f");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(23)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetFontFamilyAndFormat_f_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "f");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(24)]
	[Category(UITestCategories.Material3)]
	public void Material3DatePicker_SetFontSizeAndFormat_f_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "30");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "f");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		App.WaitForElement("CultureFormatLabel");
		App.Tap("CultureFormatLabel");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
