using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue4684 : _IssuesUITest
{
	public Issue4684(TestDevice testDevice) : base(testDevice)
	{
	}
#if ANDROID
    const string connect="CONNECT";
    const string control="CONTROL";
#else
	const string connect = "Connect";
	const string control = "Control";
#endif
	public override string Issue => "[Android] don't clear shell content because native page isn't visible";

	[Test]
	[Category(UITestCategories.Shell)]
	public void NavigatingBackAndForthDoesNotCrash()
	{
		App.TapInShellFlyout("Connect");
		App.WaitForElementTillPageNavigationSettled("Connect");
		TapTobTab(control);
		App.WaitForElementTillPageNavigationSettled("Control");
		App.TapInShellFlyout("Home");
		App.WaitForElementTillPageNavigationSettled("Control");
		App.TapInShellFlyout("Connect");
		TapTobTab(connect);
		App.WaitForElement("Connect");
		TapTobTab(control);
		App.WaitForElement("Success");
	}

	void TapTobTab(string tab)
	{
#if WINDOWS
        App.Tap("navViewItem");
#endif
		App.Tap(tab);
	}
}