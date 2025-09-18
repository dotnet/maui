using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class DatePickerFeatureTests : _GalleryUITest
{
	public const string DatePickerFeatureMatrix = "Date Picker Feature Matrix";

	public override string GalleryPageName => DatePickerFeatureMatrix;

	public DatePickerFeatureTests(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_InitialState_VerifyVisualState()
	{
		App.WaitForElement("DatePickerControl");
		App.Tap("DatePickerControl");
#if ANDROID
		App.WaitForElement("OK");
		App.Tap("OK");
#elif IOS
        App.WaitForElement("Done");
        App.Tap("Done");
#elif WINDOWS
        App.Tap("25");
#endif
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_ModifyOldDateAndNewDate_VerifyVisualState()
	{
		App.WaitForElement("DatePickerControl");
		App.Tap("DatePickerControl");
#if ANDROID
		App.Tap("26");
		App.WaitForElement("OK");
		App.Tap("OK");
		Assert.That(App.WaitForElement("NewDateSelectedLabel").GetText(), Is.EqualTo("12/26/2025 12:00:00 AM"));
		Assert.That(App.WaitForElement("OldDateSelectedLabel").GetText(), Is.EqualTo("12/24/2025 12:00:00 AM"));
#elif IOS
        App.WaitForElement("Done");
        App.Tap("Done");
#elif WINDOWS
        App.Tap("26");
#endif
		VerifyScreenshot();
	}

	[Test, Order(3)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_OldDateAndNewDate_VerifyVisualState()
	{
		App.WaitForElement("DatePickerControl");
		App.Tap("DatePickerControl");
#if ANDROID
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
		Assert.That(App.WaitForElement("NewDateSelectedLabel").GetText(), Is.EqualTo("12/26/2025 12:00:00 AM"));
		Assert.That(App.WaitForElement("OldDateSelectedLabel").GetText(), Is.EqualTo("12/24/2025 12:00:00 AM"));
#elif IOS
        App.WaitForElement("Done");
        App.Tap("Done");
#elif WINDOWS
        App.Tap("27");
#endif
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/30066
	[Test, Order(4)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetCharacterSpacingAndDate_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacingEntry");
		App.ClearText("CharacterSpacingEntry");
		App.EnterText("CharacterSpacingEntry", "5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot();
	}
#endif

	[Test, Order(5)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_DateSelectedEvent_FiresOnDateChange()
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
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/23793, https://github.com/dotnet/maui/issues/29099, https://github.com/dotnet/maui/issues/30011
	[Test, Order(6)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetDateAndFormat_VerifyVisualState()
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
#if ANDROID
		var datePicker = App.WaitForElement("DatePickerControl").GetText();
		Assert.That(datePicker, Is.EqualTo("Wednesday, December 24, 2025"));
#else
        VerifyScreenshot();
#endif
	}
#endif

#if TEST_FAILS_ON_CATALYST // Issue Links -https://github.com/dotnet/maui/issues/20904
	[Test, Order(7)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetDateAndTextColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorGreenButton");
		App.Tap("TextColorGreenButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Issue Links - https://github.com/dotnet/maui/issues/30065
	[Test, Order(8)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetDateAndFlowDirection_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST
	[Test, Order(9)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetFontAttributesAndFontFamily_VerifyVisualState()
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
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST
	[Test, Order(10)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetFontAttributesAndFontSize_VerifyVisualState()
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
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/23793, https://github.com/dotnet/maui/issues/29099, https://github.com/dotnet/maui/issues/30011
	[Test, Order(11)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetFontAttributesAndFormat_VerifyVisualState()
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
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST
	[Test, Order(12)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetFontFamilyAndFontSize_VerifyVisualState()
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
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/23793, https://github.com/dotnet/maui/issues/29099, https://github.com/dotnet/maui/issues/30011
	[Test, Order(13)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetFontFamilyAndFormat_VerifyVisualState()
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
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/23793, https://github.com/dotnet/maui/issues/29099, https://github.com/dotnet/maui/issues/30011
	[Test, Order(14)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetFontSizeAndFormat_VerifyVisualState()
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
		VerifyScreenshot();
	}
#endif

	[Test, Order(15)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetDateAndIsEnabled_VerifyVisualState()
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
		VerifyScreenshot();
	}

	[Test, Order(16)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetDateAndIsVisible_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseRadioButton");
		App.Tap("IsVisibleFalseRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("DatePickerControl");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/29812
	[Test, Order(17)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetDateAndShadowOpacity_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowTrueRadioButton");
		App.Tap("ShadowTrueRadioButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
		VerifyScreenshot();
	}
#endif

	[Test, Order(18)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetMinimumDateAndDate_VerifyVisualState()
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
		VerifyScreenshot();
	}

	[Test, Order(19)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetMaximumDateAndDate_VerifyVisualState()
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
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/30011
	[Test, Order(20)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_Format_D_LongDatePattern()
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
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/30154
	[Test, Order(21)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_Format_f_FullDateShortTime()
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
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/30154
	[Test, Order(22)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_Format_F_FullDateLongTime()
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
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/30154
	[Test, Order(23)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetFontAttributesAndFormat_f_VerifyVisualState()
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
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/30154
	[Test, Order(24)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetFontFamilyAndFormat_f_VerifyVisualState()
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
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/30154
	[Test, Order(25)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetFontSizeAndFormat_f_VerifyVisualState()
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
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/30090
	[Test, Order(26)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetCulture_enUS_VerifyDateFormat()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CultureUsButton");
		App.Tap("CultureUsButton");
		App.WaitForElement("DateEntry");
		App.ClearText("DateEntry");
		App.EnterText("DateEntry", "12/24/2026");
		App.WaitForElement("SetDateButton");
		App.Tap("SetDateButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
#if ANDROID
		var cultureFormatText = App.WaitForElement("CultureFormatLabel").GetText();
		Assert.That(cultureFormatText, Is.EqualTo("Culture: en-US, Date: 12/24/2026 12:00:00 AM"));
#else
        VerifyScreenshot();
#endif
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/30090
	[Test, Order(27)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetCulture_frFR_VerifyDateFormat()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CultureFrButton");
		App.Tap("CultureFrButton");
		App.WaitForElement("DateEntry");
		App.ClearText("DateEntry");
		App.EnterText("DateEntry", "12/24/2026");
		App.WaitForElement("SetDateButton");
		App.Tap("SetDateButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
#if ANDROID
		var cultureFormatText = App.WaitForElement("CultureFormatLabel").GetText();
		Assert.That(cultureFormatText, Is.EqualTo("Culture: fr-FR, Date: 24/12/2026 00:00:00"));
#else
        VerifyScreenshot();
#endif
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/30090
	[Test, Order(28)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetCulture_jaJP_VerifyDateFormat()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CultureJpButton");
		App.Tap("CultureJpButton");
		App.WaitForElement("DateEntry");
		App.ClearText("DateEntry");
		App.EnterText("DateEntry", "12/24/2026");
		App.WaitForElement("SetDateButton");
		App.Tap("SetDateButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
#if ANDROID
		var cultureFormatText = App.WaitForElement("CultureFormatLabel").GetText();
		Assert.That(cultureFormatText, Is.EqualTo("Culture: ja-JP, Date: 2026/12/24 0:00:00"));
#else
        VerifyScreenshot();
#endif
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/30090
	[Test, Order(29)]
	[Category(UITestCategories.DatePicker)]
	public void DatePicker_SetCulture_svFI_VerifyDateFormat()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CultureFiButton");
		App.Tap("CultureFiButton");
		App.WaitForElement("DateEntry");
		App.ClearText("DateEntry");
		App.EnterText("DateEntry", "12/24/2026");
		App.WaitForElement("SetDateButton");
		App.Tap("SetDateButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("DatePickerControl");
#if ANDROID
		var cultureFormatText = App.WaitForElement("CultureFormatLabel").GetText();
		Assert.That(cultureFormatText, Is.EqualTo("Culture: sv-FI, Date: 2026-12-24 00:00:00"));
#else
        VerifyScreenshot();
#endif
	}
#endif
}