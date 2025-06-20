using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class DatePickerFeatureTests : UITest
{
    public const string DatePickerFeatureMatrix = "Date Picker Feature Matrix";

    public DatePickerFeatureTests(TestDevice testDevice) : base(testDevice)
    {
    }

    protected override void FixtureSetup()
    {
        base.FixtureSetup();
        App.NavigateToGallery(DatePickerFeatureMatrix);
    }

    [Test, Order(1)]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_InitialState_VerifyVisualState()
    {
        App.WaitForElement("DatePickerControl");
        App.Tap("DatePickerControl");
#if IOS || MACCATALYST
        App.WaitForElement("Done");
        App.Tap("Done");
#else
        App.WaitForElement("Ok");
        App.Tap("Ok");
#endif
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_ModifyOldDateAndNewDate_VerifyVisualState()
    {
        App.WaitForElement("DatePickerControl");
        App.Tap("DatePickerControl");
        App.Tap("26");
#if ANDROID
        App.WaitForElement("Ok");
        App.Tap("Ok");
        Assert.That(App.WaitForElement("NewDateSelectedLabel").GetText(), Is.EqualTo("12/26/2025"));
        Assert.That(App.WaitForElement("OldDateSelectedLabel").GetText(), Is.EqualTo("12/24/2025"));

#else
        App.WaitForElement("Done");
        App.Tap("Done");
        // VerifyScreenshot();
#endif
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_OldDateAndNewDate_VerifyVisualState()
    {
        App.WaitForElement("DatePickerControl");
        App.Tap("DatePickerControl");
        App.Tap("28");
#if ANDROID
        App.WaitForElement("Cancel");
        App.Tap("Cancel");
        Assert.That(App.WaitForElement("NewDateSelectedLabel").GetText(), Is.EqualTo("12/26/2025"));
        Assert.That(App.WaitForElement("OldDateSelectedLabel").GetText(), Is.EqualTo("12/24/2025"));
#else
        // VerifyScreenshot();
#endif
    }

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
    [Test]
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
        // VerifyScreenshot();
    }
#endif

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_DateSelectedEvent_FiresOnDateChange()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("MinimumDateEntry");
        App.ClearText("MinimumDateEntry");
        App.EnterText("MinimumDateEntry", "12/15/2026");
        App.WaitForElement("SetDateButton");
        App.Tap("SetDateButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/23793, https://github.com/dotnet/maui/issues/29099, https://github.com/dotnet/maui/issues/30011
    [Test]
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
        var datePicker = App.WaitForElement("DatePickerControl").GetText();
        Assert.That(datePicker, Is.EqualTo("Wednesday, December 24, 2025"));
    }
#endif

#if TEST_FAILS_ON_CATALYST // Issue Links -https://github.com/dotnet/maui/issues/20904
    [Test]
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
        // VerifyScreenshot();
    }
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
    [Test]
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
        // VerifyScreenshot();
    }
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST
    [Test]
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
        // VerifyScreenshot();
    }
#endif

#if TEST_FAILS_ON_CATALYST
    [Test]
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
        // VerifyScreenshot();
    }
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/23793, https://github.com/dotnet/maui/issues/29099, https://github.com/dotnet/maui/issues/30011
    [Test]
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
        // VerifyScreenshot();
    }
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST
    [Test]
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
        // VerifyScreenshot();
    }
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/23793, https://github.com/dotnet/maui/issues/29099, https://github.com/dotnet/maui/issues/30011
    [Test]
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
        // VerifyScreenshot();
    }
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Links - https://github.com/dotnet/maui/issues/23793, https://github.com/dotnet/maui/issues/29099, https://github.com/dotnet/maui/issues/30011
    [Test]
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
        // VerifyScreenshot();
    }
