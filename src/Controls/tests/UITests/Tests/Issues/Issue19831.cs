using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.MultiTouch;
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
		var app = App as AppiumApp;

		if (app is null)
			return;

		var actions = new TouchAction(app.Driver);
		actions.Press(rect.CenterX(), rect.CenterY()).Wait(2000).Release().Perform();
		app.Click("button");

		// The test passes if the action mode menu is not visible
		VerifyScreenshot();
	}
}
