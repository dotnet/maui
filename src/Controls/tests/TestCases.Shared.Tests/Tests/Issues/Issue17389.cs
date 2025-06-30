using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17389 : _IssuesUITest
{
    public Issue17389(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "InputTransparent should not affect background color on Windows layouts";

    [Test, Order(1)]
    [Category(UITestCategories.Layout)]
    public void ValidateBackgroundColorDoesNotAffectInputTransparent()
    {
        App.WaitForElement("ToggleInputTransparentButton");

        string[] layouts = new[]
        {
             "RedGrid",
             "GreenGrid",
             "BlueBorder",
             "PurpleContent"
        };

        foreach (var layout in layouts)
        {
            App.Tap(layout);
        }

        App.WaitForElement("Tap count: 4");
        App.Click("ToggleInputTransparentButton");

        foreach (var layout in layouts)
        {
            App.Tap(layout);
        }

        App.WaitForElement("Tap count: 0");
    }

    [Test, Order(2)]
    [Category(UITestCategories.Layout)]
    public void ValidateInputTransparentBackgroundColorToggle()
    {
        Exception? exception = null;

        App.WaitForElement("ToggleBackgroundColorsButton");
        VerifyScreenshotOrSetException(ref exception, "BeforeToggleBackgroundColors");
        App.WaitForElement("ToggleBackgroundColorsButton");
        App.Tap("ToggleBackgroundColorsButton");
        VerifyScreenshotOrSetException(ref exception, "AfterToggleBackgroundColors");

        if (exception != null)
        {
            throw exception;
        }
    }
}