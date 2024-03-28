using System.Drawing;
using System.Reflection;
using Microsoft.Maui.AppiumTests;
using NUnit.Framework;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;
public class Issue19956 : _IssuesUITest
{
	public Issue19956(TestDevice device) : base(device) { }

	public override string Issue => "Sticky headers and bottom content insets";

	[Test]
	[Category(UITestCategories.Entry)]
	public void ContentAccountsForStickyHeaders()
	{
		this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows },
			"This is an iOS Keyboard Scrolling issue.");

		var app = App as AppiumApp;
		if (app is null)
			return;

		var stickyHeader = App.WaitForElement("StickyHeader");
		var stickyHeaderRect = stickyHeader.GetRect();

		// Scroll to the bottom of the page
		OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
		var scrollSequence = new ActionSequence(touchDevice, 0);
		scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, 5, 650, TimeSpan.Zero));
		scrollSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
		scrollSequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(500)));
		scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, 5, 100, TimeSpan.FromMilliseconds(250)));
		scrollSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
		app.Driver.PerformActions([scrollSequence]);

		app.Click("Entry12");
		ValidateEntryPosition("Entry12", app, stickyHeaderRect);
		ValidateEntryPosition("Entry1", app, stickyHeaderRect);
		ValidateEntryPosition("Entry2", app, stickyHeaderRect);
	}

	void ValidateEntryPosition(string entryName, AppiumApp app, Rectangle stickyHeaderRect)
	{
		var entryRect = App.WaitForElement(entryName).GetRect();
		var keyboardPos = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);

		Assert.AreEqual(App.WaitForElement("StickyHeader").GetRect(), stickyHeaderRect);
		Assert.Less(stickyHeaderRect.Bottom, entryRect.Top);
		Assert.NotNull(keyboardPos);
		Assert.Less(entryRect.Bottom, keyboardPos!.Value.Y);

		KeyboardScrolling.NextiOSKeyboardPress(app.Driver);
	}

	[Test]
	public void BottomInsetsSetCorrectly()
	{
		this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows },
			"This is an iOS Keyboard Scrolling issue.");

		var app = App as AppiumApp;
		if (app is null)
			return;

		app.Click("Entry5");
		ScrollToBottom(app);
		CheckForBottomEntry(app);
		KeyboardScrolling.NextiOSKeyboardPress(app.Driver);

		app.Click("Entry10");
		ScrollToBottom(app);
		CheckForBottomEntry(app);
		KeyboardScrolling.NextiOSKeyboardPress(app.Driver);

		ScrollToBottom(app);
		CheckForBottomEntry(app);
	}

	static void ScrollToBottom(AppiumApp app)
	{
		OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);

		var scrollSequence1 = new ActionSequence(touchDevice, 0);
		scrollSequence1.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, 5, 300, TimeSpan.Zero));
		scrollSequence1.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
		scrollSequence1.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(500)));
		scrollSequence1.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, 5, 450, TimeSpan.FromMilliseconds(250)));
		scrollSequence1.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
		app.Driver.PerformActions([scrollSequence1]);

		var scrollSequence2 = new ActionSequence(touchDevice, 0);
		scrollSequence2.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, 5, 400, TimeSpan.Zero));
		scrollSequence2.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
		scrollSequence2.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(500)));
		scrollSequence2.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, 5, 100, TimeSpan.FromMilliseconds(250)));
		scrollSequence2.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
		app.Driver.PerformActions([scrollSequence2]);

		var scrollSequence3 = new ActionSequence(touchDevice, 0);
		scrollSequence3.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, 5, 400, TimeSpan.Zero));
		scrollSequence3.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
		scrollSequence3.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(500)));
		scrollSequence3.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, 5, 100, TimeSpan.FromMilliseconds(250)));
		scrollSequence3.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
		app.Driver.PerformActions([scrollSequence3]);

		var scrollSequence4 = new ActionSequence(touchDevice, 0);
		scrollSequence4.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, 5, 400, TimeSpan.Zero));
		scrollSequence4.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
		scrollSequence4.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(500)));
		scrollSequence4.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, 5, 100, TimeSpan.FromMilliseconds(250)));
		scrollSequence4.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
		app.Driver.PerformActions([scrollSequence4]);
	}

	void CheckForBottomEntry(AppiumApp app)
	{
		var bottomEntryRect = App.WaitForElement("Entry12").GetRect();
		var keyboardPosition = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver);
		Assert.NotNull(keyboardPosition);
		Assert.Less(bottomEntryRect.Bottom, keyboardPosition!.Value.Y);
	}
}
