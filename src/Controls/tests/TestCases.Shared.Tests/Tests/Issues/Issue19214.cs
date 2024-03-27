using System.Drawing;
using NUnit.Framework;
using OpenQA.Selenium.Appium.MultiTouch;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;
public class Issue19214 : _IssuesUITest
{
    public Issue19214(TestDevice device) : base(device) { }

    public override string Issue => "iOS Keyboard Scrolling ContentInset Tests";

    [Test]
    [Category(UITestCategories.Entry)]
    public void TestMultipleScrollViews ()
    {
        this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows },
            "This is an iOS Keyboard Scrolling issue.");

        var app = App as AppiumApp;
        if (app is null)
        {
            return;
        }

        var topRectY = app.WaitForElement("TopEntry_1").GetRect().Y;
        var bottomRectY = app.WaitForElement("Entry2_3").GetRect().Y;

        for (int i = 1; i < 4; i++)
        {
            var topEntry = $"TopEntry_{i}";
            var bottomEntry = $"BottomEntry_{i}";
            var entryTwo = $"Entry2_{i}";
            var entryTen = $"Entry10_{i}";
            var scrollView = $"ScrollView_{i}";

            CheckInsets(app, topEntry, topEntry, bottomEntry, scrollView);
            CheckInsets(app, entryTwo, topEntry, bottomEntry, scrollView);
            CheckInsets(app, entryTen, topEntry, bottomEntry, scrollView, true);
        }       
    }

    void CheckInsets (AppiumApp app, string queryEntry, string topEntry, string bottomEntry, string scrollView, bool startFromBottom = false)
    {
        if (startFromBottom)
        {
            var startRect = app.WaitForElement(topEntry).GetRect();
            ScrollScrollView(app, startRect);
        }
        
         var queryRect = app.WaitForElement(queryEntry).GetRect();
         Assert.NotNull(queryRect, "Could not find the initial entry.");

        app.Click(queryEntry);
        queryRect = app.WaitForElement(queryEntry).GetRect();
        KeyboardScrolling.CheckIfViewAboveKeyboard(app, queryEntry, false);

        // Make sure we can scroll up to the top entry
        ScrollScrollView(app, queryRect, false);
        var topRect = app.WaitForElement(topEntry).GetRect();

        ConfirmVisible (app, topRect, scrollView, topEntry);

        // Scroll to the bottom of the ScrollView
        ScrollScrollView(app, topRect);

        // Make sure we get to the bottom of the ScrollView
        var bottomRect = app.WaitForElement(bottomEntry).GetRect();
        ConfirmVisible (app, bottomRect, scrollView, bottomEntry);

        // Scroll back up and make sure we can get all the way up
        ScrollScrollView(app, bottomRect, false);
        topRect = app.WaitForElement(topEntry).GetRect();
        ConfirmVisible (app, topRect, scrollView, topEntry);

        // Hide the keyboard
        KeyboardScrolling.HideKeyboard(app, app.Driver, false);
    }

    void ConfirmVisible (AppiumApp app, Rectangle rect, string scrollView, string entry)
    {
        var scrollViewRect = app.WaitForElement(scrollView).GetRect();
        KeyboardScrolling.CheckIfViewAboveKeyboard(app, entry, false);
        Assert.True(rect.Y > scrollViewRect.Y && rect.Bottom < scrollViewRect.Bottom, $"{entry} was not visible in {scrollView}");
    }

    void ScrollScrollView (AppiumApp app, Rectangle rect, bool scrollsDown = true)
    {
        var actions = new TouchAction(app.Driver);
        var newY = scrollsDown ? rect.Y - 1000 : rect.Y + 1000;
        actions.LongPress(null, rect.Left - 5, rect.Y).MoveTo(null, rect.Left - 5, newY).Release().Perform();
    }
}
