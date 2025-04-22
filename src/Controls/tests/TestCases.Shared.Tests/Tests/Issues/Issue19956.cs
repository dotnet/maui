#if IOS || (ANDROID && TEST_FAILS_ON_ANDROID)//android related issue: https://github.com/dotnet/maui/issues/27951
//The test is applicable only to mobile platforms like iOS and Android.
using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
using OpenQA.Selenium.Interactions;
using NUnit.Framework.Legacy;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Entry)]
public class Issue19956: _IssuesUITest
{
    public Issue19956(TestDevice device) : base(device) { }

    public override string Issue => "Sticky headers and bottom content insets";

    [Test]
	public void ContentAccountsForStickyHeaders()
    {
		// This is an iOS Keyboard Scrolling issue.

		var app = App as AppiumApp;

        if (app is null)
            return;

        var stickyHeader = App.WaitForElement("StickyHeader");
        var stickyHeaderRect = stickyHeader.GetRect();

		// Scroll to the bottom of the page
		App.DragCoordinates(5, 650, 5, 100);

        App.Tap("Entry12");
        ValidateEntryPosition("Entry12", app, stickyHeaderRect);
        ValidateEntryPosition("Entry1", app, stickyHeaderRect);
        ValidateEntryPosition("Entry2", app, stickyHeaderRect);
    }

    void ValidateEntryPosition (string entryName, AppiumApp app, Rectangle stickyHeaderRect)
    {
        var entryRect = App.WaitForElement(entryName).GetRect();
        var keyboardPos = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);

        ClassicAssert.AreEqual(App.WaitForElement("StickyHeader").GetRect(), stickyHeaderRect);
		ClassicAssert.Less(stickyHeaderRect.Bottom, entryRect.Top);
        ClassicAssert.NotNull(keyboardPos);
		ClassicAssert.Less(entryRect.Bottom, keyboardPos!.Value.Y);

        KeyboardScrolling.NextiOSKeyboardPress(app.Driver);
    }

    [Test]
    public void BottomInsetsSetCorrectly()
    {
        var app = App as AppiumApp;
        if (app is null)
        {
            return;
        }

        try
        {
            App.Tap("Entry5");
            ScrollToBottom(app);
            CheckForBottomEntry(app);
            KeyboardScrolling.NextiOSKeyboardPress(app.Driver);

            App.Tap("Entry10");
            ScrollToBottom(app);
            CheckForBottomEntry(app);
            KeyboardScrolling.NextiOSKeyboardPress(app.Driver);

            ScrollToBottom(app);
            CheckForBottomEntry(app);
        }
        finally
        {
            //Reset the app so other UITest is in a clean state
            Reset();
            FixtureSetup();
        }
    }

    static void ScrollToBottom(AppiumApp app)
	{
		OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);

		app.DragCoordinates(5, 300, 5, 450);

		app.DragCoordinates(5, 400, 5, 100);

		app.DragCoordinates(5, 400, 5, 100);

		app.DragCoordinates(5, 400, 5, 100);
    }

    void CheckForBottomEntry (AppiumApp app)
    {
        var bottomEntryRect = App.WaitForElement("Entry12").GetRect();
        var keyboardPosition = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);
        ClassicAssert.NotNull(keyboardPosition);
		ClassicAssert.Less(bottomEntryRect.Bottom, keyboardPosition!.Value.Y);
    }
}
#endif