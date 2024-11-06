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
    public void KeepEditorCursorAboveKeyboardWhenVerticalTextAlignmentIsCenter ()
    {
        var app = App as AppiumApp;
        if (app is null)
        {
            return;
        }

        var editorRect = app.WaitForElement("IssueEditor").GetRect();
        app.Click("IssueEditor");

        var sb = new StringBuilder();
        for (int i = 1; i <= 5; i++)
        {
            sb.Append($"\n{i}");
        }

        app.EnterText("IssueEditor", sb.ToString());

        var keyboardLocation = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);

        var cursorLabel = app.WaitForElement("CursorHeightTracker").GetText();

        var cursorHeight1 = Convert.ToDouble(cursorLabel);
        KeyboardScrolling.HideKeyboard(app, app.Driver, true);

        // Click a low spot on the editor
        var lowSpotY = editorRect.Y + editorRect.Height - 100;
        app.TapCoordinates(editorRect.X + 10, lowSpotY);

        app.EnterText("IssueEditor", "A");

        cursorLabel = app.WaitForElement("CursorHeightTracker").GetText();
        var cursorHeight2 = Convert.ToDouble(cursorLabel);

        app.EnterText("IssueEditor", sb.ToString());

        cursorLabel = app.WaitForElement("CursorHeightTracker").GetText();
        var cursorHeight3 = Convert.ToDouble(cursorLabel);

        if (keyboardLocation is Point keyboardPoint)
        {
            ClassicAssert.True(cursorHeight1 > 0);
            ClassicAssert.True(cursorHeight2 > 0);
            ClassicAssert.True(cursorHeight3 > 0);
            ClassicAssert.True(cursorHeight1 < keyboardPoint.Y);
            ClassicAssert.True(cursorHeight2 < keyboardPoint.Y);
            ClassicAssert.True(cursorHeight3 < keyboardPoint.Y);
        }
        else
        {
            ClassicAssert.Fail("keyboardLocation is null");
        }
    }
}
#endif
