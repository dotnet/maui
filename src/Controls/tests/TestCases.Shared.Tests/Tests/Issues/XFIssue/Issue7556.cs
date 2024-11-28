using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7556 : _IssuesUITest
{
	public Issue7556(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Masterbehavior.popover not being observed on iOS 13";

	// TODO: Marked as ManualReview in original test, can we somehow automate this?
	//[Test]
	//[Category(UITestCategories.FlyoutPage)]
	//public void MasterStillVisibleAfterPushingAndPoppingModalPage()
	//{
	//	if (!App.IsTablet())
	//		return;

	//	App.SetOrientationLandscape();
	//	App.WaitForElement("Split");
	//	App.WaitForElement("Flyout Visible");
	//	App.Tap("PushModalPage");
	//	App.Tap("PopModalPage");
	//	App.WaitForElement("Flyout Visible");
	//}

	//[Test]
	//public void SplitOnLandscapeFailsToDetectClose()
	//{
	//	if (!App.IsTablet())
	//		return;

	//	while (App.WaitForElement("CurrentMasterBehavior")[0].ReadText() != FlyoutLayoutBehavior.SplitOnLandscape.ToString())
	//	{
	//		App.Tap("ChangeMasterBehavior");

	//		if (App.Query("Flyout Visible").Length > 0)
	//			App.Tap("Close Flyout");
	//	}

	//	App.Tap("Flyout");
	//	App.WaitForElement("Flyout Visible");
	//	App.Tap("Close Flyout");

	//	App.SetOrientationLandscape();
	//	App.SetOrientationPortrait();
	//	App.SetOrientationLandscape();
	//	App.SetOrientationPortrait();

	//	if (App.Query("Flyout Visible").Length > 0)
	//		App.Tap("Close Flyout");

	//	App.Tap("Flyout");
	//	App.WaitForElement("Flyout Visible");
	//	App.Tap("Close Flyout");
	//	App.Tap("Flyout");
	//	App.WaitForElement("Flyout Visible");
	//}

	//[TearDown]
	//public override void TearDown()
	//{
	//	App.SetOrientationPortrait();
	//	base.TearDown();
	//}
}