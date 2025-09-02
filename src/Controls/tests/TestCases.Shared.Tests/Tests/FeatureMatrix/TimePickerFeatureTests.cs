using System;
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class TimePickerFeatureTests : _GalleryUITest
{
	public const string TimePickerFeatureMatrix = "Time Picker Feature Matrix";
	public override string GalleryPageName => TimePickerFeatureMatrix;

	public TimePickerFeatureTests(TestDevice testDevice) : base(testDevice)
	{
	}

#if TEST_FAILS_ON_IOS // Issue Link - https://github.com/dotnet/maui/issues/30837

#if TEST_FAILS_ON_CATALYST

	[Test, Order(1)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_InitialState_VerifyVisualState()
	{
		App.WaitForElement("TimePickerControl");
		App.Tap("TimePickerControl");
#if IOS
        VerifyScreenshot();
        App.WaitForElement("Done");
        App.Tap("Done");
#else
		VerifyScreenshot();
#endif
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/30199

	[Test, Order(4)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetTimeAndCharacterSpacing_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacingEntry");
		App.ClearText("CharacterSpacingEntry");
		App.EnterText("CharacterSpacingEntry", "5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST

	[Test, Order(6)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetTimeAndTextColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorGreenButton");
		App.Tap("TextColorGreenButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST

	[Test, Order(7)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetFontAttributesAndFontFamily_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesItalicButton");
		App.Tap("FontAttributesItalicButton");
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST

	[Test, Order(8)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetFontAttributesAndFontSize_VerifyVisualState()
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
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST

	[Test, Order(10)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetFontFamilyAndFontSize_VerifyVisualState()
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
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot();
	}
#endif

	[Test, Order(13)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetTimeAndIsEnabled_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledFalseButton");
		App.Tap("IsEnabledFalseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		App.Tap("TimePickerControl");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/29812

	[Test, Order(15)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetShadow_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowTrueButton");
		App.Tap("ShadowTrueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST // For more information : https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/timepicker?view=net-maui-9.0&tabs=macios#tabpanel_1_macios

	[Test, Order(16)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetFormat_t_AndTime_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "t");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // For more information : https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/timepicker?view=net-maui-9.0&tabs=macios#tabpanel_1_macios

	[Test, Order(17)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetFormatTAndTime_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "T");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // For more information : https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/timepicker?view=net-maui-9.0&tabs=macios#tabpanel_1_macios

	[Test, Order(18)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetFormat_T_WithFontAttributes_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "T");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("FontAttributesItalicButton");
		App.Tap("FontAttributesItalicButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // For more information : https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/timepicker?view=net-maui-9.0&tabs=macios#tabpanel_1_macios

	[Test, Order(19)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetFormat_T_WithFontFamily_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "T");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // For more information : https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/timepicker?view=net-maui-9.0&tabs=macios#tabpanel_1_macios

	[Test, Order(20)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetFormat_T_WithFontSize_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "T");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		App.WaitForElement("CultureFormatLabel");
		App.Tap("CultureFormatLabel");
		VerifyScreenshot();
	}
#endif

#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST

	[Test, Order(2)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_ModifyOldTimeAndNewTime_VerifyVisualState()
	{
#if ANDROID
		App.WaitForElement("OK");
		App.Tap("OK");
		App.WaitForElement("TimePickerControl");
		App.Tap("TimePickerControl");
		App.WaitForElement(AppiumQuery.ByAccessibilityId("6"));
		App.Tap(AppiumQuery.ByAccessibilityId("6"));
		App.WaitForElement("OK");
		App.Tap("OK");
#elif WINDOWS
        App.WaitForElement("AcceptButton");
        App.Tap("AcceptButton");
        App.WaitForElement("TimePickerControl");
        App.Tap("TimePickerControl");
        App.WaitForElement("6");
        App.Tap("6");
        App.WaitForElement("AcceptButton");
        App.Tap("AcceptButton");
#endif
		Assert.That(App.WaitForElement("NewTimeSelectedLabel").GetText(), Is.EqualTo("06:00:00"));
		Assert.That(App.WaitForElement("OldTimeSelectedLabel").GetText(), Is.EqualTo("10:00:00"));
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST

	[Test, Order(3)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_OldTimeAndNewTime_VerifyVisualState()
	{
#if ANDROID
		App.WaitForElement("TimePickerControl");
		App.Tap("TimePickerControl");
		App.WaitForElement(AppiumQuery.ByAccessibilityId("7"));
		App.Tap(AppiumQuery.ByAccessibilityId("7"));
		App.WaitForElement("OK");
		App.Tap("OK");
		App.WaitForElement("TimePickerControl");
		App.Tap("TimePickerControl");
		App.WaitForElement(AppiumQuery.ByAccessibilityId("8"));
		App.Tap(AppiumQuery.ByAccessibilityId("8"));
		App.WaitForElement("Cancel");
		App.Tap("Cancel");
#elif WINDOWS
        App.WaitForElement("TimePickerControl");
        App.Tap("TimePickerControl");
        App.WaitForElement("7");
        App.Tap("7");
        App.WaitForElement("AcceptButton");
        App.Tap("AcceptButton");
        App.WaitForElement("TimePickerControl");
        App.Tap("TimePickerControl");
        App.WaitForElement("8");
        App.Tap("8");
        App.WaitForElement("DismissButton");
        App.Tap("DismissButton");
#endif
		Assert.That(App.WaitForElement("NewTimeSelectedLabel").GetText(), Is.EqualTo("07:00:00"));
		Assert.That(App.WaitForElement("OldTimeSelectedLabel").GetText(), Is.EqualTo("06:00:00"));
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/30192

    [Test, Order(5)]
    [Category(UITestCategories.TimePicker)]
    public void TimePicker_SetFlowDirectionAndTime_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FlowDirectionRTL");
        App.Tap("FlowDirectionRTL");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("TimePickerControl");
        VerifyScreenshot();
    }
#endif

#if TEST_FAILS_ON_CATALYST

	[Test, Order(9)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetFontAttributesAndFormat_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesItalicButton");
		App.Tap("FontAttributesItalicButton");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "HH:mm");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("TimeEntry");
		App.ClearText("TimeEntry");
		App.EnterText("TimeEntry", "17:00");
		App.WaitForElement("SetTimeButton");
		App.Tap("SetTimeButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		App.WaitForElement("CultureFormatLabel");
		App.Tap("CultureFormatLabel");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST

	[Test, Order(11)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetFontFamilyAndFormat_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyDokdoButton");
		App.Tap("FontFamilyDokdoButton");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "HH:mm");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("TimeEntry");
		App.ClearText("TimeEntry");
		App.EnterText("TimeEntry", "17:00");
		App.WaitForElement("SetTimeButton");
		App.Tap("SetTimeButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		App.WaitForElement("CultureFormatLabel");
		App.Tap("CultureFormatLabel");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST

	[Test, Order(12)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetFontSizeAndFormat_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "20");
		App.WaitForElement("FormatEntry");
		App.ClearText("FormatEntry");
		App.EnterText("FormatEntry", "hh:mm");
		App.WaitForElement("SetFormatButton");
		App.Tap("SetFormatButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		App.WaitForElement("CultureFormatLabel");
		App.Tap("CultureFormatLabel");
		VerifyScreenshot();
	}
#endif

	[Test, Order(14)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetTimeAndIsVisible_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseButton");
		App.Tap("IsVisibleFalseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("TimePickerControl");
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/30197

	[Test, Order(21)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetCulture_enUS_VerifyTimeFormat()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CultureUSButton");
		App.Tap("CultureUSButton");
		App.WaitForElement("TimeEntry");
		App.ClearText("TimeEntry");
		App.EnterText("TimeEntry", "5:30");
		App.WaitForElement("SetTimeButton");
		App.Tap("SetTimeButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		var cultureFormatText = App.WaitForElement("CultureFormatLabel").GetText();
		Assert.That(cultureFormatText, Is.EqualTo("Culture: en-US, Time: 5:30 AM"));
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/30197

	[Test, Order(22)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetCulture_arEG_VerifyTimeFormat()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CultureEGButton");
		App.Tap("CultureEGButton");
		App.WaitForElement("TimeEntry");
		App.ClearText("TimeEntry");
		App.EnterText("TimeEntry", "11:30");
		App.WaitForElement("SetTimeButton");
		App.Tap("SetTimeButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		var cultureFormatText = App.WaitForElement("CultureFormatLabel").GetText();
		Assert.That(cultureFormatText, Is.EqualTo("Culture: ar-EG, Time: 11:30 ص"));
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/30197

	[Test, Order(23)]
	[Category(UITestCategories.TimePicker)]
	public void TimePicker_SetCulture_jaJP_VerifyTimeFormat()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CultureJPButton");
		App.Tap("CultureJPButton");
		App.WaitForElement("TimeEntry");
		App.ClearText("TimeEntry");
		App.EnterText("TimeEntry", "17:30");
		App.WaitForElement("SetTimeButton");
		App.Tap("SetTimeButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		var cultureFormatText = App.WaitForElement("CultureFormatLabel").GetText();
		Assert.That(cultureFormatText, Is.EqualTo("Culture: ja-JP, Time: 17:30"));
		VerifyScreenshot();
	}
#endif
}