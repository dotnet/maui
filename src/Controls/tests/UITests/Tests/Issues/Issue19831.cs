using System.Drawing;
using NUnit.Framework;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue19831 : _IssuesUITest
{
	public override string Issue => "[Android] Action mode menu doesn't disappear when switch on another tab";

	public Issue19831(TestDevice device)
		: base(device)
	{ }

    [Test]
	public void ActionModeMenuShouldNotBeVisibleAfterSwitchingTab()
	{
		this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows });

		var rect = App.WaitForElement("Item1").GetRect();
	
		PerformLongPress(rect);
		App.Click("button");

		// The test passes if the action mode menu is not visible
		VerifyScreenshot();
	}

	void PerformLongPress(Rectangle rect)
	{
		if (App is not AppiumApp app)
			return;

		int xPos = rect.CenterX();
		int yPos = rect.CenterY();
		OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
		var longPress = new ActionSequence(touchDevice, 0);

		longPress.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, xPos, yPos, TimeSpan.Zero));
		longPress.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
		longPress.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, xPos, yPos, TimeSpan.FromMilliseconds(2000)));
		longPress.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));

		app.Driver.PerformActions(new List<ActionSequence> { longPress });
	}
}