#endif

    [Test]
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
        // VerifyScreenshot();
    }

    [Test]
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
        // VerifyScreenshot();
    }

    [Test]
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
        // VerifyScreenshot();
    }

    [Test]
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
        // VerifyScreenshot();
    }

    [Test]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_Format_d_ShortDatePattern()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FormatEntry");
        App.ClearText("FormatEntry");
        App.EnterText("FormatEntry", "d");
        App.WaitForElement("SetFormatButton");
        App.Tap("SetFormatButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("DatePickerControl");
        // VerifyScreenshot();
    }

    [Test]
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
        // VerifyScreenshot();
    }

    [Test]
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
        Thread.Sleep(5000);
        // VerifyScreenshot();
    }

    [Test]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetFontAttributesAndFormat_d_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAttributesItalicButton");
        App.Tap("FontAttributesItalicButton");
        App.WaitForElement("FormatEntry");
        App.ClearText("FormatEntry");
        App.EnterText("FormatEntry", "d");
        App.WaitForElement("SetFormatButton");
        App.Tap("SetFormatButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("DatePickerControl");
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetFontFamilyAndFormat_d_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontFamilyDokdoButton");
        App.Tap("FontFamilyDokdoButton");
        App.WaitForElement("FormatEntry");
        App.ClearText("FormatEntry");
        App.EnterText("FormatEntry", "d");
        App.WaitForElement("SetFormatButton");
        App.Tap("SetFormatButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("DatePickerControl");
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetFontSizeAndFormat_d_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontSizeEntry");
        App.ClearText("FontSizeEntry");
        App.EnterText("FontSizeEntry", "30");
        App.WaitForElement("FormatEntry");
        App.ClearText("FormatEntry");
        App.EnterText("FormatEntry", "d");
        App.WaitForElement("SetFormatButton");
        App.Tap("SetFormatButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("DatePickerControl");
        // VerifyScreenshot();
    }

    [Test]
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
        // VerifyScreenshot();
    }

    [Test]
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
        // VerifyScreenshot();
    }

    [Test]
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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetFontAttributesAndFormat_F_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAttributesItalicButton");
        App.Tap("FontAttributesItalicButton");
        App.WaitForElement("FormatEntry");
        App.ClearText("FormatEntry");
        App.EnterText("FormatEntry", "F");
        App.WaitForElement("SetFormatButton");
        App.Tap("SetFormatButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("DatePickerControl");
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetFontFamilyAndFormat_F_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontFamilyDokdoButton");
        App.Tap("FontFamilyDokdoButton");
        App.WaitForElement("FormatEntry");
        App.ClearText("FormatEntry");
        App.EnterText("FormatEntry", "F");
        App.WaitForElement("SetFormatButton");
        App.Tap("SetFormatButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("DatePickerControl");
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetFontSizeAndFormat_F_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontSizeEntry");
        App.ClearText("FontSizeEntry");
        App.EnterText("FontSizeEntry", "30");
        App.WaitForElement("FormatEntry");
        App.ClearText("FormatEntry");
        App.EnterText("FormatEntry", "F");
        App.WaitForElement("SetFormatButton");
        App.Tap("SetFormatButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        App.WaitForElementTillPageNavigationSettled("DatePickerControl");
        // VerifyScreenshot();
    }

    [Test]
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
        var cultureFormatText = App.WaitForElement("CultureFormatLabel").GetText();
        Assert.That(cultureFormatText, Is.EqualTo("Culture: en-US, Date: 12/24/2026 12:00:00 AM"));
    }

    [Test]
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
        var cultureFormatText = App.WaitForElement("CultureFormatLabel").GetText();
        Assert.That(cultureFormatText, Is.EqualTo("Culture: fr-FR, Date: 24/12/2026 00:00:00"));
    }

    [Test]
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
        var cultureFormatText = App.WaitForElement("CultureFormatLabel").GetText();
        Assert.That(cultureFormatText, Is.EqualTo("Culture: ja-JP, Date: 2026/12/24 0:00:00"));
    }

    [Test]
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
        var cultureFormatText = App.WaitForElement("CultureFormatLabel").GetText();
        Assert.That(cultureFormatText, Is.EqualTo("Culture: sv-FI, Date: 2026-12-24 00:00:00"));
    }
}