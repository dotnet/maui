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

		_ = App.WaitForElement("Item1");
		App.LongPress("Item1");
		App.Click("button");

		// The test passes if the action mode menu is not visible
		VerifyScreenshot();
	}
}
