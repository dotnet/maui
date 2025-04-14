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
    public void DatePicker_SetCharacterSpacingAndDate_VerifyMaximumLabel()
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
    public void DatePicker_SetDateAndFontAttributesVerify()
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
    public void DatePicker_SetDateAndFontFamilyVerify()
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
    public void DatePicker_SetDateAndFontSizeVerify()
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
    public void DatePicker_SetDateAndFormatVerify()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FormatEntry");
        App.ClearText("FormatEntry");
        App.EnterText("FormatEntry", "MM/dd/yyyy");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetDateAndTextColorVerify()
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
    public void DatePicker_SetFontAttributesAndFontFamilyVerify()
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
    public void DatePicker_SetFontAttributesAndFontSizeVerify()
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
    public void DatePicker_SetFontAttributesAndFormatVerify()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontAttributesItalicButton");
        App.Tap("FontAttributesItalicButton");
        App.WaitForElement("FormatEntry");
        App.ClearText("FormatEntry");
        App.EnterText("FormatEntry", "MM/dd/yyyy");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetFontFamilyAndFontSizeVerify()
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
    public void DatePicker_SetFontFamilyAndFormatVerify()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontFamilyDokdoButton");
        App.Tap("FontFamilyDokdoButton");
        App.WaitForElement("FormatEntry");
        App.ClearText("FormatEntry");
        App.EnterText("FormatEntry", "MM/dd/yyyy");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }
    [Test]
    [Category(UITestCategories.DatePicker)]
    public void DatePicker_SetFontSizeAndFormatVerify()
    {
        App.WaitForElement("Options");
        App.Tap("Options");
        App.WaitForElement("FontSizeEntry");
        App.ClearText("FontSizeEntry");
        App.EnterText("FontSizeEntry", "20");
        App.WaitForElement("FormatEntry");
        App.ClearText("FormatEntry");
        App.EnterText("FormatEntry", "MM/dd/yyyy");
        App.WaitForElement("Apply");
        App.Tap("Apply");
        // VerifyScreenshot();
    }

    // [Test]
    // [Category(UITestCategories.DatePicker)]
    // public void DatePicker_SetMinimumDateAndDateVerify()
    // {
    //     App.WaitForElement("Options");
    //     App.Tap("Options");
    //     App.WaitForElement("MinimumDateEntry");
    //     App.ClearText("MinimumDateEntry");
    //     App.EnterText("MinimumDateEntry", DateTime.Now.AddYears(-1).ToString("MM/dd/yyyy hh:mm:ss tt"));
    //     App.WaitForElement("Apply");
    //     App.Tap("Apply");
    //     App.WaitForElement("DatePickerControl");
    //     App.Tap("DatePickerControl");
    //     App.EnterText("DatePickerControl", DateTime.Now.AddYears(-2).ToString("MM/dd/yyyy hh:mm:ss tt"));
    //     App.DismissKeyboard();
    //     Task.Delay(2000).Wait();
    //     var selectedDate = App.Query("DatePickerControl").FirstOrDefault()?.Text;
    //     // Assert.AreNotEqual("04/10/2024", selectedDate, "DatePicker allowed a date below MinimumDate!");
    //     // VerifyScreenshot();
    // }

    // [Test]
    // [Category(UITestCategories.DatePicker)]
    // public void DatePicker_SetMaximumDateAndDateVerify()
    // {
    //     App.WaitForElement("Options");
    //     App.Tap("Options");
    //     App.WaitForElement("MaximumDateEntry");
    //     App.ClearText("MaximumDateEntry");
    //     App.EnterText("MaximumDateEntry", DateTime.Now.AddYears(1).ToString("MM/dd/yyyy hh:mm:ss tt"));
    //     App.WaitForElement("Apply");
    //     App.Tap("Apply");
    //     App.WaitForElement("DatePickerControl");
    //     App.Tap("DatePickerControl");
    //     App.EnterText("DatePickerControl", DateTime.Now.AddYears(2).ToString("MM/dd/yyyy hh:mm:ss tt"));
    //     App.DismissKeyboard();
    //     // var selectedDate = App.Query("DatePickerControl").FirstOrDefault()?.Text;
    //     // Assert.AreNotEqual("04/10/2024", selectedDate, "DatePicker allowed a date below MinimumDate!");
    //     // VerifyScreenshot();
    //     // VerifyScreenshot();
    // }
}