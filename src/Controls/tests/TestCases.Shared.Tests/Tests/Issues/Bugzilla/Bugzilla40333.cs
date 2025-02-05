using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.FlyoutPage)]
public class Bugzilla40333 : _IssuesUITest
{
	public Bugzilla40333(TestDevice testDevice) : base(testDevice)
	{
	}

	const string StartNavPageTestId = "StartNavPageTest";
	const string OpenRootId = "OpenRoot";
	const string StartTabPageTestId = "StartTabPageTest";
	const string StillHereId = "3 Still Here";
	const string ClickThisId = "2 Click This";
	public override string Issue => "[Android] IllegalStateException: Recursive entry to executePendingTransactions";

	[Test]
	public void ClickingOnMenuItemInRootDoesNotCrash_NavPageVersion()
	{
		App.WaitForElement(StartNavPageTestId);
		App.Tap(StartNavPageTestId);
		App.WaitForElement(OpenRootId);

		App.Tap(OpenRootId);
		App.WaitForElement(ClickThisId);

		App.Tap(ClickThisId);
		App.WaitForElement(StillHereId);
	}

	[Test]
	public void ClickingOnMenuItemInRootDoesNotCrash_TabPageVersion()
	{
		App.WaitForElement(StillHereId);
#if ANDROID || WINDOWS // On Android and Windows, two back navigation actions are needed because the back button's position is the same for both navigation and flyout pages. This requires a double navigation to return to the root page.
		App.TapBackArrow();
		App.WaitForElement(StillHereId);
#endif
		App.TapBackArrow();

		App.WaitForElement(StartTabPageTestId);
		App.Tap(StartTabPageTestId);
		App.WaitForElement(OpenRootId);

		App.Tap(OpenRootId);
		App.WaitForElement(ClickThisId);

		App.Tap(ClickThisId);
		App.WaitForElement(StillHereId);
	}
}




