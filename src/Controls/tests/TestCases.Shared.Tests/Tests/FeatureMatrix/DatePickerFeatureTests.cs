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
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetDateAndFontAttributes_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAttributesItalicButton");
        App.Tap("FontAttributesItalicButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetDateAndFontFamily_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontFamilyDokdoButton");
        App.Tap("FontFamilyDokdoButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetDateAndFontSize_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontSizeEntry");
        App.ClearText("FontSizeEntry");
        App.EnterText("FontSizeEntry", "20");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetDateAndFormat_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FormatEntry");
        App.ClearText("FormatEntry");
        App.EnterText("FormatEntry", "D");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        Task.Delay(2000).Wait();
        // VerifyScreenshot();
    }

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
        // VerifyScreenshot();
    }

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
        // VerifyScreenshot();
    }

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
        // VerifyScreenshot();
    }

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
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }

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
        // VerifyScreenshot();
    }

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
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetFontSizeAndFormat_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontSizeEntry");
        App.ClearText("FontSizeEntry");
        App.EnterText("FontSizeEntry", "20");
        App.WaitForElement("FormatEntry");
        App.ClearText("FormatEntry");
        App.EnterText("FormatEntry", "D");
        App.WaitForElement("Apply");
        App.Tap("Apply");
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
        App.EnterText("MinimumDateEntry", "01/01/2024");
        App.WaitForElement("SetMinimumDateButton");
        App.Tap("SetMinimumDateButton");
        App.WaitForElement("DateEntry");
        App.ClearText("DateEntry");
        App.EnterText("DateEntry", "01/01/2023");
        App.WaitForElement("SetDateButton");
        App.Tap("SetDateButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        Task.Delay(2000).Wait();
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
        App.EnterText("MaximumDateEntry", "01/01/2027");
        App.WaitForElement("SetMaximumDateButton");
        App.Tap("SetMaximumDateButton");
        App.WaitForElement("DateEntry");
        App.ClearText("DateEntry");
        App.EnterText("DateEntry", "01/01/2028");
        App.WaitForElement("SetDateButton");
        App.Tap("SetDateButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        Task.Delay(2000).Wait();
        // VerifyScreenshot();
    }

    //MinimumDate and MaximumDate are same date. Try to set Date is less than MinimumDate
    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetMinimumDateAndMaximumDate_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("MinimumDateEntry");
        App.ClearText("MinimumDateEntry");
        App.EnterText("MinimumDateEntry", "01/01/2025");
        App.WaitForElement("SetMinimumDateButton");
        App.Tap("SetMinimumDateButton");
        App.WaitForElement("MaximumDateEntry");
        App.ClearText("MaximumDateEntry");
        App.EnterText("MaximumDateEntry", "01/01/2025");
        App.WaitForElement("SetMaximumDateButton");
        App.Tap("SetMaximumDateButton");
        App.WaitForElement("DateEntry");
        App.ClearText("DateEntry");
        App.EnterText("DateEntry", "01/01/2024");
        App.WaitForElement("SetDateButton");
        App.Tap("SetDateButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }

    //MinimumDate and MaximumDate are same date. Try to set Date is greater than MaximumDate
    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetMaximumDateAndMinimumDate_VerifyVisualState()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("MinimumDateEntry");
        App.ClearText("MinimumDateEntry");
        App.EnterText("MinimumDateEntry", "01/01/2025");
        App.WaitForElement("SetMinimumDateButton");
        App.Tap("SetMinimumDateButton");
        App.WaitForElement("MaximumDateEntry");
        App.ClearText("MaximumDateEntry");
        App.EnterText("MaximumDateEntry", "01/01/2025");
        App.WaitForElement("SetMaximumDateButton");
        App.Tap("SetMaximumDateButton");
        App.WaitForElement("DateEntry");
        App.ClearText("DateEntry");
        App.EnterText("DateEntry", "01/01/2026");
        App.WaitForElement("SetDateButton");
        App.Tap("SetDateButton");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
}