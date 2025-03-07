using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28078 : _IssuesUITest
{
	public Issue28078(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Crash occurs when click on flyout menu";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellFlyoutMenuCrash()
	{
		App.WaitForElement("button");
		App.Tap("button");
		App.WaitForElement("label");
		App.TapShellFlyoutIcon();
		VerifyScreenshot();
	}
}