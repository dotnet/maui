#if IOS
using System.Drawing;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;
using System.Text;
using OpenQA.Selenium;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue24977 : _IssuesUITest
{
    public Issue24977(TestDevice device) : base(device) { }

    public override string Issue => "Keyboard Scrolling in editors with Center or End VerticalTextAlignment is off";

    [Test]
    [Category(UITestCategories.Editor)]
    public void KeepEditorCursorAboveKeyboardWithVerticalAlignmentAsCenter()
    {
        var app = App as AppiumApp;
        if (app is null)
        {
            return;
        }

        var editorRect = app.WaitForElement("IssueEditor").GetRect();
        app.Click("IssueEditor");

        var sb = new StringBuilder();
        for (int i = 1; i <= 15; i++)
        {
            sb.Append($"\n{i}");
        }

        app.EnterText("IssueEditor", sb.ToString());

        var keyboardLocation = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);

        var cursorLabel = app.WaitForElement("CursorHeightTracker").GetText();

        var cursorHeight1 = Convert.ToDouble(cursorLabel);

        if (keyboardLocation is Point keyboardPoint)
        {            
            Assert.That(cursorHeight1 < keyboardPoint.Y);            
        }
        else
        {
            Assert.Fail("keyboardLocation is null");
        }
    }
}
#endif
