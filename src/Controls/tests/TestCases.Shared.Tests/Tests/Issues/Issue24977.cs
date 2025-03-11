#if IOS
using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
using System.Text;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue24977 : _IssuesUITest
{
    public Issue24977(TestDevice device) : base(device) { }

    public override string Issue => "Keyboard Scrolling in editors with Center or End VerticalTextAlignment is off";

    [Test]
    [Category(UITestCategories.Editor)]
    public void KeepEditorCursorAboveKeyboardWithVerticalAlignmentAsCenter()
    {
        TestKeepEditorCursorAboveKeyboard("CenterButton", true);
    }

    [Test]
    [Category(UITestCategories.Editor)]
    public void KeepEditorCursorAboveKeyboardWithVerticalAlignmentAsEnd()
    {
        TestKeepEditorCursorAboveKeyboard("EndButton");
    }

    void TestKeepEditorCursorAboveKeyboard(string buttonId, bool fromCenter = false)
    {
        App.WaitForElement(buttonId);
        App.Tap(buttonId);
        Thread.Sleep(1000);

        var app = App as AppiumApp;
        if (app == null)
            return;

        var editorRect = app.WaitForElement("IssueEditor").GetRect();
        app.Click("IssueEditor");

        var sb = new StringBuilder();
        for (int i = 1; i <= 20; i++)
        {
            sb.Append($"\n{i}");
        }

        app.EnterText("IssueEditor", sb.ToString());

        var keyboardLocation = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);

        var cursorLabel = app.WaitForElement("CursorHeightTracker").GetText();
        var cursorHeight1 = Convert.ToDouble(cursorLabel);
        try
        {
            if (keyboardLocation is Point keyboardPoint)
            {
                Assert.That(cursorHeight1 < keyboardPoint.Y);
            }
            else
            {
                Assert.Fail("keyboardLocation is null");
            }
        }
        finally
        {
            if (fromCenter)
            {
                App.Back();
            }
        }
    }
}
#endif