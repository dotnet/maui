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

    [Test]
    [Category(UITestCategories.Layout)]
    public void InputTransparentToggleStopsTapRouting()
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

    [Test]
    [Category(UITestCategories.Layout)]
    public void BackgroundColorToggleDoesNotChangeInputTransparentState()
    {
        App.WaitForElement("State: InputTransparent=False");
        App.WaitForElement("ToggleBackgroundColorsButton");
        App.Tap("ToggleBackgroundColorsButton");
        App.WaitForElement("Background colors: Toggled");
        App.WaitForElement("State: InputTransparent=False");
        Assert.That(App.FindElement("TapCountLabel").GetText(), Is.EqualTo("Tap count: 0"));
    }
}
