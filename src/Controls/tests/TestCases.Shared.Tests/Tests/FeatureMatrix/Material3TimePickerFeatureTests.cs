// Material3 TimePicker tests reuse the existing TimePicker Feature Matrix HostApp page.
// The native Android TimePicker uses Material3 styling (MauiMaterialTimePicker) when Material3 is enabled,
// so these tests produce separate screenshot baselines under the Material3 category.
#if ANDROID
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class Material3TimePickerFeatureTests : _GalleryUITest
{
	static AppiumQuery MaterialClockHour(string hour) => AppiumQuery.ByXPath($"//*[@text='{hour}']");
	public override string GalleryPageName => "Time Picker Feature Matrix";

	public Material3TimePickerFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_InitialState_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		App.Tap("TimePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_ModifyOldTimeAndNewTime_VerifyVisualState()
	{
		App.WaitForElement("Cancel");
		App.Tap("Cancel");
		App.WaitForElement("TimePickerControl");
		App.Tap("TimePickerControl");
		App.WaitForElement(MaterialClockHour("6"));
		App.Tap(MaterialClockHour("6"));
		App.WaitForElement("OK");
		App.Tap("OK");
		Assert.That(App.WaitForElement("NewTimeSelectedLabel").GetText(), Is.EqualTo("06:00:00"));
		Assert.That(App.WaitForElement("OldTimeSelectedLabel").GetText(), Is.EqualTo("10:00:00"));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(3)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_OldTimeAndNewTime_VerifyVisualState()
	{
		App.WaitForElement("TimePickerControl");
		App.Tap("TimePickerControl");
		App.WaitForElement(MaterialClockHour("7"));
		App.Tap(MaterialClockHour("7"));
		App.WaitForElement("OK");
		App.Tap("OK");
		App.WaitForElement("TimePickerControl");
		App.Tap("TimePickerControl");
		App.WaitForElement(MaterialClockHour("8"));
		App.Tap(MaterialClockHour("8"));
		App.WaitForElement("Cancel");
		App.Tap("Cancel");
		Assert.That(App.WaitForElement("NewTimeSelectedLabel").GetText(), Is.EqualTo("07:00:00"));
		Assert.That(App.WaitForElement("OldTimeSelectedLabel").GetText(), Is.EqualTo("06:00:00"));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(4)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetTimeAndCharacterSpacing_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CharacterSpacingEntry");
		App.ClearText("CharacterSpacingEntry");
		App.EnterText("CharacterSpacingEntry", "5");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID // Issue Link - https://github.com/dotnet/maui/issues/30192

	[Test, Order(5)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFlowDirectionAndTime_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(6)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetTimeAndTextColor_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorGreenButton");
		App.Tap("TextColorGreenButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(7)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFontAttributesAndFontFamily_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(8)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFontAttributesAndFontSize_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(9)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFontAttributesAndFormat_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(10)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFontFamilyAndFontSize_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(11)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFontFamilyAndFormat_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(12)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFontSizeAndFormat_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetTimeAndIsEnabled_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledFalseButton");
		App.Tap("IsEnabledFalseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		App.Tap("TimePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(14)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetTimeAndIsVisible_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseButton");
		App.Tap("IsVisibleFalseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("TimePickerControl");
	}

	[Test, Order(15)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetShadow_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowTrueButton");
		App.Tap("ShadowTrueButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(16)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFormat_t_AndTime_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(17)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFormatTAndTime_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(18)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFormat_T_WithFontAttributes_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(19)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFormat_T_WithFontFamily_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(20)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFormat_T_WithFontSize_VerifyVisualState()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(21)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetCulture_enUS_VerifyTimeFormat()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(22)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetCulture_arEG_VerifyTimeFormat()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(23)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetCulture_jaJP_VerifyTimeFormat()
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
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(24)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_ClearTime_UpdatesNullableTimeAndEventArgs()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CultureUSButton");
		App.Tap("CultureUSButton");
		App.WaitForElement("ClearTimeButton");
		App.Tap("ClearTimeButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");

		Assert.That(App.WaitForElement("CultureFormatLabel").GetText(), Is.EqualTo("Culture: en-US, Time: No time selected"));
		Assert.That(App.WaitForElement("NewTimeSelectedLabel").GetText(), Is.EqualTo("<null>"));
		Assert.That(App.WaitForElement("OldTimeSelectedLabel").GetText(), Is.EqualTo("10:00:00"));
	}

	[Test, Order(24)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_ClearTimeThenSetTime_RoundTripsNullableTime()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ClearTimeButton");
		App.Tap("ClearTimeButton");
		App.WaitForElement("TimeEntry");
		App.ClearText("TimeEntry");
		App.EnterText("TimeEntry", "11:30");
		App.WaitForElement("SetTimeButton");
		App.Tap("SetTimeButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");

		Assert.That(App.WaitForElement("NewTimeSelectedLabel").GetText(), Is.EqualTo("11:30:00"));
		Assert.That(App.WaitForElement("OldTimeSelectedLabel").GetText(), Is.EqualTo("<null>"));
	}

	[Test, Order(25)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_DisableFontAutoScaling_UpdatesBoundProperty()
	{
		Assert.That(App.WaitForElement("FontAutoScalingStatusLabel").GetText(), Is.EqualTo("Scale: True"));

		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAutoScalingFalseButton");
		App.Tap("FontAutoScalingFalseButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");

		Assert.That(App.WaitForElement("FontAutoScalingStatusLabel").GetText(), Is.EqualTo("Scale: False"));

		App.Tap("Options");
		App.WaitForElement("FontAutoScalingTrueButton");
		App.Tap("FontAutoScalingTrueButton");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");

		Assert.That(App.WaitForElement("FontAutoScalingStatusLabel").GetText(), Is.EqualTo("Scale: True"));
	}

	[Test, Order(26)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_IsOpenAndEvents_StaySynchronized()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		App.WaitForElement("OpenTimePickerButton");
		App.Tap("OpenTimePickerButton");

#if ANDROID
		App.WaitForElement("Cancel");
		App.Tap("Cancel");
#elif IOS
		App.WaitForElement("Done");
		App.Tap("Done");
#elif WINDOWS
		App.WaitForElement("DismissButton");
		App.Tap("DismissButton");
#else
		App.TapCoordinates(10, 10);
#endif

		Assert.That(App.WaitForElement("IsOpenStatusLabel").GetText(), Is.EqualTo("IsOpen: False"));
		Assert.That(App.WaitForElement("OpenedCountLabel").GetText(), Is.EqualTo("Opened: 1"));
		Assert.That(App.WaitForElement("ClosedCountLabel").GetText(), Is.EqualTo("Closed: 1"));
		App.WaitForElement("OpenTimePickerButton");
		App.Tap("OpenTimePickerButton");
#if ANDROID
		App.WaitForElement("OK");
		App.Tap("OK");
#elif IOS
		App.WaitForElement("Done");
		App.Tap("Done");
#elif WINDOWS
		App.WaitForElement("AcceptButton");
		App.Tap("AcceptButton");
#else
		App.TapCoordinates(10, 10);
#endif

		Assert.That(App.WaitForElement("IsOpenStatusLabel").GetText(), Is.EqualTo("IsOpen: False"));
		Assert.That(App.WaitForElement("OpenedCountLabel").GetText(), Is.EqualTo("Opened: 2"));
		Assert.That(App.WaitForElement("ClosedCountLabel").GetText(), Is.EqualTo("Closed: 2"));
	}

	[Test, Order(27)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetTimeAndTextColorRed_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TextColorRedButton");
		App.Tap("TextColorRedButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(28)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFontAttributesBold_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesBoldButton");
		App.Tap("FontAttributesBoldButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(29)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFontAttributesNone_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesItalicButton");
		App.Tap("FontAttributesItalicButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontAttributesNoneButton");
		App.Tap("FontAttributesNoneButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(30)]
	[Category(UITestCategories.Material3)]
	public void Material3TimePicker_SetFontFamilyMontserratBold_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FontFamilyMontserratBoldButton");
		App.Tap("FontFamilyMontserratBoldButton");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElementTillPageNavigationSettled("TimePickerControl");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
