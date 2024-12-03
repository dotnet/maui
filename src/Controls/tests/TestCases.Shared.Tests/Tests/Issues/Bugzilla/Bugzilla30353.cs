#if !WINDOWS // Setting orientation is not supported on Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla30353 : _IssuesUITest
{
	public Bugzilla30353(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "FlyoutPage.IsPresentedChanged is not raised";

	// There is 2 "Toggle" in the UI, which is which? They also need AutomationIds
	// [Test]
	// [FailsOnMacWhenRunningOnXamarinUITest]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// [Category(UITestCategories.FlyoutPage)]
	// public void FlyoutPageIsPresentedChangedRaised()
	// {
	// 	App.SetOrientationPortrait();
	// 	App.Screenshot("Portrait");
	// 	App.Tap("Toggle");
	// 	App.Screenshot("Portrait Visible");
	// 	App.WaitForElement("The Flyout is now visible");
	// 	App.Back();
	// 	App.Screenshot("Portrait Invisible");
	// 	App.WaitForElement("The Flyout is now invisible");
	// 	App.SetOrientationLandscape();
	// 	App.Screenshot("Landscape Invisible");
	// 	App.WaitForElement("The Flyout is now invisible");
	// 	App.Tap("Toggle");
	// 	App.Screenshot("Landscape Visible");
	// 	App.WaitForElement("The Flyout is now visible");
	// 	App.Back();
	// 	App.Screenshot("Landscape InVisible");
	// 	App.WaitForElement("The Flyout is now invisible");
	// 	App.SetOrientationPortrait();
	// 	App.Tap("Toggle");
	// 	App.Screenshot("Portrait Visible");
	// 	App.WaitForElement("The Flyout is now visible");
	// 	App.Back();
	// 	App.Screenshot("Portrait Invisible");
	// 	App.WaitForElement("The Flyout is now invisible");
	// 	App.SetOrientationLandscape();
	// }

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}
}
#endif