#if (IOS && TEST_FAILS_ON_IOS) || (ANDROID && TEST_FAILS_ON_ANDROID) // Android related issue: https://github.com/dotnet/maui/issues/27951, iOS related issue: https://github.com/dotnet/maui/issues/28760
//The test is applicable only to mobile platforms like iOS and Android.
using System.Drawing;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue19214 : _IssuesUITest
{
    public Issue19214(TestDevice device) : base(device) { }

    public override string Issue => "iOS Keyboard Scrolling ContentInset Tests";

    [Test]
    [Category(UITestCategories.Entry)]
    public void TestMultipleScrollViews ()
    {
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
        ClassicAssert.NotNull(queryRect, "Could not find the initial entry.");

        app.Click(queryEntry);
        queryRect = app.WaitForElement(queryEntry).GetRect();
        KeyboardScrolling.CheckIfViewAboveKeyboard(app, queryEntry, false);

        // Make sure we can scroll up to the top entry
        ScrollScrollView(app, queryRect, false);
        var topRect = app.WaitForElement(topEntry).GetRect();

        ConfirmVisible (app, topRect, scrollView, topEntry, true);

        // Scroll to the bottom of the ScrollView
        ScrollScrollView(app, topRect);

        // Make sure we get to the bottom of the ScrollView
        var bottomRect = app.WaitForElement(bottomEntry).GetRect();
        ConfirmVisible (app, bottomRect, scrollView, bottomEntry, false);

        // Scroll back up and make sure we can get all the way up
        ScrollScrollView(app, bottomRect, false);
        topRect = app.WaitForElement(topEntry).GetRect();
        ConfirmVisible (app, topRect, scrollView, topEntry, true);

        // Hide the keyboard
        KeyboardScrolling.HideKeyboard(app, app.Driver, false);
    }

    void ConfirmVisible (AppiumApp app, Rectangle rect, string scrollView, string entry, bool isTopField)
    {
        var scrollViewRect = app.WaitForElement(scrollView).GetRect();
        KeyboardScrolling.CheckIfViewAboveKeyboard(app, entry, false);
        // ClassicAssert.True(rect.Y > scrollViewRect.Y && rect.Bottom < scrollViewRect.Bottom, $"{entry} was not visible in {scrollView}");
        if (isTopField)
        {
            ClassicAssert.Greater(rect.Y, scrollViewRect.Y, $"rect.Y: {rect.Y} was not greater than scrollViewRect.Y: {scrollViewRect.Y}");
        }
        else
        {
            ClassicAssert.Less(rect.Bottom, scrollViewRect.Bottom, $"rect.Bottom: {rect.Bottom} was not less than scrollViewRect.Bottom: {scrollViewRect.Bottom}");
        }
    }

    void ScrollScrollView (AppiumApp app, Rectangle rect, bool scrollsDown = true)
    {
        var newY = scrollsDown ? rect.Y - 5000 : rect.Y + 5000;

        OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
		var scrollSequence = new ActionSequence(touchDevice, 0);
        if (scrollsDown)
        {
		    scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, rect.Left - 5, rect.Y, TimeSpan.Zero));
        }
        else
        {
            scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, rect.Left - 5, rect.Bottom, TimeSpan.Zero));
        }
		scrollSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
		scrollSequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(500)));
		scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, rect.Left - 5, newY, TimeSpan.FromMilliseconds(250)));
		scrollSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
		app.Driver.PerformActions([scrollSequence]);
    }
}
#endif
