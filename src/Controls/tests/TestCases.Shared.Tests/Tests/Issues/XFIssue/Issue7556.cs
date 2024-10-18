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
	//	if (!RunningApp.IsTablet())
	//		return;

	//	RunningApp.SetOrientationLandscape();
	//	RunningApp.WaitForElement("Split");
	//	RunningApp.WaitForElement("Flyout Visible");
	//	RunningApp.Tap("PushModalPage");
	//	RunningApp.Tap("PopModalPage");
	//	RunningApp.WaitForElement("Flyout Visible");
	//}

	//[Test]
	//public void SplitOnLandscapeFailsToDetectClose()
	//{
	//	if (!RunningApp.IsTablet())
	//		return;

	//	while (RunningApp.WaitForElement("CurrentMasterBehavior")[0].ReadText() != FlyoutLayoutBehavior.SplitOnLandscape.ToString())
	//	{
	//		RunningApp.Tap("ChangeMasterBehavior");

	//		if (RunningApp.Query("Flyout Visible").Length > 0)
	//			RunningApp.Tap("Close Flyout");
	//	}

	//	RunningApp.Tap("Flyout");
	//	RunningApp.WaitForElement("Flyout Visible");
	//	RunningApp.Tap("Close Flyout");

	//	RunningApp.SetOrientationLandscape();
	//	RunningApp.SetOrientationPortrait();
	//	RunningApp.SetOrientationLandscape();
	//	RunningApp.SetOrientationPortrait();

	//	if (RunningApp.Query("Flyout Visible").Length > 0)
	//		RunningApp.Tap("Close Flyout");

	//	RunningApp.Tap("Flyout");
	//	RunningApp.WaitForElement("Flyout Visible");
	//	RunningApp.Tap("Close Flyout");
	//	RunningApp.Tap("Flyout");
	//	RunningApp.WaitForElement("Flyout Visible");
	//}

	//[TearDown]
	//public override void TearDown()
	//{
	//	RunningApp.SetOrientationPortrait();
	//	base.TearDown();
	//}
}