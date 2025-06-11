using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue4684 : _IssuesUITest
{
	public Issue4684(TestDevice testDevice) : base(testDevice)
	{
	}

	const string connect = "Connect";
	const string control = "Control";
	public override string Issue => "[Android] don't clear shell content because native page isn't visible";

	[Test]
	[Category(UITestCategories.Shell)]
	public void NavigatingBackAndForthDoesNotCrash()
	{
		App.TapInShellFlyout("Connect");
		App.WaitForElementTillPageNavigationSettled("Connect");
		App.TapTab(control, true);
		App.WaitForElementTillPageNavigationSettled("Control");
		App.TapInShellFlyout("Home");
		App.WaitForElementTillPageNavigationSettled("Control");
		App.TapInShellFlyout("Connect");
		App.TapTab(connect, true);
		App.WaitForElement("Connect");
		App.TapTab(control, true);
		App.WaitForElement("Success");
	}
}