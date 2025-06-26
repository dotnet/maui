#if IOS //This sample is specific to the iOS platform and handles text input behavior by accessing the native UITextView.
using System.Drawing;
using Xunit;
using Xunit;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;
using System.Text;
using OpenQA.Selenium;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue19214_2 : _IssuesUITest
{
    public Issue19214_2(TestDevice device) : base(device) { }

    public override string Issue => "iOS Editor Cursor stays above keyboard - Top level Grid";

    [Fact]
    [Trait("Category", UITestCategories.Entry)]
    public void KeepEditorCursorAboveKeyboardInGrid ()
    {
        var app = App as AppiumApp;
        if (app is null)
        {
            return;
        }

        var editorRect = app.WaitForElement("IssueEditor").GetRect();
        app.Click("IssueEditor");

        var sb = new StringBuilder();
        for (int i = 1; i <= 30; i++)
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
            Assert.True(cursorHeight1 > 0);
            Assert.True(cursorHeight2 > 0);
            Assert.True(cursorHeight3 > 0);
            Assert.True(cursorHeight1 < keyboardPoint.Y);
            Assert.True(cursorHeight2 < keyboardPoint.Y);
            Assert.True(cursorHeight3 < keyboardPoint.Y);
        }
        else
        {
            throw new InvalidOperationException("keyboardLocation is null");
        }
    }
}
#endif
