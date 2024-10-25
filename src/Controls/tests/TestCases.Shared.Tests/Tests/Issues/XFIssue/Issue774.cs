using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue774 : _IssuesUITest
{
	public Issue774(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ActionSheet won't dismiss after rotation to landscape";

	//[Test]
	//[Category(UITestCategories.ActionSheet)]
	//[FailsOnAndroid]
	//public void Issue774TestsDismissActionSheetAfterRotation()
	//{
	//	RunningApp.Tap(q => q.Button("Show ActionSheet"));
	//	RunningApp.Screenshot("Show ActionSheet");

	//	RunningApp.SetOrientationLandscape();
	//	RunningApp.Screenshot("Rotate Device");

	//	// Wait for the action sheet element to show up
	//	RunningApp.WaitForElement(q => q.Marked("What's up"));

	//	var dismiss = RunningApp.Query("Dismiss");

	//	var target = dismiss.Length > 0 ? "Dismiss" : "Destroy";


	//	RunningApp.Tap(q => q.Marked(target));
	//	RunningApp.WaitForNoElement(q => q.Marked(target));

	//	RunningApp.Screenshot("Dismiss ActionSheet");

	//}

	//[TearDown]
	//public override void TearDown()
	//{
	//	RunningApp.SetOrientationPortrait();

	//	base.TearDown();
	//}
}