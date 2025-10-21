#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19831 : _IssuesUITest
{
	public override string Issue => "[Android] Action mode menu doesn't disappear when switch on another tab";

	public Issue19831(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.ListView)]
	public void ActionModeMenuShouldNotBeVisibleAfterSwitchingTab()
	{
		_ = App.WaitForElement("Item1");

		// 1. Open a context menu.
		App.LongPress("Item1");

		// 2. Navigate to a different tab.
		App.Click("button");

		// 3. The test passes if the action mode menu is not visible.
		VerifyScreenshot();
	}
}
#endif