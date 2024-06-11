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
	public void ActionModeMenuShouldNotBeVisibleAfterSwitchingTab()
	{
		_ = App.WaitForElement("Item1");
		App.LongPress("Item1");
		App.Click("button");

		// The test passes if the action mode menu is not visible
		VerifyScreenshot();
	}
}
#endif