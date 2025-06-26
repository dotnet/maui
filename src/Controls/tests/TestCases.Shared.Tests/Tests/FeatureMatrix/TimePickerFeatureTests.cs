using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class TimePickerFeatureTests : UITest
{
    public const string TimePickerFeatureMatrix = "Time Picker Feature Matrix";
    public TimePickerFeatureTests(TestDevice testDevice) : base(testDevice)
    {
    }
    protected override void FixtureSetup()
    {
        base.FixtureSetup();
        App.NavigateToGallery(TimePickerFeatureMatrix);
    }

    [Test, Order(1)]
    [Category(UITestCategories.TimePicker)]
    public void TimePicker_InitialState_VerifyVisualState()
    {
        App.WaitForElement("TimePickerControl");
        App.Tap("TimePickerControl");
#if ANDROID
        App.WaitForElement("OK");
        App.Tap("OK");
#elif IOS
        App.WaitForElement("Done");
        App.Tap("Done");
#elif WINDOWS
        App.Tap("5");
#endif
        VerifyScreenshot();
    }

    [Test, Order(2)]
    [Category(UITestCategories.TimePicker)]
    public void TimePicker_ModifyOldDateAndNewDate_VerifyVisualState()
    {
        App.WaitForElement("TimePickerControl");
        App.Tap("TimePickerControl");
#if ANDROID
        App.Tap("6");
        App.WaitForElement("OK");
        App.Tap("OK");
        Assert.That(App.WaitForElement("NewTimeSelectedLabel").GetText(), Is.EqualTo("06:00:00"));
        Assert.That(App.WaitForElement("OldTimeSelectedLabel").GetText(), Is.EqualTo("10:00:00"));
#elif IOS
        App.WaitForElement("Done");
        App.Tap("Done");
#elif WINDOWS
        App.Tap("66");
#endif
        VerifyScreenshot();
    }

    [Test, Order(3)]
    [Category(UITestCategories.TimePicker)]
    public void TimePicker_OldDateAndNewDate_VerifyVisualState()
    {
        App.WaitForElement("TimePickerControl");
        App.Tap("TimePickerControl");
#if ANDROID
        App.WaitForElement("7");
        App.Tap("7");
        App.WaitForElement("OK");
        App.Tap("OK");
        App.WaitForElement("TimePickerControl");
        App.Tap("TimePickerControl");
        App.WaitForElement("8");
        App.Tap("8");
        App.WaitForElement("Cancel");
        App.Tap("Cancel");
        Assert.That(App.WaitForElement("NewTimeSelectedLabel").GetText(), Is.EqualTo("07:00:00"));
        Assert.That(App.WaitForElement("OldTimeSelectedLabel").GetText(), Is.EqualTo("06:00:00"));
#elif IOS
        App.WaitForElement("Done");
        App.Tap("Done");
#elif WINDOWS
        App.Tap("7");
#endif
        VerifyScreenshot();
    }

#if TEST_FAILS_ON_MACCATALYST && TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/30199
    [Test]
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

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_MACCATALYST && TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/30192
    [Test]
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

#if TEST_FAILS_ON_MACCATALYST
    [Test]
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

#if TEST_FAILS_ON_MACCATALYST
    [Test]
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

#if TEST_FAILS_ON_MACCATALYST
    [Test]
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

#if TEST_FAILS_ON_MACCATALYST
    [Test]
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
        VerifyScreenshot();
    }
#endif

#if TEST_FAILS_ON_MACCATALYST
    [Test]
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

#if TEST_FAILS_ON_MACCATALYST
    [Test]
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
        VerifyScreenshot();
    }
#endif

#if TEST_FAILS_ON_MACCATALYST
    [Test]
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
        VerifyScreenshot();
    }
#endif

    [Test]
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

    [Test]
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

#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/29812
    [Test]
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

    [Test]
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

    [Test]
    [Category(UITestCategories.TimePicker)]
    public void TimePicker_SetFormat_T_AndTime_VerifyVisualState()
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

    [Test]
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

    [Test]
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

    [Test]
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
        VerifyScreenshot();
    }

    [Test]
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
    }

    [Test]
    [Category(UITestCategories.TimePicker)]
    public void TimePicker_SetCulture_frFR_VerifyTimeFormat()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("CultureFRButton");
        App.Tap("CultureFRButton");
        App.WaitForElement("TimeEntry");
        App.ClearText("TimeEntry");
        App.EnterText("TimeEntry", "17:30");
        App.WaitForElement("SetTimeButton");
        App.Tap("SetTimeButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("TimePickerControl");
        var cultureFormatText = App.WaitForElement("CultureFormatLabel").GetText();
        Assert.That(cultureFormatText, Is.EqualTo("Culture: fr-FR, Time: 17:30"));
    }

    [Test]
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
    }
}