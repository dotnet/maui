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
	//[FailsOnAndroidWhenRunningOnXamarinUITest]
	//public void Issue774TestsDismissActionSheetAfterRotation()
	//{
	//	App.Tap(q => q.Button("Show ActionSheet"));
	//	App.Screenshot("Show ActionSheet");

	//	App.SetOrientationLandscape();
	//	App.Screenshot("Rotate Device");

	//	// Wait for the action sheet element to show up
	//	App.WaitForElement(q => q.Marked("What's up"));

	//	var dismiss = App.Query("Dismiss");

	//	var target = dismiss.Length > 0 ? "Dismiss" : "Destroy";


	//	App.Tap(q => q.Marked(target));
	//	App.WaitForNoElement(q => q.Marked(target));

	//	App.Screenshot("Dismiss ActionSheet");

	//}

	//[TearDown]
	//public override void TearDown()
	//{
	//	App.SetOrientationPortrait();

	//	base.TearDown();
	//}
}