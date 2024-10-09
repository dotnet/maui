#if __ANDROID__ // These tests don't work in iOS for unrelated reasons (see https://bugzilla.xamarin.com/show_bug.cgi?id=41085)
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

	public override string Issue => "[Android] IllegalStateException: Recursive entry to executePendingTransactions";

	// [Test]
	// public void ClickingOnMenuItemInRootDoesNotCrash_NavPageVersion()
	// {
	// 	App.Tap("StartNavPageTest");
	// 	App.WaitForElement("OpenRoot");

	// 	App.Tap("OpenRoot");
	// 	App.WaitForElement("ClickThisId");

	// 	App.Tap("ClickThisId");
	// 	App.WaitForElement("StillHereId"); // If the bug isn't fixed, the app will have crashed by now
	// }

	// [Test]
	// public void ClickingOnMenuItemInRootDoesNotCrash_TabPageVersion()
	// {
	// 	App.Tap("StartTabPageTest");
	// 	App.WaitForElement("OpenRoot");

	// 	App.Tap("OpenRoot");
	// 	App.WaitForElement("ClickThisId");

	// 	App.Tap("ClickThisId");
	// 	App.WaitForElement("StillHereId"); // If the bug isn't fixed, the app will have crashed by now
	// }
}
#endif